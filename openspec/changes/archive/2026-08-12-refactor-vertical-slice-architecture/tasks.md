# Tasks: Refactor HackerRank1 to Vertical Slice Architecture

## 1. Project setup and packages

- [x] 1.1 Confirm `dotnet build` on the current `lab1/verticalslice` branch compiles (baseline) and note its output.
- [x] 1.2 In `HackerRank1.csproj` add `Npgsql.EntityFrameworkCore.PostgreSQL` 6.0.0 and `Microsoft.EntityFrameworkCore.Design` 6.0.0 (`PrivateAssets=all`).
- [x] 1.3 In `HackerRank1.csproj` remove `Microsoft.EntityFrameworkCore.InMemory` and `EFCore.AutomaticMigrations`; keep EF Core 6.0.0, Newtonsoft.Json, Swashbuckle, MSTest references.
- [x] 1.4 Add `ConnectionStrings:DefaultConnection` to `appsettings.json` with `CHANGE_ME` placeholders only (no real credentials).

## 2. Shared infrastructure

- [x] 2.1 Move `Library` and `Book` entities into `Shared/Models/` keeping namespace `LibraryService.WebAPI.Data` (shapes and navigation unchanged).
- [x] 2.2 Move `LibraryContext` into `Shared/Data/` keeping namespace `LibraryService.WebAPI.Data` (`Libraries`, `Books` sets unchanged).
- [x] 2.3 Add a `BookForm` compatibility type in namespace `LibraryService.WebAPI.DTO` (e.g. `Shared/Models/BookForm.cs`) so the read-only tests still compile.
- [x] 2.4 Delete the old `Controllers/`, `Services/`, `DTO/` folders and their `NotImplementedException` stubs after the slices below replace them.

## 3. Libraries feature slices

- [x] 3.1 Create `Features/Libraries/GetLibraries/` with endpoint (GET /api/libraries -> 200 + array), handler (list all async), and `LibraryResponse`.
- [x] 3.2 Create `Features/Libraries/GetLibraryById/` with endpoint (GET /api/libraries/{libraryId} -> 200 / 404), handler, and `LibraryResponse`.
- [x] 3.3 Create `Features/Libraries/CreateLibrary/` with endpoint (POST /api/libraries -> 200 + created library), handler (add + SaveChanges), `CreateLibraryRequest`, and `LibraryResponse`.
- [x] 3.4 Create `Features/Libraries/UpdateLibrary/` with endpoint (PUT /api/libraries/{libraryId} -> 204 / 404), handler (update + SaveChanges), `UpdateLibraryRequest`, and `LibraryResponse`.
- [x] 3.5 Create `Features/Libraries/DeleteLibrary/` with endpoint (DELETE /api/libraries/{libraryId} -> 204 / 404), handler (remove + SaveChanges, cascade deletes books).

## 4. Books feature slices (strictly library-scoped)

- [x] 4.1 Create `Features/Books/GetBooks/` with endpoint (GET /api/libraries/{libraryId}/books -> 200 / 404), handler (library exists + books filtered by LibraryId), and `BookResponse`.
- [x] 4.2 Create `Features/Books/CreateBook/` with endpoint (POST /api/libraries/{libraryId}/books -> 201 / 404), handler (save with `Category = request.Category ?? string.Empty`, LibraryId from route), `CreateBookRequest`, and `BookResponse`.
- [x] 4.3 Create `Features/Books/UpdateBook/` with endpoint (PUT /api/libraries/{libraryId}/books/{bookId} -> 204 / 404), handler (book must belong to route library), `UpdateBookRequest`, and `BookResponse`.
- [x] 4.4 Create `Features/Books/DeleteBook/` with endpoint (DELETE /api/libraries/{libraryId}/books/{bookId} -> 204 / 404), handler (book must belong to route library).

## 5. Wiring, DI, and startup

- [x] 5.1 Register every slice handler in `Startup.ConfigureServices` (transient).
- [x] 5.2 Replace `UseInMemoryDatabase` with `UseNpgsql(Configuration.GetConnectionString("DefaultConnection"))` in `Startup.ConfigureServices`.
- [x] 5.3 Add `SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true` to `AddControllers` so book POSTs without `category` return 201, not 400.
- [x] 5.4 Keep Swagger (v1 doc + UI) and `MapControllers` wiring unchanged; confirm controller endpoints expose the same routes/verbs.

## 6. EF migrations and schema

- [x] 6.1 Generate the single migration: `dotnet ef migrations add InitialCreate --project HackerRank1 --output-dir Shared/Data/Migrations`.
- [x] 6.2 Verify the migration creates only `Libraries` and `Books` with the `Books.LibraryId` FK to `Libraries.Id` on delete cascade; do not run `dotnet ef database update`.
- [x] 6.3 Confirm `dotnet ef migrations list` shows exactly `InitialCreate` (pending/applied markers unavailable - DB access uses CHANGE_ME placeholders).

## 7. Verification

- [x] 7.1 Run `dotnet build` and confirm 0 compilation errors.
- [x] 7.2 Run `dotnet test` (solution -> `LibraryService.Integration.Test`) and confirm all existing integration tests pass.
- [x] 7.3 Start the API, confirm it launches and Swagger (`/swagger`) loads successfully.
- [ ] 7.4 Smoke-test all Library endpoints: GET list, GET by id, POST, PUT, DELETE, verifying 200 / 201 / 204 / 404 as applicable.
- [ ] 7.5 Smoke-test all Book endpoints pinned to a temp library, verifying 201 / 200 / 204 / 404 as applicable.
- [ ] 7.6 Verify library-scoped ownership: create a book under Library A, attempt read/update/delete through Library B, confirm 404.
- [ ] 7.7 Clean up all temporary Supabase records created during smoke tests and stop all temporary API processes.
- [x] 7.8 Do not commit or push any changes.