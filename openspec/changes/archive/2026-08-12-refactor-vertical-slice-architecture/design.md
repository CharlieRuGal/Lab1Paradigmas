# Design: Refactor HackerRank1 to Vertical Slice Architecture

## Context

The current `lab1/verticalslice` branch (created from `main`) is a plain layered API: `Controllers/LibrariesController.cs` (partial), `Controllers/BooksController.cs` (empty), `Services/` (book methods + library delete are `NotImplementedException`), `DTO/`, and `Data/LibraryContext.cs` containing `Library`, `Book`, and the EF context. Runtime uses InMemory (`UseInMemoryDatabase("librarydb")`). Two test projects (`LibraryService.Integration.Test` in the solution, and an identical orphan copy under `IntegrationTest/`) act as the acceptance oracle via `WebApplicationFactory<Program>` + a swapped SQLite `LibraryContext`; per user decision these tests are **read-only**, so the namespaces they import must keep resolving. See proposal.md for the motivation.

Key constraints that shape this design:

- Tests `using LibraryService.WebAPI.Data;` reference `LibraryContext`, `Library`, `Book`; tests `using LibraryService.WebAPI.DTO;` reference `BookForm`. These namespaces/types MUST continue to exist, unchanged, for tests to compile untouched.
- Sibling branches (`lab1/layers`, `lab1/clean`) already solved the runtime/DB layer: `ConnectionStrings:DefaultConnection` in `appsettings.json` + `UseNpgsql(Configuration.GetConnectionString("DefaultConnection"))` in `Startup`, `Npgsql.EntityFrameworkCore.PostgreSQL` 6.0.0 + `Microsoft.EntityFrameworkCore.Design` 6.0.0, and an `InitialCreate` migration with the `Books.LibraryId -> Libraries.Id` FK configured `onDelete: Cascade`. This design reuses that pattern with **placeholder credentials only**.
- Sister branches also reveal the needed handling for the tests: they POST book payloads containing only `name` and must still get 201, which requires (a) `SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true` on `AddControllers`, and (b) normalizing a missing `category` to `string.Empty` so the NOT NULL column is satisfied.

## Goals / Non-Goals

**Goals:**

- Make the use case the primary unit of organization; each behavior's request, response, handler, endpoint, mapping, and validation live in one feature folder.
- Complete all library/book functionality while preserving the exact HTTP contract (routes, JSON shape/property names, Swagger, 200/201/204/404).
- Switch the runtime to PostgreSQL/Supabase via Npgsql, keep EF Core 6, add a single `InitialCreate` migration with cascade delete.
- Single `.csproj`, no MediatR/AutoMapper/FluentValidation/CQRS, manual mapping, no new assemblies.
- Keep the integration tests green and byte-for-byte unchanged.

**Non-Goals:**

- No CQRS, repositories, or domain/application/infrastructure layering.
- No new project/solution, no new endpoints beyond the existing contract (there is no GET-by-book-id route today, so none is added).
- No runtime schema auto-creation (`dotnet ef database update` is not executed); no schema beyond `Libraries`/`Books`.
- No credentials in any artifact (placeholders only).

## Decisions

### D1. Vertical-slice folder layout (feature per use case)
Each endpoint maps to its own slice; code that changes to deliver that one behavior is colocated. Names follow the multi-file shape from the request, with per-slice request/response models.

