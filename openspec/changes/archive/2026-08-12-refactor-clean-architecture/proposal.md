## Why

The `HackerRank1` Web API mixes persistence, DTOs, services, and controllers in one flat structure: `LibraryContext` and the `Library`/`Book` entities share a single file, services inject `LibraryContext` directly, the `BooksController` is empty, and several service methods are `NotImplementedException` stubs. This makes the project hard to test, extend, and maintain, and it cannot run against the intended PostgreSQL/Supabase backend. This change restructures the project into Clean Architecture layers inside the existing single project and completes the missing Library/Book operations, while preserving the public HTTP contract.

## What Changes

- **Introduce four Clean Architecture layers** inside the existing `HackerRank1` project, organized by folders and namespaces only: `Presentation` (controllers), `Application` (services, DTOs, abstractions), `Domain` (entities, domain rules), `Infrastructure` (EF Core, repositories, migrations). **No** new projects, `.csproj` files, solutions, or assemblies.
- **Extract `Library` and `Book` into `Domain/Entities`**, free of EF Core / ASP.NET Core / Npgsql / Newtonsoft dependencies.
- **Add repository abstractions** and implementations: `ILibraryRepository`, `IBookRepository` (application-facing abstractions) with EF Core-backed implementations in `Infrastructure/Repositories`.
- **Move services into `Application`** and rewire them to depend on repository abstractions instead of `LibraryContext`; add application service interfaces.
- **Move DTOs (`LibraryForm`, `BookForm`) into `Application/DTOs`** and have controllers use them with manual mapping — the JSON contract stays byte-for-byte identical.
- **Complete the missing functionality**: implement `BooksController` (GET/POST/PUT/DELETE), `LibrariesController.DELETE`, `LibraryService.Delete`, and all `BookService` stubs, with 200 / 201 / 204 / 404 semantics and books always scoped to the route `libraryId`.
- **Adopt PostgreSQL/Supabase persistence**: add the `Npgsql.EntityFrameworkCore.PostgreSQL` package, add a `DefaultConnection` connection string to `appsettings.json`, switch `Startup` to `UseNpgsql`, and generate the `InitialCreate` migration (the only migration). In-memory EF becomes the test-only provider.
- **Keep the single `LibraryService.Integration.Test` and orphan `IntegrationTest` projects building**; integration tests continue to supply their own in-memory `LibraryContext`, so they remain live against the DI-registered services.
- **No** AutoMapper, MediatR, CQRS, FluentValidation, or other architectural frameworks; manual mapping only.

## Capabilities

### New Capabilities
- `library-service`: The HTTP contract for the Library Service API (routes, payload shapes, and status-code semantics) that the Clean Architecture refactor must preserve while making all Library/Book endpoints functional.

### Modified Capabilities
<!-- None - no existing specs in the repository. -->

## Impact

- **Code restructured**: all `.cs` under `HackerRank1` are re-homed into `Presentation/`, `Application/`, `Domain/`, and `Infrastructure/`; `Program.cs`/`Startup.cs` registration updated.
- **New files**: `ILibraryRepository`, `IBookRepository` and EF Core implementations; domain entities; application DTOs and service interfaces; the `InitialCreate` migration + model snapshot.
- **Dependencies**: adds `Npgsql.EntityFrameworkCore.PostgreSQL` (and EF Core Design tooling for migrations) to `HackerRank1.csproj`; no other new packages.
- **Database**: connection string `DefaultConnection` added to `appsettings.json`; `InitialCreate` is the only migration and touches only `Libraries`/`Books` tables with the existing schema (`PK_Libraries`, `PK_Books`, `FK_Books_Libraries_LibraryId`).
- **Tests**: `LibraryService.Integration.Test` (in-solution) keeps its in-memory SQLite/EF setup; orphan `IntegrationTest/` is a copy and out of scope.