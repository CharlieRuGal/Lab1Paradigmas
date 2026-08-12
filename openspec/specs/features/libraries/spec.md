# features/libraries Specification

## Purpose
Defines the externally observable behavior of the library management API: listing, retrieving, creating, updating, and deleting libraries, plus the persistence guarantees the service must honor.
## Requirements
### Requirement: List libraries
The system SHALL support listing all libraries via `GET /api/libraries` and return HTTP 200 with a JSON array of libraries. Each library object SHALL expose the JSON property names `id`, `name`, and `location`.

#### Scenario: Successful list
- **WHEN** a client sends `GET /api/libraries`
- **THEN** the system responds with HTTP 200 and a JSON array of all libraries, each containing `id`, `name`, and `location`

#### Scenario: List a single library
- **WHEN** a client sends `GET /api/libraries/{libraryId}`
- **THEN** the system responds with HTTP 200 and the matching library JSON

#### Scenario: Missing library when retrieving by id
- **WHEN** a client sends `GET /api/libraries/{libraryId}` and no library with that id exists
- **THEN** the system responds with HTTP 404

### Requirement: Create a library
The system SHALL support creating a library via `POST /api/libraries` with a JSON body containing `name` and `location`, and return HTTP 200 with the created library JSON including its assigned `id`.

#### Scenario: Successful create
- **WHEN** a client sends `POST /api/libraries` with a valid library payload
- **THEN** the system persists the library and responds with HTTP 200 and the created library JSON including its `id`

### Requirement: Update a library
The system SHALL support updating a library via `PUT /api/libraries/{libraryId}` with a JSON body, returning HTTP 204 on success.

#### Scenario: Successful update
- **WHEN** a client sends `PUT /api/libraries/{libraryId}` for an existing library with a valid payload
- **THEN** the system updates the library and responds with HTTP 204

#### Scenario: Update of a missing library
- **WHEN** a client sends `PUT /api/libraries/{libraryId}` and no library with that id exists
- **THEN** the system responds with HTTP 404

### Requirement: Delete a library
The system SHALL support deleting a library via `DELETE /api/libraries/{libraryId}`, returning HTTP 204 on success and HTTP 404 when the library does not exist.

#### Scenario: Successful delete
- **WHEN** a client sends `DELETE /api/libraries/{libraryId}` for an existing library
- **THEN** the system deletes the library and responds with HTTP 204

#### Scenario: Delete of a missing library
- **WHEN** a client sends `DELETE /api/libraries/{libraryId}` and no library with that id exists
- **THEN** the system responds with HTTP 404

#### Scenario: Books are deleted with their library
- **WHEN** a library that contains books is deleted
- **THEN** the system deletes its books as well (cascade delete)

### Requirement: Persistent storage
The system SHALL persist libraries and books in a PostgreSQL database reached through the configured connection string, using EF Core 6, a schema limited to the `Libraries` and `Books` tables, a `Library` to `Book` foreign key with cascade delete, and a single `InitialCreate` migration. The system SHALL NOT create the database schema automatically at runtime by running `dotnet ef database update`.

#### Scenario: Data survives across requests
- **WHEN** a library is created and later retrieved or listed
- **THEN** the library is present, confirming data is persisted in the configured database

#### Scenario: Required schema
- **WHEN** the database schema is inspected
- **THEN** it contains only the `Libraries` and `Books` tables with the `Books.LibraryId` foreign key to `Libraries.Id` configured for cascade delete