```
HackerRank1/
├── Features/
│   ├── Libraries/
│   │   ├── GetLibraries/         GET  /api/libraries
│   │   │   ├── GetLibrariesEndpoint.cs
│   │   │   ├── GetLibrariesHandler.cs
│   │   │   └── LibraryResponse.cs
│   │   ├── GetLibraryById/       GET  /api/libraries/{libraryId}
│   │   │   ├── GetLibraryByIdEndpoint.cs
│   │   │   ├── GetLibraryByIdHandler.cs
│   │   │   └── LibraryResponse.cs
│   │   ├── CreateLibrary/        POST /api/libraries
│   │   │   ├── CreateLibraryEndpoint.cs
│   │   │   ├── CreateLibraryHandler.cs
│   │   │   ├── CreateLibraryRequest.cs
│   │   │   └── LibraryResponse.cs
│   │   ├── UpdateLibrary/        PUT  /api/libraries/{libraryId}
│   │   │   ├── UpdateLibraryEndpoint.cs
│   │   │   ├── UpdateLibraryHandler.cs
│   │   │   ├── UpdateLibraryRequest.cs
│   │   │   └── LibraryResponse.cs
│   │   └── DeleteLibrary/        DELETE /api/libraries/{libraryId}
│   │       ├── DeleteLibraryEndpoint.cs
│   │       └── DeleteLibraryHandler.cs
│   └── Books/
│       ├── GetBooks/             GET  /api/libraries/{libraryId}/books
│       │   ├── GetBooksEndpoint.cs
│       │   ├── GetBooksHandler.cs
│       │   └── BookResponse.cs
│       ├── CreateBook/           POST /api/libraries/{libraryId}/books
│       │   ├── CreateBookEndpoint.cs
│       │   ├── CreateBookHandler.cs
│       │   ├── CreateBookRequest.cs
│       │   └── BookResponse.cs
│       ├── UpdateBook/           PUT  /api/libraries/{libraryId}/books/{bookId}
│       │   ├── UpdateBookEndpoint.cs
│       │   ├── UpdateBookHandler.cs
│       │   ├── UpdateBookRequest.cs
│       │   └── BookResponse.cs
│       └── DeleteBook/           DELETE /api/libraries/{libraryId}/books/{bookId}
│           ├── DeleteBookEndpoint.cs
│           └── DeleteBookHandler.cs
├── Shared/
│   ├── Data/
│   │   ├── LibraryContext.cs     (namespace LibraryService.WebAPI.Data - compat)
│   │   └── Migrations/           (InitialCreate migration + snapshot)
│   └── Models/
│       ├── Library.cs            (namespace LibraryService.WebAPI.Data - compat)
│       ├── Book.cs               (namespace LibraryService.WebAPI.Data - compat)
│       └── BookForm.cs           (namespace LibraryService.WebAPI.DTO - test compat)
├── Program.cs
├── Startup.cs
└── appsettings.json
```

### D2. Endpoint-per-slice controllers, not Minimal APIs and not resource controllers
Each slice contains a small `ControllerBase` endpoint class annotated `[ApiController]` + route, e.g. `CreateLibraryEndpoint` with `[Route("api/libraries")]` and `[HttpPost]`. Perserving `MapControllers()` wiring keeps `Program`/`Startup` untouched in spirit and reproduces the exact controller-generated Swagger contract with zero extra configuration.

- **Alternative rejected - one `LibrariesController`/`BooksController` per resource:** regroups by technical type/resource and forces a single class to reach across every use case; that is the old organization, not VSA.
- **Alternative rejected - Minimal API (`MapGet`/`MapPost`):** requires `AddEndpointsApiExplorer`, per-endpoint OpenAPI metadata, and a different startup shape to reconstruct the same Swagger contract; higher risk of contract drift.

### D3. Handlers use `LibraryContext` directly (no repository layer)
Each handler is a small class with an async method that executes its one use case against the injected `LibraryContext` (list, single, add, update, remove, each `SaveChangesAsync` where appropriate). No repository/unit-of-work abstraction, per the request ("VSA does not require a repository layer"). Handlers are registered in DI and injected into their own slice's endpoint. A slice never invokes another slice's handler.

### D4. Library/book scoping and 404 semantics
Book queries always filter on `LibraryId == routeLibraryId` (in addition to any `bookId`). If the route library does not exist, or the book does not exist **within that library**, the slice returns 404. Because filtering is library-scoped, a book owned by library A can never be reached through library B's routes (cross-library GET/PUT/DELETE → 404), satisfying the ownership rule and the specs.

### D5. Completing the missing behaviors
- `DeleteLibrary`: load by id (404 if absent), `Remove`, `SaveChanges` - cascade delete of its books is handled by the required `Books.LibraryId -> Libraries.Id` FK (Cascade) declared in the model/migration, and verified by the existing `TestDeleteLibrary` test.
- `GetBooks`: verify library exists (404), return books for that library.
- `CreateBook`: verify library, save with `Category = request.Category ?? string.Empty`, return 201.
- `UpdateBook`/`DeleteBook`: 404 when the library or the in-library book is missing; 204 on success.
- Library `Add/Update`: preserve existing 200 (POST) and 204 (PUT) plus 404 on missing target.

### D6. Validation handling for the test contract
`AddControllers(options => options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true)` so missing optional string fields (e.g. `category` in the book POST used by tests) are not rejected with 400. No validation framework is added.

