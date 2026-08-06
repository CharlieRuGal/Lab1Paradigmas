## Why

The LibraryService Web API currently mixes concerns: persistence entities (`Library`, `Book`) live inside `LibraryContext`, services talk to `LibraryContext` directly, controllers reference services loosely, and there are no repositories or DTOs. This makes the codebase hard to test, extend, and maintain. This change restructures the project into clean Presentation / Application / Domain / Infrastructure layers while preserving all existing behavior and endpoints.

## What Changes

- **Introduce a four-layer architecture** *inside the existing single `HackerRank1` project*: `Presentation` (controllers), `Application` (services + DTOs), `Domain` (entities + abstractions), `Infrastructure` (data, repositories, migrations), organized by folders only.
- **Move entities** `Library` and `Book` into the Domain layer, decoupled from EF Core and ASP.NET Core.
- **Add repository interfaces and implementations** for `Library` and `Book` persistence; services depend on repository abstractions instead of `LibraryContext` directly.
- **Add application service interfaces and DTOs** for the Library and Book use cases; controllers operate on DTOs, not persistence entities.
- **Re-register** services, repositories, and `LibraryContext` via dependency injection in `Startup.cs`.
- **Preserve** the existing HTTP endpoints, status-code semantics, request/response JSON contract, EF Core 6, PostgreSQL/Supabase via Npgsql, and the `InitialCreate` migration / schema.

Note on credentials: a live Supabase connection string currently sits in `appsettings.json`. This refactor does **not** change it, and no artifact in this change will reproduce it. The migration-preservation strategy guarantees the existing Supabase tables are not altered.

## Constraints

- Implement the layered architecture entirely inside the existing `HackerRank1` project.
- Do **not** split the solution into multiple `.csproj` projects.
- Keep a single Visual Studio project; organize code using folders only.
- Do **not** create additional solutions or assemblies.
- Only reorganize the existing code into `Presentation/`, `Application/`, `Domain/`, and `Infrastructure/` folders while preserving the current project structure. New types are limited to the few needed to complete the layering (repository interfaces/implementations) and stay inside `HackerRank1`.
- Do **not** introduce AutoMapper, MediatR, CQRS, FluentValidation, or any additional architectural framework.
- Use manual mapping only.
- Keep the existing request and response contract unchanged. If DTOs are introduced, they MUST preserve the same JSON schema and payload structure currently exposed by the API.
- Keep the implementation simple and appropriate for an academic laboratory.

## Capabilities

### New Capabilities
- `library-service`: The HTTP contract for the Library Service API (endpoints, payload shapes, and not-found semantics) that the layered refactor must preserve. Captures the externally observable behavior the new layered architecture must keep intact.

### Modified Capabilities
<!-- None - no existing specs in the repository. -->

## Impact

- **Code restructured**: `HackerRank1/Controllers`, `HackerRank1/DTO`, `HackerRank1/Data`, `HackerRank1/Migrations`, `HackerRank1/Services` are reorganized into `Presentation/`, `Application/`, `Domain/`, `Infrastructure/`.
- **New files**: repositories (`ILibraryRepository`, `IBookRepository` and implementations), domain entities, application DTOs, application service interfaces.
- **Moved files**: `LibraryContext`, entity models, migration models and the EF snapshot re-homed to `Infrastructure`.
- **Namespaces**: re-namespace moved types; `Program.cs`/`Startup.cs` registration points updated.
- **Saved migration**: the migration assembly's namespace that preserves tables (`Libraries`, `Books`) and their indexes/FKs is carried into `Infrastructure`.
- **No** changes to external dependencies, DB schema, endpoints, or rolling behavior.

## Migration-preservation strategy

Only moving namespaces and folders, **not** schema. The existing `...(InitialCreate).cs` migration enumerates `PK_Libraries`, `PK_Books`, and `FK_Books_Libraries_LibraryId`; as long as the entity model and table names (`Libraries`, `Books`) are unchanged, `dotnet ef migrations list` reports the migration as `applied`, and no new migration is generated. The invariant the design and tasks will respect is that the existing migration is kept and reused as-is, unchanged.

## Impact

- **Code**: all `.cs` under `HackerRank1` except `Program.cs` (unchanged) are relocated/reorganized; `Startup.cs` DI registrations updated.
- **Build**: `dotnet build`; `dotnet ef migrations list`; `dotnet test` for the in-solution `LibraryService.Integration.Test` project.
- **API**: endpoint and status-code fidelity preserved (200/201/204/404 semantics).
- **Credentials**: the pre-existing `appsettings.json` connection string is untouched; no credential is written by this change.