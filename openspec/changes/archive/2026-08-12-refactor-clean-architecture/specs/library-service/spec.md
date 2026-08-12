## Purpose

Defines the HTTP contract for the Library Service API — the routes, request/response payload shapes, and status-code semantics for managing libraries and the books they contain — that the Clean Architecture refactor must preserve and make fully functional.

## ADDED Requirements

### Requirement: API endpoints are preserved
The system SHALL expose the same set of HTTP endpoints after the refactor as it exposes today, with unchanged routes, methods, payload keys, and status-code semantics.

#### Scenario: Existing routes still resolve
- **WHEN** a client issues the same HTTP requests that were valid before the refactor
- **THEN** the system routes them to the same endpoints and returns responses with unchanged status codes

#### Scenario: Request and response JSON unchanged
- **WHEN** a client sends the same request bodies and inspects the same responses before and after the refactor
- **THEN** the JSON schema and payload keys are identical, even when the payloads are produced through DTOs instead of persistence entities

### Requirement: Get libraries
The system SHALL provide an endpoint that returns all libraries.

#### Scenario: Returns all libraries
- **WHEN** a GET request is made to the libraries list endpoint
- **THEN** the system responds 200 OK with all libraries

#### Scenario: Retrieve a single library by id
- **WHEN** a GET request is made for a library by id that exists
- **THEN** the system responds 200 OK with that library

#### Scenario: Requested library does not exist
- **WHEN** a GET request is made for a single library by id that does not exist
- **THEN** the system responds 404 Not Found

### Requirement: Create library
The system SHALL allow creating a new library and persisting it.

#### Scenario: Successful creation
- **WHEN** a POST request is made with a valid library payload
- **THEN** the system creates the library and responds 200 OK with the created library

### Requirement: Update library
The system SHALL allow updating an existing library.

#### Scenario: Successful update
- **WHEN** a PUT request is made for an existing library with update data
- **THEN** the system updates the library and responds 204 No Content

#### Scenario: Library not found
- **WHEN** a PUT request is made for a library that does not exist
- **THEN** the system responds 404 Not Found

### Requirement: Delete library
The system SHALL allow deleting an existing library, including any books it owns.

#### Scenario: Successful deletion
- **WHEN** a DELETE request is made for a library that exists
- **THEN** the system deletes the library and its books and responds 204 No Content

#### Scenario: Library not found
- **WHEN** a DELETE request is made for a library that does not exist
- **THEN** the system responds 404 Not Found

### Requirement: Get books in a library
The system SHALL return the books that belong to a given library.

#### Scenario: Library exists with books
- **WHEN** a GET request is made to a library's books endpoint for a library that has books
- **THEN** the system responds 200 OK with that library's books

#### Scenario: Library exists without books
- **WHEN** a GET request is made to a library's books endpoint for a library that has no books
- **THEN** the system responds 200 OK with an empty collection

#### Scenario: Library not found
- **WHEN** a GET request is made to a library's books endpoint for a library that does not exist
- **THEN** the system responds 404 Not Found

### Requirement: Add a book to a library
The system SHALL create a new book and attach it to the library in the request route.

#### Scenario: Library exists
- **WHEN** a POST request is made to a library's books endpoint with a valid book payload
- **THEN** the system creates the book, assigns it to the route library, and responds 201 Created
- **AND** the created book is returned in the response body

#### Scenario: Library not found
- **WHEN** a POST request is made to a library's books endpoint for a library that does not exist
- **THEN** the system responds 404 Not Found

### Requirement: Update a book in a library
The system SHALL allow updating an existing book that belongs to a given library.

#### Scenario: Successful update
- **WHEN** a PUT request is made for a book that exists in the route library with update data
- **THEN** the system updates the book and responds 204 No Content

#### Scenario: Book not found
- **WHEN** a PUT request is made for a book that does not exist in the route library
- **THEN** the system responds 404 Not Found

#### Scenario: Library not found
- **WHEN** a PUT request is made to a books endpoint for a library that does not exist
- **THEN** the system responds 404 Not Found

### Requirement: Delete a book in a library
The system SHALL allow deleting an existing book that belongs to a given library.

#### Scenario: Successful deletion
- **WHEN** a DELETE request is made for a book that exists in the route library
- **THEN** the system deletes the book and responds 204 No Content

#### Scenario: Book not found
- **WHEN** a DELETE request is made for a book that does not exist in the route library
- **THEN** the system responds 404 Not Found

#### Scenario: Library not found
- **WHEN** a DELETE request is made to a books endpoint for a library that does not exist
- **THEN** the system responds 404 Not Found

### Requirement: Books are scoped to the route library
The system SHALL treat the library id from the route as authoritative for book operations; book persistence and lookups must never cross library boundaries.

#### Scenario: Book created belongs to route library
- **WHEN** a book is created via a library's books endpoint
- **THEN** the persisted book has the route library id regardless of the library id in the payload

#### Scenario: Book lookup is library-scoped
- **WHEN** a book operation is performed with a book id that exists only under a different library
- **THEN** the system responds as if the book does not exist, with 404 Not Found

#### Scenario: Books are cascade-deleted with their library
- **WHEN** a library is deleted
- **THEN** the books that belonged to it are also deleted