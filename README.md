# Church API

[![.NET CI](https://github.com/DervisGomez/appi-miembros/actions/workflows/dotnet.yml/badge.svg)](https://github.com/DervisGomez/appi-miembros/actions/workflows/dotnet.yml)

REST API built with ASP.NET Core 8 for managing church members and their donations. Designed as a portfolio project that demonstrates production-oriented backend practices: JWT authentication, role-based authorization, structured logging, global error handling, containerization, and automated testing.

> This is a portfolio and learning project. It is not intended for production use without additional hardening.

## Features

- ASP.NET Core 8 Web API
- Entity Framework Core 8 with SQL Server
- JWT authentication and role-based authorization (`Admin`, `User`)
- Password hashing with ASP.NET Core Identity `PasswordHasher`
- User registration, login, and admin promotion
- Member management (CRUD)
- Donation management with member association
- Pagination, filtering, and sorting on list endpoints
- DTO pattern with manual mapper classes
- Input validation with Data Annotations
- Global exception handling middleware returning RFC 7807 `ProblemDetails`
- Dependency injection with scoped services
- Structured logging with Serilog (console sink and request logging)
- Health checks with database connectivity probe
- Swagger / OpenAPI (Development environment only)
- Docker and Docker Compose support
- GitHub Actions CI pipeline
- Unit tests and integration tests (46 tests)

## Technologies

| Technology | Purpose |
|---|---|
| ASP.NET Core 8 | Web API framework |
| Entity Framework Core 8 | ORM and migrations |
| SQL Server | Primary database |
| SQLite | In-memory database for integration tests |
| JWT (`System.IdentityModel.Tokens.Jwt`) | Token generation and validation |
| Swashbuckle (Swagger) | API documentation |
| ASP.NET Core Identity (`PasswordHasher`) | Password hashing |
| Serilog | Structured logging |
| xUnit | Test framework |
| Moq | Dependency mocking |
| FluentAssertions | Readable test assertions |
| EF Core InMemory | In-memory database for unit tests |
| Docker / Docker Compose | Containerized local deployment |
| GitHub Actions | Continuous integration |

## Architecture

The solution follows a conventional `src` / `tests` layout with a layered structure inside a single API project:

```text
.
├── ChurchApi.sln
├── Dockerfile
├── docker-compose.yml
├── .editorconfig
├── .github/
│   └── workflows/
│       └── dotnet.yml
├── src/
│   └── ChurchApi/
│       ├── Controllers/       HTTP endpoints (Auth, Members, Donations)
│       ├── Data/              EF Core DbContext
│       ├── Dtos/              Request, response, and query models
│       ├── Enums/             Shared enumerations (UserRole, SortOrder)
│       ├── Exceptions/        Domain-specific exceptions
│       ├── Extensions/        Service registration and pipeline configuration
│       ├── HealthChecks/      Database health check implementation
│       ├── Helpers/           Utility classes (password hashing)
│       ├── Interfaces/        Service contracts (IJwtTokenService)
│       ├── Mappers/           Manual entity-to-DTO mapping
│       ├── Middleware/        Global exception handling
│       ├── Migrations/        EF Core database migrations
│       ├── Models/            Domain entities
│       ├── Options/           Strongly typed configuration (JwtOptions)
│       ├── Services/          Business logic and service interfaces
│       └── Program.cs         Application entry point
└── tests/
    └── ChurchApi.Tests/
        ├── Fixtures/          Test fixtures (DonationServiceFixture)
        ├── Helpers/           In-memory DbContext factory
        ├── Integration/       HTTP integration tests
        │   ├── Auth/
        │   ├── Donations/
        │   ├── Health/
        │   ├── Members/
        │   ├── Helpers/
        │   └── Infrastructure/
        └── Unit/              Service layer unit tests
            ├── Helpers/
            └── Services/
```

**Controllers** handle HTTP concerns and delegate to services. **Services** contain business logic and interact with `AppDbContext`. **Dtos** decouple the API contract from domain models. **Mappers** translate between entities and DTOs. **Middleware** catches unhandled exceptions and returns structured error responses.

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server instance (local development) **or** Docker with Docker Compose

## Local Setup

### Clone and build

```bash
git clone git@github.com:DervisGomez/appi-miembros.git
cd appi-miembros
dotnet restore
dotnet build
```

### Configuration

Do not commit real connection strings or JWT secrets. Store local development secrets with the .NET user-secrets store:

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

### Database

Apply EF Core migrations before running the application:

```bash
dotnet ef database update --project src/ChurchApi
```

### Run

```bash
dotnet run --project src/ChurchApi
```

Default URLs (see `launchSettings.json`):

| Profile | URL |
|---|---|
| HTTP | http://localhost:5101 |
| HTTPS | https://localhost:7231 |
| Swagger | http://localhost:5101/swagger |

## Docker

Run the API and SQL Server together without installing SQL Server locally:

```bash
docker compose up --build
```

The API will be available at:

```text
http://localhost:8080/swagger
```

Docker Compose starts:

- **sqlserver**: SQL Server 2022 with persistent storage and health check.
- **churchapi**: ASP.NET Core API published in Release mode with health check.

The API container applies EF Core migrations automatically via `Database__ApplyMigrations=true`.

Useful commands:

```bash
docker compose ps
docker compose logs
docker compose logs churchapi
docker compose down
docker compose down -v   # also removes the SQL Server data volume
```

## Environment Variables

ASP.NET Core maps double underscores to nested configuration sections.

| Variable | Description | Required |
|---|---|---|
| `ConnectionStrings__SqlServer` | SQL Server connection string | Yes |
| `Jwt__Secret` | JWT signing key (minimum 32 characters) | Yes |
| `Jwt__Issuer` | JWT issuer claim | Yes |
| `Jwt__Audience` | JWT audience claim | Yes |
| `Jwt__ExpirationMinutes` | Token lifetime in minutes | Yes |
| `Database__ApplyMigrations` | Apply EF migrations on startup (`true` / `false`) | No |
| `ASPNETCORE_ENVIRONMENT` | Runtime environment (`Development`, `Production`, etc.) | No |
| `ASPNETCORE_URLS` | URLs the server listens on | No |

Example for deployed environments:

```bash
ConnectionStrings__SqlServer="Server=...;Database=ChurchDB;..."
Jwt__Secret="ReplaceWithAProductionSecretAtLeast32Chars"
Jwt__Issuer="ChurchApi"
Jwt__Audience="ChurchApi.Clients"
Jwt__ExpirationMinutes="60"
```

## GitHub Actions

The CI pipeline (`.github/workflows/dotnet.yml`) runs on every push and pull request to `main`:

1. Checkout repository
2. Setup .NET 8 SDK
3. Cache NuGet packages
4. `dotnet restore`
5. `dotnet build --configuration Release`
6. `dotnet test --configuration Release`

## Logging

Logging is configured with **Serilog** via `appsettings.json` and `LoggingExtensions`:

- Console sink with structured output template
- Request logging middleware with elapsed time per request
- Health check endpoints logged at `Debug` level to reduce noise
- Errors logged at `Error` level through both request logging and `ExceptionMiddleware`

Log enrichment includes application name and environment.

## Health Checks

A database connectivity health check is exposed at:

```text
GET /health
```

Returns JSON with overall status, duration, and per-check details. Returns `200` when healthy and `503` when degraded or unhealthy.

## Testing

Run all tests from the solution root:

```bash
dotnet test
```

### Test suite (46 tests)

| Area | Scope |
|---|---|
| `AuthServiceTests` | Registration, login, conflict and unauthorized scenarios |
| `MemberServiceTests` | Pagination, sorting, CRUD, constraint violations |
| `DonationServiceTests` | Pagination, filtering, sorting, CRUD, validation |
| `AuthControllerTests` | Register, login, ProblemDetails responses |
| `MemberControllerTests` | CRUD endpoints, authorization, conflict handling |
| `DonationControllerTests` | List, create, delete endpoints |
| `HealthCheckTests` | `/health` endpoint availability |

### Test infrastructure

- **Unit tests**: EF Core InMemory provider via `TestDbContextFactory`, Moq for `IJwtTokenService`, `TimeProvider` for deterministic dates.
- **Integration tests**: `WebApplicationFactory` with SQLite in-memory database, seeded admin user, full HTTP pipeline.

## API Endpoints

All routes are relative to the application base URL.

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
| `PUT` | `/api/members/{id}` | `Admin` | Update a member |
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

### Authorization roles

| Role | Access |
|---|---|
| `User` | Read members and donations, create members and donations |
| `Admin` | All `User` permissions, plus update/delete members, delete donations, and promote users |

## Roadmap

Planned improvements for future versions (not yet implemented):

- Refresh tokens
- Rate limiting on authentication endpoints
- API versioning
- OpenTelemetry / distributed tracing
- Clean Architecture layering
- Test coverage reporting in CI

## License

MIT
