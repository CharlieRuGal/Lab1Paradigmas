## 1. Domain layer

- [x] 1.1 Create `HackerRank1/Domain/Entities/Library.cs` (namespace `LibraryService.WebAPI.Domain.Entities`) with `Id`, `Name`, `Location` and BCL `[Key]`; no EF/ASP.NET/Npgsql/Newtonsoft usings
- [x] 1.2 Create `HackerRank1/Domain/Entities/Book.cs` with `Id`, `Name`, `Category`, `LibraryId`, `virtual Library Library`; no EF/ASP.NET/Npgsql/Newtonsoft usings
- [x] 1.3 Delete old entity declarations from `HackerRank1/Data/LibraryContext.cs` (moved to Domain)

## 2. Application layer - abstractions and DTOs

- [x] 2.1 Create `HackerRank1/Application/Interfaces/ILibraryRepository.cs` (namespace `LibraryService.WebAPI.Application.Interfaces`) with `Get(int[] ids)`, `Add`, `AddRange`, `Update`, `Delete` using Domain entities
- [x] 2.2 Create `HackerRank1/Application/Interfaces/IBookRepository.cs` with `Get(int libraryId, int[] ids)`, `Add`, `Update`, `Delete` using Domain entities
- [x] 2.3 Move `HackerRank1/DTO/LibraryForm.cs` to `HackerRank1/Application/DTOs/LibraryForm.cs`; namespace `LibraryService.WebAPI.Application.DTOs`; keep properties/`[JsonProperty]` unaltered
- [x] 2.4 Move `HackerRank1/DTO/BookForm.cs` to `HackerRank1/Application/DTOs/BookForm.cs`; same treatment
- [x] 2.5 Create `HackerRank1/Application/Interfaces/ILibrariesService.cs` and `IBooksService.cs` (namespaces `LibraryService.WebAPI.Application.Interfaces`) with the existing service signatures

## 3. Application layer - services

- [x] 3.1 Move `HackerRank1/Services/LibraryService.cs` to `HackerRank1/Application/Services/LibrariesService.cs`; namespace `LibraryService.WebAPI.Application.Services`; inject `ILibraryRepository` (not `LibraryContext`); implement `Delete` via repository
- [x] 3.2 Move `HackerRank1/Services/BookService.cs` to `HackerRank1/Application/Services/BooksService.cs`; inject `IBookRepository`; implement `Get`/`Add`/`Update`/`Delete` (stubs removed) with library-scoped lookups
- [x] 3.3 Delete old `HackerRank1/Services/` folder after the Move completes (Service interfaces now live in Application/Interfaces)

## 4. Infrastructure layer - persistence

- [x] 4.1 Move `HackerRank1/Data/LibraryContext.cs` to `HackerRank1/Infrastructure/Data/LibraryContext.cs`; namespace `LibraryService.WebAPI.Infrastructure.Data`; now references `Domain.Entities`; add explicit Book→Library relationship with `.OnDelete(DeleteBehavior.Cascade)` in `OnModelCreating`
- [x] 4.2 Create `HackerRank1/Infrastructure/Repositories/LibraryRepository.cs` implementing `ILibraryRepository` over `LibraryContext` (including cascade delete of books via model behavior / manual book removal)
- [x] 4.3 Create `HackerRank1/Infrastructure/Repositories/BookRepository.cs` implementing `IBookRepository` over `LibraryContext` with library-scoped queries (`WHERE LibraryId == libraryId`)

## 5. Presentation layer - controllers

- [x] 5.1 Move `HackerRank1/Controllers/LibrariesController.cs` to `HackerRank1/Presentation/Controllers/LibrariesController.cs`; namespace `LibraryService.WebAPI.Presentation.Controllers`; accept/return DTOs (`LibraryForm`) with manual mapping; implement DELETE → 204/404; do not reference `LibraryContext`
- [x] 5.2 Create/complete `HackerRank1/Presentation/Controllers/BooksController.cs` implementing GET (200/404, empty list ok), POST (201/404, route `libraryId` authoritative), PUT (204/404), DELETE (204/404), all library-scoped and DTO-based, manual mapping, no EF queries
- [x] 5.3 Verify no controller references `LibraryContext` or `Microsoft.EntityFrameworkCore`

## 6. Wiring, project, and configuration

- [x] 6.1 Add `Npgsql.EntityFrameworkCore.PostgreSQL` 6.0.0 and `Microsoft.EntityFrameworkCore.Design` 6.0.0 (PrivateAssets) to `HackerRank1.csproj`; remove no existing packages
- [x] 6.2 Add `ConnectionStrings:DefaultConnection` to `HackerRank1/appsettings.json` using the existing Supabase endpoint/config (credential not reproduced in this change; recommend `appsettings.Development.json`/user-secrets override)
- [x] 6.3 Update `HackerRank1/Startup.cs`: register transient `ILibraryRepository→LibraryRepository` and `IBookRepository→BookRepository`; switch `LibraryContext` to `options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection"))`; keep controllers + Swagger unchanged
- [x] 6.4 Confirm `Program.cs` still uses `UseStartup<Startup>` and namespaces resolve

## 7. Migration generation

- [x] 7.1 Run `dotnet ef migrations add InitialCreate` (startup project `HackerRank1`) targeting `LibraryContext`; verify scaffold lands in `HackerRank1/Infrastructure/Migrations` with namespace `LibraryService.WebAPI.Infrastructure.Migrations`
- [x] 7.2 Confirm the generated migration creates only `Libraries` and `Books` with `PK_Libraries`, `PK_Books`, `FK_Books_Libraries_LibraryId`, and `ON DELETE CASCADE` on the FK
- [x] 7.3 Run `dotnet ef migrations list` and confirm `InitialCreate` is the only migration

## 8. Test projects

- [x] 8.1 Update only the `using` directives in `LibraryService.Integration.Test/IntegrationTest.cs` from `LibraryService.WebAPI.Data` and `LibraryService.WebAPI.DTO` to the new namespaces (`Domain.Entities`, `Infrastructure.Data`, `Application.DTOs`); no test-logic changes
- [x] 8.2 Mirror the same using updates in the orphan `IntegrationTest/IntegrationTest.cs` (kept building; otherwise out of scope)

## 9. Verification

- [x] 9.1 Run `dotnet build` on the solution and fix until 0 compilation errors
- [x] 9.2 Run `dotnet test` and confirm the in-solution `LibraryService.Integration.Test` tests pass (POST book 201, GET books 200/404, DELETE library 204/404)
- [x] 9.3 Run the dependency-rule grep gate (D8): no `Microsoft.EntityFrameworkCore`/`Npgsql`/`Newtonsoft`/`Infrastructure` usings in `Domain`; no `Infrastructure`/EF/Npgsql/Newtonsoft usings in `Application`; no `LibraryContext` or EF in controllers
- [x] 9.4 Start the API (with Supabase reachable) and load Swagger successfully
- [x] 9.5 Smoke-test all endpoints: `GET /api/libraries`, `GET /api/libraries/{id}`, `POST /api/libraries`, `PUT /api/libraries/{id}`, `DELETE /api/libraries/{id}`, `GET/POST/PUT/DELETE /api/libraries/{libraryId}/books...` asserting 200/201/204/404 and library-scoping (book under a different library returns 404)
- [x] 9.6 Clean up any temporary Supabase records created during smoke testing
- [x] 9.7 Stop any background API process after smoke testing
- [x] 9.8 Do not commit or push any changes
