## 1. Implement BooksService

- [x] 1.1 Implement `Get(int libraryId, int[] ids)` to delegate to `_bookRepository.Get(libraryId, ids)`
- [x] 1.2 Implement `Add(Book book)` to delegate to `_bookRepository.Add(book)`
- [x] 1.3 Implement `Update(Book book)` to delegate to `_bookRepository.Update(book)`
- [x] 1.4 Implement `Delete(Book book)` to delegate to `_bookRepository.Delete(book)`

## 2. Implement LibrariesService.Delete

- [x] 2.1 Implement `Delete(Library library)` to delegate to `_libraryRepository.Delete(library)`

## 3. Implement BooksController

- [x] 3.1 Add `GET` action: resolve the route library via `_librariesService.Get(new[] { libraryId })`, return 404 if absent, else 200 OK with that library's books mapped to `BookForm`
- [x] 3.2 Add `POST` action: return 404 if the route library does not exist, else create a `Book` from the payload with `LibraryId` set from the route, persist via `_booksService.Add`, and return 201 Created with the created `BookForm`
- [x] 3.3 Add `PUT {bookId}` action: return 404 if the route library is absent or the book does not exist in that library, else update the book (keeping `LibraryId` from the route) via `_booksService.Update` and return 204 No Content
- [x] 3.4 Add `DELETE {bookId}` action: return 404 if the route library is absent or the book does not exist in that library, else delete via `_booksService.Delete` and return 204 No Content

## 4. Add LibrariesController.Delete

- [x] 4.1 Add `DELETE {libraryId}` action: return 404 if the library does not exist, else delete via `_librariesService.Delete` and return 204 No Content

## 5. Verify

- [x] 5.1 Run `dotnet build` and confirm 0 errors
- [x] 5.2 Run `dotnet test` and confirm all `LibraryService.Integration.Test` tests pass
- [x] 5.3 Run `dotnet ef migrations list` and confirm no new migrations were generated
- [x] 5.4 Smoke-test the endpoints against the running app (Swagger) and confirm routes/status codes behave as specified
- [x] 5.5 Clean up any test records created in the Supabase database during smoke tests
