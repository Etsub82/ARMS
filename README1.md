# Application Role Management System (ARMS) - Project Documentation

## 1. Project Overview and Goal

The Application Role Management System (ARMS) is a robust backend API designed to manage client applications, assign them to logical groups, and define granular roles within those groups. It provides a secure administrative interface for management tasks and a programmatic API endpoint for client applications to verify their credentials and dynamically retrieve their assigned access rights (groups and roles).

**Overall Goal:** To centralize and streamline the management of application access control, allowing administrators to define permissions and client applications to discover their authorized capabilities.

## 2. Core Components and Architecture

The project adheres to a **Clean Architecture** pattern, promoting separation of concerns, maintainability, and testability.

*   **API (Presentation Layer):**
    *   Handles HTTP requests and responses.
    *   Contains controllers (`AdminController`, `ApiController`, `AuthController`).
    *   Manages dependency injection, routing, and serialization.
    *   Configures Swagger/OpenAPI for API documentation and testing.
    *   Implements JWT authentication and authorization middleware.
*   **Application (Application/Core Layer):**
    *   Contains Data Transfer Objects (DTOs) for requests and responses.
    *   Defines interfaces for repositories (`IGenericRepository`, `IApplicationRepository`).
    *   Includes AutoMapper profiles for object-to-object mapping.
    *   Orchestrates business logic by interacting with Domain and Persistence layers.
*   **Domain (Domain Layer):**
    *   Contains core business entities (`ApplicationModel`, `ApplicationGroup`, `RoleModel`, `GroupRole`).
    *   Defines base entities (`BaseDomainEntity`) with common properties like `Id`, `DateCreated`, etc.
    *   Encapsulates enterprise-wide business rules.
*   **Persistence (Infrastructure/Data Access Layer):**
    *   Implements the data access logic using Entity Framework Core.
    *   Contains the `ARMSDbContext` for database interaction.
    *   Provides concrete implementations for repository interfaces (`GenericRepository`, `ApplicationRepository`).
    *   Manages database migrations.

## 3. Key Features Implemented

### 3.1. Data Layer and Database Setup

*   **Entity Framework Core (EF Core):** Used as the Object-Relational Mapper (ORM) for interacting with a SQL Server database.
*   **SQL Server LocalDB:** The target database system for local development.
*   **Database Context (`ARMSDbContext`):** Configured to map domain entities to database tables. Crucially, `Id` properties in `BaseDomainEntity` are configured as identity columns (`ValueGeneratedOnAdd()`).
*   **Migrations:** Managed through `dotnet ef migrations` commands to evolve the database schema (e.g., `InitialCreate`, `FinalInitialMigration`).
*   **Connection Strings:** Configured in `appsettings.json` and `appsettings.Development.json` (e.g., `staggingConnectionString`).

### 3.2. Administrative API Endpoints (`AdminController`)

The `AdminController` provides a comprehensive set of endpoints for managing the ARMS system. All these endpoints are secured with JWT authentication and require an "Administrator" role.

*   **Group Management:**
    *   `POST /api/Admin/createGroup`: Creates a new application group.
    *   `DELETE /api/Admin/deleteGroup/{id}`: Deletes an existing application group (with checks for dependent applications/roles).
*   **Role Management:**
    *   `POST /api/Admin/createRole`: Creates a new role.
    *   `DELETE /api/Admin/deleteRole/{id}`: Deletes an existing role (with checks for dependent group roles).
*   **Application Management:**
    *   `POST /api/Admin/createApplication`: Registers a new client application, automatically generating `AppId` and `AppKey` if not provided, and setting the initial status to "Pending".
    *   `GET /api/Admin/pendingApplications`: Retrieves a list of applications awaiting approval.
    *   `PUT /api/Admin/approveApplication`: Approves a pending application.
    *   `PUT /api/Admin/rejectApplication`: Rejects a pending application.
    *   `DELETE /api/Admin/deleteApplication/{id}`: Deletes an application.
*   **Assignment Operations:**
    *   `PUT /api/Admin/assignApplicationToGroup`: Assigns an approved application to a specific group.
    *   `POST /api/Admin/assignRolesToGroup`: Assigns multiple roles to a group.

### 3.3. Client Application API Endpoint (`ApiController`)

The `ApiController` provides a core endpoint for external client applications to retrieve their access rights programmatically.

