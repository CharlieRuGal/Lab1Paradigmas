## Context

See proposal.md - Why. Current state relevant to the approach:

- Single assembly `HackerRank1` (net6.0) with mixed concerns. `LibraryContext` and the `Library`/`Book` entities share one file (`Data/LibraryContext.cs`); services (`Services/*.cs`) inject `LibraryContext` directly; controllers talk to services; DTOs (`DTO/*Form.cs`) exist but are unused by controllers.
- `Startup.cs` registers `ILibrariesService`/`IBooksService` as transient plus `LibraryContext` (Npgsql → Supabase). Connection string (including a live password) sits in `appsettings.json`; it is untouched and never reproduced here.
- Migrations: `Migrations/...(InitialCreate).cs` + `LibraryContextModelSnapshot.cs`, both in namespace `HackerRank1.Migrations`. The migration itself only references table/column names — no CLR types. The snapshot references entity types by FQN (`LibraryService.WebAPI.Data.Book/.Library`) and `[DbContext(typeof(LibraryContext))]`.
- Test project `LibraryService.Integration.Test` (in-solution) imports `LibraryService.WebAPI.Data`, `LibraryService.WebAPI.DTO`, and the web `Program`/`Startup`; it builds an in-memory SQLite `LibraryContext`. Orphan `IntegrationTest/` is out of scope.
- Pre-existing gaps (stubbed `BooksService`, `LibrariesService.Delete`, empty `BooksController`) are **preserved as-is**; implementing them is a separate change.

## Goals / Non-Goals

**Goals:**
- Four layers with dependencies pointing inward: Presentation → Application → Domain, and Infrastructure implementing Domain abstractions.
- Single-project, folders-only layering: one Visual Studio project (`HackerRank1`), one assembly, no new solutions or `.csproj` files.
- Controllers free of business logic and EF queries; services depend on repository interfaces; repositories own EF access.
- Manual mapping only; no AutoMapper, MediatR, CQRS, FluentValidation, or any additional architectural framework.
- Preserve every HTTP route, status code, payload, the database schema, and the `InitialCreate` migration.
- Minimal, safe churn: prefer moving/adapting existing types over rewriting them; keep everything simple and appropriate for an academic laboratory.

**Non-Goals:**
- Implementing the stubbed book/library methods (existing behavior is kept, including `NotImplementedException`).
- Changing the data model, the DB schema, endpoints, or the JSON wire format.
- Creating new projects, solutions, or assemblies; splitting the solution in any way.
- Introducing AutoMapper, MediatR, CQRS, FluentValidation, or any other architectural framework; adding new NuGet packages.
- Fixing pre-existing test-framework hazards (`MSTest` `Using` in xUnit projects) or the orphan `IntegrationTest` project.

## Decisions

**D1. Folder/namespace layout.** Adopt the requested four folders with a single root namespace to minimize churn:

```
HackerRank1/
├── Presentation/Controllers      → LibraryService.WebAPI.Presentation.Controllers
├── Application/DTOs              → LibraryService.WebAPI.Application.DTOs
├── Application/Interfaces        → LibraryService.WebAPI.Application.Interfaces
├── Application/Services          → LibraryService.WebAPI.Application.Services
├── Domain/Entities               → LibraryService.WebAPI.Domain.Entities
├── Domain/Interfaces             → LibraryService.WebAPI.Domain.Interfaces
├── Infrastructure/Data           → LibraryService.WebAPI.Infrastructure.Data
├── Infrastructure/Repositories   → LibraryService.WebAPI.Infrastructure.Repositories
├── Infrastructure/Migrations     → LibraryService.WebAPI.Infrastructure.Migrations
├── Program.cs
├── Startup.cs
└── appsettings.json
```

Rationale: keeps the existing `LibraryService.WebAPI` root, so the test project and snapshot only need updated type references, not a wholesale rename. Alternatives considered: per-layer top-level namespaces (`Presentation`, `Application`, …) — rejected, too broad and collides with framework names; separate projects per layer — rejected outright by the constraints (single project, folders only, no new assemblies) and would break the WebApplicationFactory test setup.