### D7. Namespace/type compatibility with read-only tests
`Library`, `Book`, and `LibraryContext` remain in namespace `LibraryService.WebAPI.Data` (fields `Libraries`, `Books`, and entity shapes unchanged), and a small `BookForm` stays available in namespace `LibraryService.WebAPI.DTO` purely so the tests compile untouched. Physically the types live under `Shared/Models` and `Shared/Data`; only the namespaces are constrained by compatibility. `LibraryForm` no longer needs to exist (no test references it) and is dropped.

### D8. PostgreSQL/Supabase configuration + packages
- `Startup.ConfigureServices`: `services.AddDbContext<LibraryContext>(o => o.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")))`, preserving the established pattern from the sibling branches. The real credentials are never committed; `appsettings.json` gains a `ConnectionStrings:DefaultConnection` with `CHANGE_ME` placeholder values (host, database, user, password). A developer fills in their own Supabase pooler values locally.
- `HackerRank1.csproj`: add `Npgsql.EntityFrameworkCore.PostgreSQL` 6.0.0 and `Microsoft.EntityFrameworkCore.Design` 6.0.0 (`PrivateAssets=all`); remove `Microsoft.EntityFrameworkCore.InMemory` (runtime no longer uses it) and `EFCore.AutomaticMigrations` (its runtime auto-migration behavior would conflict with the explicit-migration workflow and obscure `dotnet ef migrations list`). Keep EF Core 6.0.0, Newtonsoft.Json (used by the compat `BookForm` attributes), Swashbuckle, and MSTest references.

### D9. Migrations and schema
Generate exactly one migration via `dotnet ef migrations add InitialCreate --project HackerRank1 --output-dir Shared/Data/Migrations`. It creates `Libraries` (Id, Name, Location) and `Books` (Id, Name, Category, LibraryId) with the cascade FK, matching the schema already produced in the sibling branches. The DB is NOT updated by the migration command pipeline during apply; verification uses `dotnet ef migrations list` to confirm only `InitialCreate` exists. No schema beyond these two tables.

## Risks / Trade-offs

- [Compatibility namespaces (`LibraryService.WebAPI.Data`/`.DTO`) differ from their physical folders] → Mitigation: documented here and in code layout; kept because it is the only way to keep the read-only tests untouched (explicit user decision).
- [Suppressing implicit required validation relaxes 400 behavior for null `name`/`location` payloads] → Mitigation: acceptable for an academic lab; matches the sibling branches' behavior and keeps the documented 201-on-book-create test contract; request JSON contract (property names) is unchanged.
- [SQLite (tests) vs PostgreSQL (runtime) provider semantics] → Mitigation: EF Core model drives both; the FK cascade required by the delete test is configured in the model/migration and already proven by the existing tests.
- [Supabase database may already contain objects from prior labs, so `migrations add`/`migrations list` could report drift] → Mitigation: apply does not generate/verify against the live DB beyond `migrations list`; DB apply is left to the user. Schema documented as fixed to `Libraries`/`Books`.
- [Removing `EFCore.AutomaticMigrations` alters runtime auto-migrate behavior] → Mitigation: intended - explicit, reproducible migrations are required; no runtime auto-migration.

## Migration Plan

1. Restructure source into `Features/**` + `Shared/**` while preserving namespaces/behavior (D1-D7).
2. Update `Startup.cs` to `UseNpgsql`, register slice handlers, keep Swagger, apply controller option (D6).
3. Update `appsettings.json` with placeholder `ConnectionStrings:DefaultConnection` and `HackerRank1.csproj` packages (D8).
4. Generate `InitialCreate` migration under `Shared/Data/Migrations` (D9).
5. Verify: `dotnet build` (0 errors), `dotnet test` (all pass; runs the solution's `LibraryService.Integration.Test`, which is provider-swapped and DB-independent), `dotnet ef migrations list` (only `InitialCreate`), API starts, Swagger loads, smoke-test all library and book endpoints including 200/201/204/404 and the cross-library 404 ownership check. Temporary Supabase records and processes from smoke tests are cleaned up. No commit/push.
6. Rollback: this is a git branch (`lab1/verticalslice`); reverting is `git checkout main` / discarding branch work. No migration is applied to the database during apply, so no DB rollback is required.

## Open Questions

- None that change the specs, approach, or task breakdown. Remaining unknowns (exact Supabase credentials, whether the DB already holds prior-lab objects) are operational and deferred to apply-time; placeholders are used so no artifact needs them.