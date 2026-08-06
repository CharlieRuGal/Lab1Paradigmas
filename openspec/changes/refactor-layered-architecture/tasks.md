## 1. Domain Layer

- [x] 1.1 Create `HackerRank1/Domain/Entities/Library.cs` containing the `Library` entity (Id, Name, Location) in namespace `LibraryService.WebAPI.Domain.Entities`, moved verbatim from `Data/LibraryContext.cs`
- [x] 1.2 Create `HackerRank1/Domain/Entities/Book.cs` containing the `Book` entity (Id, Name, Category, LibraryId, Library nav) in namespace `LibraryService.WebAPI.Domain.Entities`, moved verbatim from `Data/LibraryContext.cs`
- [x] 1.3 Create `HackerRank1/Domain/Interfaces/ILibraryRepository.cs` in namespace `LibraryService.WebAPI.Domain.Interfaces` with Task-based persistence methods for `Library` (Get, Add, AddRange, Update, Delete) using only `Domain.Entities` types
- [x] 1.4 Create `HackerRank1/Domain/Interfaces/IBookRepository.cs` in namespace `LibraryService.WebAPI.Domain.Interfaces` with Task-based persistence methods for `Book` (Get, Add, Update, Delete) using only `Domain.Entities` types
- [x] 1.5 Remove the `Book` and `Library` class definitions from `HackerRank1/Data/LibraryContext.cs`, leaving only the `DbContext` (namespace `LibraryService.WebAPI.Data`) referencing `Domain.Entities`

## 2. Infrastructure Layer

- [x] 2.1 Create `HackerRank1/Infrastructure/Repositories/LibraryRepository.cs` implementing `ILibraryRepository` on `LibraryContext`, using async EF operations (mirror today's `LibrariesService` logic: filter by optional ids, add/addrange, update, delete)
- [x] 2.2 Create `HackerRank1/Infrastructure/Repositories/BookRepository.cs` implementing `IBookRepository` on `LibraryContext`, using async EF operations (query by libraryId and optional ids, add, update, delete)
- [x] 2.3 Update `LibraryContext` (namespace `LibraryService.WebAPI.Infrastructure.Data`) so `DbSet<Library>`/`DbSet<Book>` reference the moved domain entities; update its namespace to `LibraryService.WebAPI.Infrastructure.Data`
- [x] 2.4 Re-home `HackerRank1/Migrations/...InitialCreate.cs` to `HackerRank1/Infrastructure/Migrations/` byte-identical, updating only its namespace to `LibraryService.WebAPI.Infrastructure.Migrations`; do NOT change table, column, key, FK, or index definitions
- [x] 2.5 Re-home `HackerRank1/Migrations/LibraryContextModelSnapshot.cs` to `HackerRank1/Infrastructure/Migrations/`, updating its namespace to `LibraryService.WebAPI.Infrastructure.Migrations`, its `[DbContext(typeof(LibraryContext))]` to the new context type, and its entity FQNs (`...Data.Book`/`...Data.Library`) to the domain types; keep table/column configuration identical
- [x] 2.6 Verify no schema drift: run `dotnet ef migrations list` and confirm `InitialCreate` is still recognized with no new migration

## 3. Application Layer

- [x] 3.1 Move `BookForm.cs` and `LibraryForm.cs` to `HackerRank1/Application/DTOs/` with namespace `LibraryService.WebAPI.Application.DTOs`, keeping the `[JsonProperty]` camelCase names intact
- [x] 3.2 Create `HackerRank1/Application/Interfaces/ILibrariesService.cs` in namespace `LibraryService.WebAPI.Application.Interfaces` (move existing interface, unchanged signatures)
- [x] 3.3 Create `HackerRank1/Application/Interfaces/IBooksService.cs` in namespace `LibraryService.WebAPI.Application.Interfaces` (move existing interface, unchanged signatures)
- [x] 3.4 Move `LibrariesService` to `HackerRank1/Application/Services/` in namespace `LibraryService.WebAPI.Application.Services`, changing its dependency from `LibraryContext` to `ILibraryRepository` and delegating persistence accordingly
- [x] 3.5 Move `BooksService` to `HackerRank1/Application/Services/` in namespace `LibraryService.WebAPI.Application.Services`, changing its dependency from `LibraryContext` to `IBookRepository`; preserve the existing method stubs and their throw behavior exactly

## 4. Presentation Layer

- [x] 4.1 Move `LibrariesController.cs` to `HackerRank1/Presentation/Controllers/` with namespace `LibraryService.WebAPI.Presentation.Controllers`
- [x] 4.2 In `LibrariesController`, change `POST`/`PUT` to accept `LibraryForm` and map to a `Library` entity before calling the service; return the same status codes (200 OK / 204 No Content / 404 Not Found) and DTO responses whose JSON schema and payload keys exactly match the current contract (`id`, `name`, `location`)
- [x] 4.3 Move `BooksController.cs` to `HackerRank1/Presentation/Controllers/` with namespace `LibraryService.WebAPI.Presentation.Controllers`, keeping its existing route `api/libraries/{libraryId}/books` and behavior unchanged (stub/empty behavior preserved)

## 5. Dependency Injection

- [x] 5.1 In `Startup.cs`, register `ILibraryRepository`→`LibraryRepository` and `IBookRepository`→`BookRepository` as transient
- [x] 5.2 In `Startup.cs`, update service registrations to the new `Application` namespaces and ensure `LibraryContext` registration points at the relocated context; leave Swagger and Npgsql configuration untouched

## 6. Test Project Compatibility

- [x] 6.1 Update `LibraryService.Integration.Test/IntegrationTest.cs` `using` directives so the project compiles against the relocated types (`Domain.Entities`, `Application.DTOs`, `Infrastructure.Data`), without changing any test logic or assertions
- [x] 6.2 Confirm the orphan `IntegrationTest/` project is not referenced by the solution and requires no edits

## 7. Verification

- [x] 7.1 Run `dotnet build` on the solution and fix any compile errors introduced by the refactor
- [x] 7.2 Run `dotnet test` on `LibraryService.Integration.Test` and confirm tests behave as before the refactor
- [x] 7.3 Run `dotnet ef migrations list` and confirm `InitialCreate` shows as applied with no unexpected new migrations
- [x] 7.4 Run the API and smoke-test Swagger endpoints manually: list/create/update/delete library and list/add book, confirming status codes match the preserved contract
- [x] 7.4b Confirm request/response JSON payloads are identical before and after the refactor (same keys and value shapes for library and book payloads), so the request/response contract is unchanged
- [x] 7.5 Confirm no credentials were added, modified, logged, or committed anywhere in the change (grep the diff for the Supabase connection string content)
- [x] 7.6 Confirm the solution still contains only `HackerRank1` and `LibraryService.Integration.Test`, no new `.csproj`/`.sln` files or NuGet packages were added, and all changes live inside the single `HackerRank1` project (folders only)