**D2. Domain entities.** `Book` and `Library` move verbatim (same properties, `[Key]` kept) into `Domain/Entities`. Navigation `Book.Library` is kept. Domain has zero references to EF, ASP.NET, or Newtonsoft.

**D3. Repository interfaces live in Domain.** `ILibraryRepository` and `IBookRepository` (with `Task`-based methods matching today's service shapes) go in `Domain/Interfaces`. Infrastructure provides `LibraryRepository`/`BookRepository` implementing them on top of `LibraryContext`. Rationale: inverts the dependency so Application depends only on abstractions; alternatives (interfaces in Application) rejected because Infrastructure would then depend on Application.

**D4. Application services and DTOs.** Existing `ILibrariesService`/`LibrariesService` move to `Application/Interfaces` + `Application/Services` with their method signatures intact (`Get`, `Add`, `AddRange`, `Update`, `Delete`); the implementation now depends on `ILibraryRepository` instead of `LibraryContext`. Same for `IBooksService`/`BooksService`. `BookForm`/`LibraryForm` move to `Application/DTOs`; their `[JsonProperty]` camelCase names are kept so DTOs serialize to the exact same JSON keys (`id`, `name`, `location`, `category`, `libraryId`) as today's entity responses under ASP.NET Core's default camelCase JSON policy.

**D5. Controllers use DTOs without changing the contract.** `LibrariesController` and `BooksController` move to `Presentation/Controllers`. Controller actions accept/return the application DTOs (`LibraryForm`, `BookForm`) rather than entities, with manual DTO↔entity mapping done by hand in the controllers — no AutoMapper or any mapping library. The request and response JSON contract MUST remain byte-for-byte identical to today: the DTOs mirror the current payload keys exactly (the API serializes with ASP.NET Core's default camelCase policy, and the DTO property names already match it), so switching `LibrariesController`'s `POST`/`PUT` from the `Library` entity to `LibraryForm` changes the .NET type only, not the JSON on the wire. Verify contract parity with Swagger before/after.

**D6. DI registration.** In `Startup.ConfigureServices`, add transient repository registrations alongside the existing service registrations and keep the Npgsql `LibraryContext` registration and Swagger untouched.

**D7. Migration preservation.** Keep the `InitialCreate` migration file byte-identical (it references no CLR types). Re-home both migration files under `Infrastructure/Migrations` and update their namespaces to `LibraryService.WebAPI.Infrastructure.Migrations`. Hand-edit `LibraryContextModelSnapshot.cs` so its entity FQNs (`...Data.Book`, `...Data.Library`) and `[DbContext(typeof(LibraryContext))]` point at the new types — table names, columns, keys, FK, and index stay exactly as today. No new migration is generated; Supabase tables are untouched. Alternatives considered: regenerating the snapshot via `dotnet ef migrations remove`/`add` — rejected, risks dropping/rewriting the migration; leaving migrations in the old folder — rejected, breaks the layering.

## Risks / Trade-offs

- [DTO/entity mapping drifts the JSON contract (e.g., a missing property or mismatched key)] → DTOs mirror the current camelCase payload keys exactly; verify request/response JSON before/after via Swagger and the integration tests.
- [Snapshot hand-edit desyncs the model → EF generates an unwanted migration] → Verify with `dotnet ef migrations list` and an empty-migration check; revert to git if drift appears.
- [Test project breaks on renamed namespaces] → Update only its `using` directives; test logic untouched.
- [Connection-string password exposure] → No change; do not log, commit, or document it. Consider flagging the pre-existing `appsettings.json` credential for rotation separately.
- [Scope creep into implementing stubs] → Explicit Non-Goal; stubs stay identical after the move.

## Migration Plan

1. Refactor is code-only; no migration runs and no DB change occurs.
2. Verify (see tasks.md): `dotnet build`, `dotnet test`, `dotnet ef migrations list` (InitialCreate shown), Swagger smoke test.
3. Rollback: revert the refactor commit in git; DB is unaffected in all cases since no schema operation ever executes.

## Open Questions

None — decisions above fully determine the task breakdown.