## Context

See proposal.md - Why. Current state relevant to the approach:

- Single assembly `HackerRank1` (net6.0) with mixed concerns. `LibraryContext` and the `Library`/`Book` entities share one file (`Data/LibraryContext.cs`); services (`Services/*.cs`) inject `LibraryContext` directly; controllers talk to services; DTOs (`DTO/*Form.cs`) exist but only `LibraryForm`/`BookForm` are present and carry Newtonsoft `[JsonProperty]` names. `BooksController` is empty; `LibrariesService.Delete`, `BookService.Get/Add/Update/Delete` are `NotImplementedException` stubs; `LibrariesController` POST returns `Ok` (200) and PUT returns 204.
- `Startup.cs` registers `ILibrariesService`/`IBooksService` as transient plus `LibraryContext` against an in-memory provider (`UseInMemoryDatabase`), controllers, and Swagger. There is **no** Npgsql package, no connection string, and no migrations on this branch (the `Migrations/` folder is empty).
- Adopted (user decision): this change adopts PostgreSQL/Supabase persistence — Npgsql package, a `DefaultConnection` connection string, and a generated `InitialCreate` migration. In-memory EF remains only as the integration-test provider, which injects its own in-context SQLite/in-memory `LibraryContext`.
- The in-solution test project `LibraryService.Integration.Test` imports `LibraryService.WebAPI.Data` and `LibraryService.WebAPI.DTO` and consumes `LibraryContext`/`Library`/`Book`/`BookForm`; the orphan `IntegrationTest/` is a duplicate copy. Tests replace the `LibraryContext` registration with an in-memory SQLite singleton.
- Spec contract to satisfy: `openspec/changes/refactor-clean-architecture/specs/library-service/spec.md` (endpoints, payload keys, 200/201/204/404 semantics, library-scoped books, cascade delete).

## Goals / Non-Goals

**Goals:**
- Four layers with dependencies pointing inward: Presentation → Application → Domain; Infrastructure implements Application abstractions (and uses Domain entities). Domain is free of EF Core / ASP.NET Core / Npgsql / Newtonsoft / Swagger / Infrastructure references.
- Single-project, folders-only layering: one Visual Studio project (`HackerRank1`), one assembly, no new solutions or `.csproj` files.
- Controllers free of EF queries and business logic; services depend on repository abstractions (in Application), never on `LibraryContext`; repositories own all EF access.
- Manual mapping only; no AutoMapper, MediatR, CQRS, FluentValidation, or any additional architectural framework; no extra NuGet packages beyond `Npgsql.EntityFrameworkCore.PostgreSQL` + EF Core Design tooling.
- All Library/Book operations functional with 200 / 201 / 204 / 404 semantics; books always scoped to the route `libraryId`; `InitialCreate` is the only migration and matches the existing `Libraries`/`Books` schema.
- Enforced, documented dependency rule (see D8) with a verification gate in tasks.

**Non-Goals:**
- Changing the external HTTP contract, payload keys, status codes, routes, or the DB schema.
- Splitting the solution or creating projects/assemblies.
- Introducing any new architectural/library abstraction beyond the repository interfaces.
- Overhauling test logic: tests are only re-pointed at new namespaces, behavior unchanged.
- Resolving the pre-existing Supabase credential management; the connection string is carried from the existing configuration approach and never reproduced in this change.

## Decisions

**D1. Folder/namespace layout (single project, single root namespace).**
Keep the existing `LibraryService.WebAPI` root namespace and add a per-layer sub-namespace, minimizing churn for the test project and snapshot:

```
HackerRank1/
├── Presentation/Controllers      → LibraryService.WebAPI.Presentation.Controllers
│     ├── LibrariesController.cs
│     └── BooksController.cs
├── Application/Interfaces        → LibraryService.WebAPI.Application.Interfaces
│     ├── ILibrariesService.cs
│     ├── IBooksService.cs
│     ├── ILibraryRepository.cs
│     └── IBookRepository.cs
├── Application/Services          → LibraryService.WebAPI.Application.Services
│     ├── LibrariesService.cs
│     └── BooksService.cs
├── Application/DTOs              → LibraryService.WebAPI.Application.DTOs
│     ├── LibraryForm.cs
│     └── BookForm.cs
├── Domain/Entities               → LibraryService.WebAPI.Domain.Entities
│     ├── Library.cs
│     └── Book.cs
├── Infrastructure/Data           → LibraryService.WebAPI.Infrastructure.Data
│     └── LibraryContext.cs
├── Infrastructure/Repositories   → LibraryService.WebAPI.Infrastructure.Repositories
│     ├── LibraryRepository.cs
│     └── BookRepository.cs
├── Infrastructure/Migrations     → LibraryService.WebAPI.Infrastructure.Migrations
│     ├── 2026..._InitialCreate.cs
│     ├── 2026..._InitialCreate.Designer.cs
│     └── LibraryContextModelSnapshot.cs
├── Program.cs                    → LibraryService.WebAPI (unchanged, uses Startup)
├── Startup.cs                    → LibraryService.WebAPI (DI + Npgsql + Swagger)
├── HackerRank1.csproj
└── appsettings.json
```