*   **`POST /api/Api/getaccess`**:
    *   **Input:** `AppCredentialDto` (containing `AppId` and `AppKey`).
    *   **Functionality:**
        1.  Validates the provided `AppId` and `AppKey`.
        2.  Checks if the application's status is "Approved".
        3.  If valid and approved, retrieves the `ApplicationGroup` the app belongs to and all `RoleModel`s assigned to that group.
        4.  Returns an `AppAccessDto` detailing the application's name, approval status, and its assigned group with associated roles.
    *   **Importance:** This is the primary access control gateway, enabling client applications to dynamically discover and enforce their permissions.

### 3.4. Authentication and Authorization (JWT)

*   **JWT Generation (`AuthController`):**
    *   `POST /api/Auth/login`: Authenticates an "admin" user (hardcoded for demonstration) and issues a JSON Web Token (JWT).
*   **JWT Configuration (`API/Program.cs`):**
    *   Configures `Microsoft.AspNetCore.Authentication.JwtBearer` services.
    *   Sets up `TokenValidationParameters` for validating issuer, audience, lifetime, and the signing key.
    *   Crucially, uses `Encoding.UTF8.GetBytes()` for the `Secret` key to ensure robust key handling.
    *   **Middleware Order:** `app.UseAuthentication()` is correctly placed **before** `app.UseAuthorization()` in the HTTP request pipeline.
*   **JWT Settings (`appsettings.json`, `appsettings.Development.json`):**
    *   `JwtSettings` section defines `Secret` (at least 32 characters long), `Issuer`, and `Audience`.
*   **Authorization (`AdminController`):**
    *   The `[Authorize(Roles = "Administrator")]` attribute is applied to the `AdminController` to restrict access to authenticated users with the "Administrator" role.
*   **Swagger Integration:**
    *   `AddSecurityDefinition` and `AddSecurityRequirement` are configured in `Program.cs` to enable JWT Bearer authentication within the Swagger UI, providing an "Authorize" button for token input.

## 4. Technical Stack

*   **Language:** C#
*   **Framework:** .NET 9.0
*   **Web Framework:** ASP.NET Core Web API
*   **ORM:** Entity Framework Core
*   **Database:** SQL Server (LocalDB)
*   **Object Mapping:** AutoMapper
*   **Authentication:** JSON Web Tokens (JWT)
*   **API Documentation:** Swagger UI (OpenAPI)

## 5. Development Process & Key Challenges/Solutions

The development process was iterative, involving significant troubleshooting, particularly in the early stages of database setup and later with JWT authentication.

1.  **Initial Database Setup and Migrations:**
    *   **Challenge:** Persistent "Pending Model Change Warning" errors during `dotnet ef database update`. Issues with `Id` not being auto-generated by the database (`id: 0`). Conflicts between nullable `Id` properties and `IDENTITY` column requirements.
    *   **Solution:** Involved multiple cycles of:
        *   Correcting the connection string name in `appsettings.json` and `ARMSDbContextFactory.cs`.
        *   Explicitly configuring `Id` as an identity column using `ValueGeneratedOnAdd()` in `Persistence/DbContext.cs`.
        *   Ensuring `Id` in `BaseDomainEntity.cs` and `BaseCommandResponse.cs` was `int` (non-nullable).
        *   Performing `dotnet clean`, deleting all existing migrations and `ARMSDbContextModelSnapshot.cs`, dropping the database, and then creating a single `FinalInitialMigration`.
        *   Explicitly setting `DateCreated`, `CreatedBy`, `LastModifiedDate`, and `LastModifiedBy` when creating new entities to avoid `NULL` errors.
2.  **API Endpoint Implementation:**
    *   Administrative endpoints were built incrementally, ensuring proper DTOs for request/response and utilizing repository patterns for data access.
    *   **Challenge:** `AppId` and `AppKey` for applications were `null` when not explicitly provided.
    *   **Solution:** Modified the `CreateApplication` endpoint to use `string.IsNullOrWhiteSpace` checks before generating new GUIDs for `AppId` and `AppKey`.
3.  **AutoMapper Integration:**
    *   **Challenge:** `AutoMapperMappingException` due to missing type map configurations.
    *   **Solution:** Added necessary `CreateMap` configurations in `Application/Profiles/MappingProfiles.cs` for mapping between domain entities and DTOs (e.g., `ApplicationModel` to `ApplicationListDto`).
