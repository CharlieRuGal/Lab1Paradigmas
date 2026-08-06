## Why

The LibraryService API is a coding challenge whose scaffolding is complete but whose core behaviors are left stubbed: `BooksService` (all four methods), `LibrariesService.Delete`, and all `BooksController` actions throw `NotImplementedException` or are missing, and `LibrariesController` has no `DELETE` action. The graded integration tests (`LibraryService.Integration.Test\IntegrationTest.cs`) exercise these endpoints and currently fail with `InternalServerError` on `POST`/`GET /api/libraries/{id}/books` and on library delete. Until these behaviors are implemented, the API is not deliverable.

## What Changes

- Implement all of `BooksService` (`Get`, `Add`, `Update`, `Delete`), delegating to the already-implemented `BookRepository`.
- Implement `LibrariesService.Delete`, delegating to the already-implemented `LibraryRepository.Delete` (FK cascade removes the library's books).
- Implement `BooksController` actions scoped under `api/libraries/{libraryId}/books`:
  - `GET` returns 200 OK with the library's books (DTO-shaped), 404 Not Found when the library does not exist.
  - `POST` returns 201 Created when the library exists, 404 Not Found when it does not.
  - `PUT {bookId}` returns 204 No Content on success, 404 when the library or book does not exist.
  - `DELETE {bookId}` returns 204 No Content on success, 404 when the library or book does not exist.
- Add `LibrariesController` `DELETE /api/libraries/{libraryId}` returning 204 No Content when deleted, 404 Not Found when the library does not exist.
- Books always belong to the library in the route: the book's `LibraryId` is taken from the route, never cross-library; book lookups are scoped to the route library.

## Capabilities

### New Capabilities
- `library-service`: The HTTP surface for managing libraries and the books they contain. Covers listing/filtering, creating, updating, deleting libraries, and the CRUD of books within a library, including not-found semantics.

### Modified Capabilities
<!-- None - no existing main specs. -->

## Impact

- **Code**: `HackerRank1/Application/Services/BooksService.cs`, `HackerRank1/Application/Services/LibrariesService.cs`, `HackerRank1/Presentation/Controllers/BooksController.cs`, `HackerRank1/Presentation/Controllers/LibrariesController.cs`.
- **APIs**: Activates `GET/POST/PUT/DELETE` book endpoints and the `DELETE` library endpoint on the already-declared service interfaces.
- **Data**: Uses the existing `LibraryContext` (`Library`, `Book`) and existing repositories; no schema, migration, dependency, or package changes.
- **Tests**: `LibraryService.Integration.Test` (SQLite in-memory) is the acceptance harness.
