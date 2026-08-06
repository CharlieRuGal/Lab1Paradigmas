## Context

The layered architecture is in place (see `refactor-layered-architecture`). The repositories are fully implemented (`BookRepository`, `LibraryRepository`) and correctly scope queries to a library via `LibraryId`. The stubs are confined to the service and presentation layers:

- `BooksService` — all four methods throw `NotImplementedException`.
- `LibrariesService.Delete` — throws `NotImplementedException`.
- `BooksController` — has a constructor with `IBooksService` and `ILibrariesService` but no actions.
- `LibrariesController` — has `GET/POST/PUT` but no `DELETE`.

The acceptance harness (`LibraryService.Integration.Test`) runs against SQLite in-memory via `Startup` and asserts exact status codes (201 POST book, 200 GET books, 204 DELETE, 404 not-found) and that deleted libraries are gone.

## Goals / Non-Goals

**Goals:**
- Wire the existing repositories through the service interfaces and controllers with zero duplicated query logic.
- Enforce book-to-library ownership exclusively from the route's `libraryId` (no cross-library reads/writes).
- Preserve the exact route, JSON, and status-code contract already exercised by the integration tests.

**Non-Goals:**
- No schema, migration, package, or DI changes; no changes to repositories, entities, DTOs, or `Startup`.
- No new architectural patterns (no MediatR/AutoMapper/FluentValidation); manual mapping only, matching the existing controllers.

## Decisions

### D1. Services are thin passthroughs to the existing repositories
`BooksService` methods delegate directly to `IBookRepository` (e.g. `Get` → `_bookRepository.Get(libraryId, ids)`), and `LibrariesService.Delete` delegates to `ILibraryRepository.Delete`. Repositories already implement the correct filtering and `bool` return for delete.
- **Why:** avoids duplicating EF query logic; the constraint that EF queries live only in repositories is preserved.
- **Alternative considered:** re-implementing queries in services — rejected (duplication, violates layering).

### D2. Book ownership comes from the route, not the payload
Controllers construct `Book` entities from `BookForm` but set `LibraryId` from the route parameter `libraryId`, ignoring any `libraryId` sent in the body. Book reads/updates/deletes are resolved via `_booksService.Get(libraryId, ids)`, which filters by `LibraryId` in the repository.
- **Why:** the tests POST a `BookForm` with only `Name` and rely on the route to assign the library; it also prevents attaching books to arbitrary libraries.
- **Alternative considered:** trusting `BookForm.LibraryId` — rejected (cross-library risk).

### D3. Controllers gate on library existence through the service, never the context
For `GET/POST/PUT/DELETE` book actions, the controller first resolves the library with `_librariesService.Get(new[] { libraryId })` and returns 404 if absent. For `LibrariesController.Delete`, it resolves the library and calls `Delete`, returning 404 when the library does not exist.
- **Why:** matches the existing `LibrariesController` pattern and keeps controllers free of `LibraryContext` (constraint).
- **Alternative considered:** a dedicated `BookExists`/`LibraryExists` repository call — unnecessary; existing `Get` covers it.

### D4. Book DELETE/PUT not-found is scoped to the route library
`PUT/DELETE {bookId}` first verifies the book exists in the route library via `_booksService.Get(libraryId, new[] { bookId })`; if the resolved book is null the controller returns 404, regardless of whether a book with that id exists under a different library.
- **Why:** `BookRepository.Delete` matches by `Id` alone, so the controller-side scoped lookup is the enforcement point that prevents deleting another library's book.
- **Alternative considered:** hardening `BookRepository.Delete` to also match `LibraryId` — possible but unnecessary given the controller gate; noted as a defensive follow-up if scoping ever bypasses the controller.

### D5. Book responses are DTO-shaped
`BooksController` returns `BookForm` collections/objects (id/name/category/libraryId), mirroring how `LibrariesController` returns `LibraryForm`.
- **Why:** keeps persistence entities out of HTTP responses (refactor spec); the `[JsonProperty]` camelCase keys still deserialize into the `Book` entity the tests assert against.

### D6. DELETE library relies on FK cascade
`LibrariesController.Delete` returns 204 after the repository removes the library; the existing `Book.LibraryId` FK cascade removes its books (schema is unchanged).
- **Why:** the tests assert that GET books for a deleted library returns 404; cascade keeps the book rows consistent.

## Risks / Trade-offs

- **`BookRepository.Delete` matches by `Id` only** → Mitigated by D4: the controller never calls delete without first scoping the book lookup to the route library.
- **Status-code regression** → Mitigated by running `dotnet test` (integration harness) after implementation; no serializer or routing config is touched.
- **Body/route `libraryId` mismatch** → Mitigated by D2: the route value is always authoritative, so payload `libraryId` (or its absence) cannot create cross-library books.
- **Shared in-memory SQLite across tests** → Each test seeds its own libraries and uses ids 1-4; not affected by these changes.

## Migration Plan

No schema or configuration changes. Rollout is code-only: build, run the integration tests, verify Swagger still lists the new/updated endpoints. Rollback is reverting the controller/service edits (restores the stubbed baseline).

## Open Questions

None that affect the approach. Whether `POST /books` should return the created `BookForm` in the body is unasserted by the tests and can be settled during implementation.
