## Purpose

Defines the externally observable HTTP contract of the Library Service API — endpoints, payload shapes, and status-code semantics — that the layered-architecture refactor must preserve without regression.

## ADDED Requirements

### Requirement: API endpoints are preserved
The system SHALL expose the same set of HTTP endpoints after the refactor as it exposes today, with unchanged routes, methods, payload shapes, and status-code semantics.

#### Scenario: Existing routes still resolve
- **WHEN** a client issues the same HTTP requests that were valid before the refactor
- **THEN** the system routes them to the same endpoints and returns responses with unchanged status codes

#### Scenario: Request and response JSON unchanged
- **WHEN** a client sends the same request bodies and inspects the same responses before and after the refactor
- **THEN** the JSON schema and payload keys are identical, even when the payloads are produced through DTOs instead of persistence entities

### Requirement: List libraries
The system SHALL expose a collection endpoint for libraries that returns all libraries.

#### Scenario: Retrieve all libraries
- **WHEN** a client requests the libraries collection
- **THEN** the system responds 200 OK with the full set of libraries

#### Scenario: Retrieve a single library by id
- **WHEN** a client requests a library by id that exists
- **THEN** the system responds 200 OK with that library

#### Scenario: Single library not found
- **WHEN** a client requests a library by id that does not exist
- **THEN** the system responds 404 Not Found

### Requirement: Create library
The system SHALL support creating a library from a request payload and return the created resource.

#### Scenario: Successful creation
- **WHEN** a client submits a valid library payload
- **THEN** the system persists the library and responds 200 OK with the created library

### Requirement: Update library
The system SHALL support updating an existing library.

#### Scenario: Update existing library
- **WHEN** a client submits an update for a library that exists
- **THEN** the system applies the update and responds 204 No Content

#### Scenario: Update non-existent library
- **WHEN** a client submits an update for a library that does not exist
- **THEN** the system responds 404 Not Found

### Requirement: Delete library
The system SHALL support deleting a library by id and the books that belong to it.

#### Scenario: Delete existing library
- **WHEN** a client deletes a library that exists
- **THEN** the system removes the library and responds 204 No Content

#### Scenario: Delete non-existent library
- **WHEN** a client deletes a library that does not exist
- **THEN** the system responds 404 Not Found

### Requirement: List books in a library
The system SHALL expose the books that belong to a specific library.

#### Scenario: Library exists
- **WHEN** a client requests the books of a library that exists
- **THEN** the system responds 200 OK with that library's books (possibly empty)

#### Scenario: Library not found
- **WHEN** a client requests the books of a library that does not exist
- **THEN** the system responds 404 Not Found

### Requirement: Add book to a library
The system SHALL support adding a book to a specific library.

#### Scenario: Library exists
- **WHEN** a client submits a book payload for a library that exists
- **THEN** the system persists the book and responds 201 Created

#### Scenario: Library not found
- **WHEN** a client submits a book payload for a library that does not exist
- **THEN** the system responds 404 Not Found

### Requirement: Persistence entities are not exposed
The system SHALL not leak persistence entities in its HTTP responses; controllers SHALL operate on DTOs so that response payloads are decoupled from the persistence model.

#### Scenario: Responses shaped by DTOs
- **WHEN** a client receives a successful response
- **THEN** the payload is shaped by DTOs and does not include persistence-only concerns

### Requirement: Database schema is preserved
The system SHALL keep the existing `Libraries` and `Books` tables and their schema unchanged so that existing data in the Supabase database remains valid.

#### Scenario: No schema drift
- **WHEN** the application is started and migrations are checked
- **THEN** the existing migration is recognized as applied and no new schema changes are generated