Rationale: preserves the existing root namespace so existing `LibraryService.WebAPI` references in tests and DI wire-up only need sub-namespace additions, not a wholesale rename. Alternatives rejected: flat top-level layer namespaces (too broad, collides with framework namespaces) and separate projects per layer (explicitly forbidden).

**D2. Domain entities extracted with zero infrastructure dependencies.**
`Book` and `Library` move verbatim into `Domain/Entities`; keep the same properties (`Id`, `Name`, `Location` / `Id`, `Name`, `Category`, `LibraryId`, `virtual Library Library`) and the BCL `[Key]` attribute (System.ComponentModel.DataAnnotations is framework, not EF). The files must not import EF Core, ASP.NET Core, Npgsql, Newtonsoft, or any `Infrastructure` namespace. No `LibraryContext` reference remains in Domain.

**D3. Repository abstractions live in Application; implementations in Infrastructure.**
`ILibraryRepository` and `IBookRepository` (methods mirroring today's service shapes: `Get(int[] ids)` / `Get(int libraryId, int[] ids)`, `Add`, `AddRange` (libraries only), `Update`, `Delete`) are declared in `Application/Interfaces` using Domain entities. `LibraryRepository` / `BookRepository` implement them in `Infrastructure/Repositories` over `LibraryContext`. Rationale: matches the user's requirement that Application contains repository abstractions and Infrastructure implements abstractions defined in Application, and keeps the dependency arrow Infrastructure → Application + Domain. Alternatives: interfaces in Domain (rejected — Application was specified as the home for repository abstractions); repositories injected into controllers (rejected — presentation must not touch persistence).

**D4. Application services retarget abstractions and complete the stubs.**
`ILibrariesService`/`IBooksService` and `LibrariesService`/`BooksService` move into `Application/Interfaces` + `Application/Services` with intact signatures, but the implementations now depend on the repository abstractions instead of `LibraryContext`. All previously-stubbed methods are implemented: `LibrariesService.Delete` removes the library (cascade via D9); `BooksService.Get/Add/Update/Delete` delegate to the book repository with `libraryId` scoping enforced at both service and repository bound.

**D5. DTOs in Application; controllers use them with manual mapping; JSON contract preserved.**
`LibraryForm`/`BookForm` move to `Application/DTOs`. Controllers accept/return DTOs and hand-map DTO↔entity in the controller (Presentation) layer. Contract preservation rationale: ASP.NET Core 6's default serializer is System.Text.Json with camelCase naming, so DTO property names `Id`/`Name`/`Location`/`Category`/`LibraryId` serialize to exactly `id`/`name`/`location`/`category`/`libraryId` — the same keys entities produced before. The Newtonsoft `[JsonProperty(...)]` attributes are inert under System.Text.Json but harmless and kept. The integration test deserializes GET books responses into the `Book` entity; since the JSON keys are unchanged (`id, name, category, libraryId`), the DTO→JSON bytes remain deserializable by the test. Status codes unchanged: GET → 200 / 404; POST library → 200 with created entity; POST book → 201 with created book; PUT → 204 / 404; DELETE → 204 / 404.

**D6. DI + Npgsql/Supabase configuration.**
`Startup.ConfigureServices`: register transient `ILibraryRepository→LibraryRepository` and `IBookRepository→BookRepository`, keep transient services, replace in-memory with `services.AddDbContext<LibraryContext>(o => o.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")))`, keep `AddControllers()` and Swagger exactly as-is. Add `ConnectionStrings:DefaultConnection` to `appsettings.json` matching the existing Supabase endpoint/config used by the project; recommend overriding the password-holding value via `appsettings.Development.json` / user-secrets so a live credential is never committed. `Program.cs` unchanged (still `UseStartup<Startup>`).

**D7. EF Core 6 + `InitialCreate` migration, schema-preserving.**
Add `Npgsql.EntityFrameworkCore.PostgreSQL` 6.0.0 and `Microsoft.EntityFrameworkCore.Design` 6.0.0 (PrivateAssets) to `HackerRank1.csproj` (no other packages). Generate one migration `InitialCreate` via `dotnet ef migrations add InitialCreate` once the refactor compiles; it materializes exactly the current model: `Libraries` (`Id`, `Name`, `Location`, `PK_Libraries`) and `Books` (`Id`, `Name`, `Category`, `LibraryId`, `PK_Books`, `FK_Books_Libraries_LibraryId`). Because EF uses the design-time host and the model only, generation does not connect to or modify the Supabase database; no second migration is ever produced. Verify with `dotnet ef migrations list` that `InitialCreate` is the only migration.

**D8. Dependency-rule enforcement (the Clean Architecture guard for a single project).**
With one assembly, enforcement is by namespace contract + verification gate, not compiler isolation:
- **Allowed references matrix** enforced by convention and reviewed at apply:
  - `Domain`: only BCL namespaces (System.*). **Forbidden**: `Microsoft.EntityFrameworkCore`, `Microsoft.AspNetCore.*`, `Microsoft.Extensions.*`, `Npgsql`, `Newtonsoft.*`, and any `LibraryService.WebAPI.Infrastructure*` / `Application` / `Presentation` using.
  - `Application`: `Domain` + BCL. **Forbidden**: `Microsoft.EntityFrameworkCore.*`, `Microsoft.AspNetCore.*`, `Npgsql`, `Newtonsoft.*`, `LibraryService.WebAPI.Infrastructure*`, `LibraryService.WebAPI.Presentation*`.
  - `Infrastructure`: `Domain`, `Application`, EF Core, Npgsql, Newtonsoft + BCL. **Forbidden**: `Microsoft.AspNetCore.*` and `LibraryService.WebAPI.Presentation*`.
  - `Presentation.Controllers`: `Application`, `Domain`, ASP.NET Core MVC + BCL. **Forbidden**: `LibraryService.WebAPI.Infrastructure*` (controllers must never see `LibraryContext`) and direct EF queries.
- **Verification gate in tasks.md**: a grep-based scan (`rg`/`Select-String`) confirms Domain and Application files contain none of the forbidden usings, and controllers contain no `LibraryContext`/`Microsoft.EntityFrameworkCore` references; plus `dotnet build` (the compiler catches cross-namespace impedance mismatches) and the integration tests.

**D9. Cascade delete configured in the model.**
In `LibraryContext.OnModelCreating`, configure the Book→Library relationship explicitly: `HasOne(b => b.Library).WithMany().HasForeignKey(b => b.LibraryId).OnDelete(DeleteBehavior.Cascade)`. Rationale: guarantees deleting a library removes its books in both the Npgsql schema (FK `ON DELETE CASCADE`) and the in-memory test provider, satisfying the *Books are cascade-deleted with their library* scenario. Library entity needs no `Books` collection; the explicit mapping makes the relationship unambiguous.

## Risks / Trade-offs

- [JSON contract drift between DTOs and entities changes Swagger schema / breaks tests] → DTO property names exactly mirror entity property names (camelCase output); verify request/response JSON before/after via Swagger and by running the integration tests.
- [Single-assembly enforcement is convention-based; someone could add a forbidden `using`] → Allowed-reference matrix in D8 plus the grep verification gate in tasks; keep it a documented, checked process for a lab setting rather than adding an analyzer package.
- [`dotnet ef migrations add` regenerates a different schema or a spurious second migration] → Generate once, then verify with `dotnet ef migrations list` (only `InitialCreate`) and `dotnet build`; revert via git if drift appears. The model is unchanged, so no drift is expected.
- [Npgsql connection unavailable or credential untracked breaks local run] → `DefaultConnection` in `appsettings.json` (and `appsettings.Development.json` override); the API must boot even if the DB is unreachable (EF lazily connects), Swagger still loads; DB-dependent operations are verified in smoke tests against Supabase.
- [Test project breaks on renamed namespaces (`Data`/`DTO`→`Domain.Entities`/`Infrastructure.Data`/`Application.DTOs`)] → Update only the `using` directives in the in-solution test file (and mirror with the orphan copy); test logic untouched.
- [Scope creep into adding a migration on existing Supabase data] → Migration is generated but never applied here; smoke-test records are cleaned up; no `dotnet ef database update` is part of this change.

## Migration Plan

1. Refactor is code-only: re-home files, add packages/connection config, generate `InitialCreate` (scaffold only, never applied).
2. Verify (see tasks.md): `dotnet build` (0 errors), `dotnet test` (in-solution integration tests pass), `dotnet ef migrations list` (only `InitialCreate`), API starts + Swagger loads, smoke-test all Library/Book endpoints asserting 200/201/204/404, clean up temporary Supabase records, stop the API process.
3. Rollback: `git stash`/revert the refactor commit; the Supabase database is never modified (no migration applied, only temporary test rows that are cleaned up), so the DB is unaffected in all cases.

## Open Questions

None that change the specs or approach. The exact value of the `DefaultConnection` string and how the credential is supplied (direct value vs env override) is resolved at implementation time from the existing Supabase configuration and is deliberately not reproduced in this change.