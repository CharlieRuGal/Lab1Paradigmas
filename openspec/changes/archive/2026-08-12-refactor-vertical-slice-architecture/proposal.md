# Refactor HackerRank1 to Vertical Slice Architecture

## Why

The `HackerRank1` Web API (lab1/verticalslice branch, created from `main`) is organized by technical layer (`Controllers/`, `Services/`, `DTO/`, `Data/`), most endpoints are unimplemented (`BooksService`, library `Delete`, all book actions throw `NotImplementedException`), and the runtime database is the InMemory provider instead of the required PostgreSQL/Supabase. The laboratory requires a Vertical Slice Architecture refactor that keeps the public HTTP contract intact, completes the missing functionality, and passes the existing integration tests.

## What Changes

- Reorganize the single `HackerRank1` project around use cases: `Features/Libraries/<UseCase>` and `Features/Books/<UseCase>`, where each slice contains its request/response models, handler, endpoint, mapping, and validation together.
- Remove the technical-layer folders (`Controllers/`, `Services/`, `DTO/`) as the primary organization. No N-Layer or Clean Architecture structure.
- Add feature slices: `GetLibraries`, `GetLibraryById`, `CreateLibrary`, `UpdateLibrary`, `DeleteLibrary`, `GetBooks`, `CreateBook`, `UpdateBook`, `DeleteBook`.
- Implement the previously missing behavior: library `DELETE`, and book `GET`/`POST`/`PUT`/`DELETE`, all scoped to the route `libraryId`.
- Replace the runtime InMemory provider with PostgreSQL (Supabase) via `Npgsql` while keeping EF Core 6; add `Microsoft.EntityFrameworkCore.Design` and generate a single `InitialCreate` migration preserving the `Libraries` -> `Books` FK with cascade delete.
- Move cross-cutting infrastructure into a small shared area: `Shared/Data` (`LibraryContext`, migrations) and `Shared/Models` (`Library`, `Book`).
- Keep the exact HTTP contract: routes, request/response JSON and property names, Swagger contract, and status codes (200 / 201 / 204 / 404).
- No MediatR, AutoMapper, FluentValidation, CQRS, or new solutions/assemblies. Single `.csproj`, manual mapping.

## Capabilities

### New Capabilities
- `features/libraries`: Library use cases - list, get by id, create, update, delete - with the required status codes, 404 behavior, async execution, and cascade delete of books on library deletion.
- `features/books`: Book use cases scoped to a library - list, create, update, delete - with strict library scoping (cross-library operations return 404) and the required status codes.

### Modified Capabilities
- None. No existing specs exist; all library/book behavior is defined by the new capabilities above.

## Impact

- `HackerRank1` project: remove `Controllers/`, `Services/`, `DTO/`; add `Features/**` slices and `Shared/**`; update `Startup.cs` for `UseNpgsql`; add `ConnectionStrings:DefaultConnection` (placeholder only) in `appsettings.json`; adjust `HackerRank1.csproj` packages.
- Packages: add `Npgsql.EntityFrameworkCore.PostgreSQL` (EF 6 compatible) and `Microsoft.EntityFrameworkCore.Design`; remove runtime-only InMemory usage.
- Database: new `InitialCreate` migration for `Libraries` and `Books`; `dotnet ef database update` is NOT executed automatically.
- Tests: the two integration test projects (SQLite-based `WebApplicationFactory` harness) must keep passing; they replace `LibraryContext` themselves, so they are unaffected by the PostgreSQL switch.
- No public API, database schema, or solution-structure changes.