4.  **JWT Authentication and Authorization:**
    *   **Challenge 1: `System.ArgumentOutOfRangeException: ... key size must be greater than: '256' bits, key has '128' bits.`**
        *   **Solution:** Updated the `Secret` key in `appsettings.json` to be at least 32 characters long to meet the minimum key size requirement for HMAC-SHA256.
    *   **Challenge 2: Swagger "Authorize" button missing.**
        *   **Solution:** Configured `SwaggerGen` in `Program.cs` with `AddSecurityDefinition` and `AddSecurityRequirement` to enable JWT Bearer authentication in the Swagger UI.
    *   **Challenge 3: `401 Unauthorized` even with a token, `www-authenticate: Bearer` in response.**
        *   **Solution:** Corrected the middleware order in `Program.cs`, ensuring `app.UseAuthentication()` is called *before* `app.UseAuthorization()`.
    *   **Challenge 4: `www-authenticate: Bearer error="invalid_token",error_description="The signature key was not found"`**
        *   **Solution:** Changed `Encoding.ASCII.GetBytes()` to `Encoding.UTF8.GetBytes()` when converting the `Secret` key to bytes in `Program.cs` for `SymmetricSecurityKey`. This ensures proper handling of various characters and consistent key representation during token validation.
    *   **Challenge 5: File locking during build/run (`apphost.exe`, `Application.dll`, `Domain.dll`, `Persistence.dll`).**
        *   **Solution:** Repeatedly identified and terminated the locking process using `netstat -ano` and `taskkill /PID <PID> /F` in an administrator Command Prompt.

## 6. How to Run and Test the Project

To get your ARMS project up and running:

### Prerequisites

*   .NET 9.0 SDK installed.
*   SQL Server LocalDB installed (usually comes with Visual Studio or SQL Server Express).
*   A code editor (like Visual Studio or VS Code).

### Steps to Run

1.  **Navigate to the project root:**
    ```bash
    cd C:\Users\esaya\OneDrive\Pictures\Downloads\ARMS-master\ARMS-master
    ```

2.  **Restore NuGet packages (if not already done):**
    ```bash
    dotnet restore
    ```

3.  **Ensure database is up-to-date:**
    *   If you have issues or want a fresh start, you can drop the database (BE CAREFUL, this deletes all data):
        ```bash
        dotnet ef database drop --project Persistence --startup-project API
        ```
    *   Apply the latest migrations to create/update the database schema:
        ```bash
        dotnet ef database update --project Persistence --startup-project API
        ```

4.  **Build the project:**
    ```bash
    dotnet build
    ```
    *   *Troubleshooting:* If you encounter "file locked" errors, use `netstat -ano | findstr :<port_number>` and `taskkill /PID <PID> /F` to kill any lingering processes locking the files, then retry `dotnet build`.

5.  **Run the API application:**
    ```bash
    dotnet run --project API
    ```
    *   This will start the Kestrel web server, typically on `https://localhost:5001` and `http://localhost:5217`.
    *   *Troubleshooting:* If you encounter "address already in use" errors, use `netstat -ano | findstr :<port_number>` and `taskkill /PID <PID> /F` to kill the process using the port, then retry `dotnet run`.

### Testing with Swagger UI

Once the application is running:

1.  **Open Swagger UI:** Navigate to `https://localhost:5001/swagger` (or the appropriate URL shown in your console).

2.  **Login (Get JWT Token):**
    *   Expand the `Auth` controller.
    *   Expand the `POST /api/Auth/login` endpoint.
    *   Click "Try it out".
    *   In the "Request body", enter:
        ```json
        {
          "username": "admin",
          "password": "password"
        }
        ```
    *   Click "Execute".
    *   **Copy the entire `token` string** from the `Response body`.

3.  **Authorize in Swagger:**
    *   Click the green "Authorize" button at the top right of the Swagger UI.
    *   In the "Value" field, type `Bearer ` (note the space after Bearer) and then paste the token you copied.
    *   Example: `Bearer eyJhbGciOiJIUzI1Ni...`
    *   Click "Authorize", then "Close".

4.  **Test an Admin Endpoint (e.g., `createGroup`):**
    *   Expand the `Admin` controller.
    *   Expand the `POST /api/Admin/createGroup` endpoint.
    *   Click "Try it out".
    *   In the "Request body", enter:
        ```json
        {
          "name": "My New Application Group"
        }
        ```
    *   Click "Execute".
    *   You should receive a `200 OK` response with the `id` of the newly created group, confirming successful authentication and authorization.

5.  **Test Client Access Endpoint (e.g., `getaccess`):**
    *   First, ensure you have an **Approved** application in your database. You can create and approve one using the Admin endpoints. Make note of its `AppId` and `AppKey`.
    *   Expand the `Api` controller.
    *   Expand the `POST /api/Api/getaccess` endpoint.
    *   Click "Try it out".
    *   In the "Request body", enter the `AppId` and `AppKey` of an approved application:
        ```json
        {
          "appId": "YOUR_APP_ID",
          "appKey": "YOUR_APP_KEY"
        }
        ```
    *   Click "Execute".
    *   You should receive a `200 OK` response with the application's access information (group and roles).