# Features: Books

## Purpose

Defines the externally observable behavior of the book management API: books are always manipulated through a parent library, and every operation is strictly scoped to the library id in the route.

## ADDED Requirements

### Requirement: List books of a library
The system SHALL support listing the books of a library via `GET /api/libraries/{libraryId}/books`, returning HTTP 200 with a JSON array of books. Each book object SHALL expose the JSON property names `id`, `name`, `category`, and `libraryId`.

#### Scenario: Successful list for an existing library
- **WHEN** a client sends `GET /api/libraries/{libraryId}/books` for an existing library
- **THEN** the system responds with HTTP 200 and a JSON array of that library's books (empty array when the library has no books)

#### Scenario: List for a missing library
- **WHEN** a client sends `GET /api/libraries/{libraryId}/books` and no library with that id exists
- **THEN** the system responds with HTTP 404

### Requirement: Create a book in a library
The system SHALL support creating a book via `POST /api/libraries/{libraryId}/books` with a JSON body containing a book name, returning HTTP 201 on success with the created book JSON.

#### Scenario: Successful create
- **WHEN** a client sends `POST /api/libraries/{libraryId}/books` with a valid payload for an existing library
- **THEN** the system persists the book scoped to that library and responds with HTTP 201 and the created book JSON

#### Scenario: Create for a missing library
- **WHEN** a client sends `POST /api/libraries/{libraryId}/books` and no library with that id exists
- **THEN** the system responds with HTTP 404

### Requirement: Update a book in a library
The system SHALL support updating a book via `PUT /api/libraries/{libraryId}/books/{bookId}`, returning HTTP 204 on success.

#### Scenario: Successful update
- **WHEN** a client sends `PUT /api/libraries/{libraryId}/books/{bookId}` for a book that belongs to that library
- **THEN** the system updates the book and responds with HTTP 204

#### Scenario: Update for a missing library or book
- **WHEN** a client sends `PUT /api/libraries/{libraryId}/books/{bookId}` and the library does not exist, or the book does not exist in that library
- **THEN** the system responds with HTTP 404

### Requirement: Delete a book from a library
The system SHALL support deleting a book via `DELETE /api/libraries/{libraryId}/books/{bookId}`, returning HTTP 204 on success.

#### Scenario: Successful delete
- **WHEN** a client sends `DELETE /api/libraries/{libraryId}/books/{bookId}` for a book that belongs to that library
- **THEN** the system deletes the book and responds with HTTP 204

#### Scenario: Delete for a missing library or book
- **WHEN** a client sends `DELETE /api/libraries/{libraryId}/books/{bookId}` and the library does not exist, or the book does not exist in that library
- **THEN** the system responds with HTTP 404

### Requirement: Library scoping of book operations
Every book operation SHALL be scoped to the `libraryId` in the route. A book created under one library SHALL NOT be retrieved, updated, or deleted through a different library id; such attempts SHALL return HTTP 404.

#### Scenario: Cross-library read returns 404
- **WHEN** a client requests books of library B and includes or targets a book that belongs to library A
- **THEN** the system responds with HTTP 404

#### Scenario: Cross-library mutation returns 404
- **WHEN** a client attempts to update or delete a book that belongs to library A through library B's route
- **THEN** the system responds with HTTP 404 and does not modify the book