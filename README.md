# Church API

[![.NET CI](https://github.com/DervisGomez/appi-miembreos/actions/workflows/dotnet.yml/badge.svg)](https://github.com/DervisGomez/appi-miembreos/actions/workflows/dotnet.yml)

A REST API built with ASP.NET Core 8 for managing church members and their donations. The project demonstrates common backend patterns including JWT authentication, role-based authorization, Entity Framework Core data access, and unit testing with mocked dependencies.

This is a learning and portfolio project. It is not intended for production use.

## Features

- ASP.NET Core 8 Web API
- Entity Framework Core with SQL Server
- JWT authentication via ASP.NET Core `AddJwtBearer`
- Role-based authorization (`Admin`, `User`)
- Password hashing with ASP.NET Core Identity `PasswordHasher`
- User registration and login
- Member management (CRUD)
- Donation management with member association
- Pagination on list endpoints (members and donations)
- Filtering donations by `MemberId`, `MinAmount`, and `MaxAmount`
- Sorting members by name and donations by date
- DTO pattern for request and response models
- Manual mapping via static mapper classes
- Global exception handling middleware with consistent JSON error responses
- Dependency injection with scoped services
- Separation of JWT token generation (`IJwtTokenService`) from authentication logic (`IAuthService`)
- Unit testing with xUnit, Moq, and FluentAssertions
- Swagger / OpenAPI (Development environment only)

## Architecture

The solution follows a conventional `src` / `tests` layout:

```text
.
├── ChurchApi.sln
├── src/
│   └── ChurchApi/
│       ├── Authentication/    Custom JWT authentication handler and options
│       ├── Controllers/       HTTP endpoints (Auth, Members, Donations)
│       ├── Data/              EF Core DbContext
│       ├── Dtos/              Request, response, and query models
│       ├── Enums/             Shared enumerations (UserRole, SortOrder)
│       ├── Exceptions/        Domain-specific exceptions
│       ├── Extensions/        Service registration extensions
│       ├── Helpers/           Utility classes (password hashing)
│       ├── Interfaces/        Service contracts
│       ├── Mappers/           Manual entity-to-DTO mapping
│       ├── Middleware/        Global exception handling
│       ├── Migrations/        EF Core database migrations
│       ├── Models/            Domain entities
│       ├── Services/          Business logic
│       └── Program.cs         Application entry point and DI configuration
└── tests/
    └── ChurchApi.Tests/
        ├── Helpers/           Test infrastructure (in-memory DbContext factory)
        └── Services/          Unit tests for service layer
```

**Controllers** handle HTTP concerns and delegate to services. **Services** contain business logic and interact with `AppDbContext`. **Dtos** decouple the API contract from domain models. **Mappers** translate between entities and DTOs. **Middleware** catches unhandled exceptions and returns structured error responses.

## Technologies

| Technology | Purpose |
|---|---|
| ASP.NET Core 8 | Web API framework |
| Entity Framework Core 8 | ORM and migrations |
| SQL Server | Primary database |
| JWT (`System.IdentityModel.Tokens.Jwt`) | Token generation and validation |
| Swashbuckle (Swagger) | API documentation |
| ASP.NET Core Identity (`PasswordHasher`) | Password hashing |
| xUnit | Test framework |
| Moq | Dependency mocking |
| FluentAssertions | Readable test assertions |
| EF Core InMemory | In-memory database for unit tests |

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server instance accessible from your machine

### Setup

```bash
git clone https://github.com/DervisGomez/appi-miembreos.git
cd appi-miembreos
dotnet restore
dotnet build
```

> **Note:** The cloned directory name matches the repository name on GitHub. If you fork or rename the repository, adjust the `cd` command accordingly.

### Configuration

Do not commit real connection strings or JWT secrets. Store local development secrets with
the .NET user-secrets store:

```bash
dotnet user-secrets set "ConnectionStrings:SqlServer" "Server=localhost,1433;Database=ChurchDB;User Id=sa;Password=YourPassword;TrustServerCertificate=True;" --project src/ChurchApi
dotnet user-secrets set "Jwt:Secret" "ReplaceWithALongLocalDevelopmentSecretAtLeast32Chars" --project src/ChurchApi
```

`src/ChurchApi/appsettings.json` intentionally contains no secret values:

```json
"ConnectionStrings": {
  "SqlServer": ""
}
```

For deployed environments, configure secrets through environment variables or your hosting
provider's secret manager. ASP.NET Core maps double underscores to configuration sections:

```bash
Jwt__Secret="ReplaceWithAProductionSecretAtLeast32Chars"
Jwt__Issuer="ChurchApi"
Jwt__Audience="ChurchApi.Clients"
Jwt__ExpirationMinutes="60"
```

### Database

Apply EF Core migrations before running the application:

```bash
dotnet ef database update --project src/ChurchApi
```

### Run

```bash
dotnet run --project src/ChurchApi
```

By default, the application starts on `http://localhost:5101` and `https://localhost:7231` (see `launchSettings.json`).

## Running Tests

Execute all unit tests from the solution root:

```bash
dotnet test
```

Current test coverage focuses on `AuthService` (registration, login, and error scenarios) using an in-memory database and mocked `IJwtTokenService`.

## API Documentation

Swagger UI is enabled automatically when running in the **Development** environment.

| Profile | Swagger URL |
|---|---|
| HTTP | http://localhost:5101/swagger |
| HTTPS | https://localhost:7231/swagger |

The Swagger configuration includes a Bearer token security scheme for testing authenticated endpoints.

## API Endpoints

All routes are relative to the application base URL (e.g. `http://localhost:5101`).

### Auth

| Method | Endpoint | Authorization | Description |
|---|---|---|---|
| `POST` | `/api/auth/register` | None | Register a new user |
| `POST` | `/api/auth/login` | None | Authenticate and receive a JWT |
| `PATCH` | `/api/auth/{userId}/promote` | `Admin` | Promote a user to the `Admin` role |

### Members

| Method | Endpoint | Authorization | Description |
|---|---|---|---|
| `GET` | `/api/members` | Authenticated | List members (paginated, sortable) |
| `GET` | `/api/members/{id}` | Authenticated | Get a member by ID |
| `POST` | `/api/members` | Authenticated | Create a member |
| `PUT` | `/api/members` | `Admin` | Update a member |
| `DELETE` | `/api/members/{id}` | `Admin` | Delete a member |
| `GET` | `/api/members/{memberId}/donations` | Authenticated | List donations for a member |
| `POST` | `/api/members/{memberId}/donations` | Authenticated | Add a donation to a member |

**Query parameters for `GET /api/members`:**

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Page` | `int` | `1` | Page number |
| `PageSize` | `int` | `10` | Items per page (max `100`) |
| `SortOrder` | `Asc` \| `Desc` | `Asc` | Sort by name and last name |

### Donations

| Method | Endpoint | Authorization | Description |
|---|---|---|---|
| `GET` | `/api/donations` | Authenticated | List donations (paginated, filterable, sortable) |
| `DELETE` | `/api/donations/{id}` | `Admin` | Delete a donation |

**Query parameters for `GET /api/donations`:**

| Parameter | Type | Default | Description |
|---|---|---|---|
| `MemberId` | `int?` | — | Filter by member ID |
| `MinAmount` | `decimal?` | — | Minimum donation amount |
| `MaxAmount` | `decimal?` | — | Maximum donation amount |
| `Page` | `int` | `1` | Page number |
| `PageSize` | `int` | `10` | Items per page (max `100`) |
| `SortOrder` | `Asc` \| `Desc` | `Desc` | Sort by donation date |

## Authentication

### Register

`POST /api/auth/register`

Creates a new user with the `User` role. Passwords are hashed before persistence. Returns the created user without a token.

### Login

`POST /api/auth/login`

Validates credentials by username or email. On success, returns a JWT signed with HMAC-SHA256. Tokens expire after one hour and include `NameIdentifier` (user ID) and `Role` claims.

### Authorization

Protected endpoints require a valid Bearer token in the `Authorization` header. Some operations are restricted to the `Admin` role:

| Role | Access |
|---|---|
| `User` | Read members and donations, create members and donations |
| `Admin` | All `User` permissions, plus update/delete members, delete donations, and promote users to admin |

### Promote to Admin

`PATCH /api/auth/{userId}/promote` (Admin only)

Promotes an existing user to the `Admin` role.

## Testing

The test project (`tests/ChurchApi.Tests`) uses:

- **xUnit** as the test runner and assertion framework
- **Moq** to mock `IJwtTokenService`, keeping `AuthService` tests independent of JWT configuration
- **FluentAssertions** for expressive assertions
- **EF Core InMemory** via `TestDbContextFactory` to isolate database access in unit tests

Tests target the service layer directly, without spinning up the web host.

## Future Improvements

The following items are planned but **not yet implemented**:

- Refresh tokens
- Integration tests
- Docker support
- GitHub Actions CI/CD
- Clean Architecture layering
- Structured logging with Serilog

## License

MIT
