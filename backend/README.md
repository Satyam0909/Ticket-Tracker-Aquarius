# Backend

ASP.NET Core Web API targeting .NET 10, with EF Core and SQL Server. The API contract remains under `/api/tickets`; Swagger UI is enabled only in Development.

## Structure

- `Api/Controllers`: HTTP routes and status codes.
- `Api/Contracts` and `Api/Validation`: request binding, validation attributes, and response-safe contracts.
- `Api/Middleware`: exception translation, request logging, and security headers.
- `Application/Services`: ticket use cases and repository-independent contracts.
- `Application/Utilities`: reusable ticket filtering, sorting, and pagination.
- `Domain`: ticket entity and status/priority constants.
- `Infrastructure/Repositories`: EF Core query and persistence implementation.
- `Infrastructure/Persistence`: SQL Server `DbContext` and mappings.
- `Migrations`: versioned SQL Server schema.

## Run and migrate

Set `ASPNETCORE_ENVIRONMENT=Development` to load the LocalDB connection string. Run the commands from the repository root.

In Git Bash:

```bash
export ASPNETCORE_ENVIRONMENT=Development
dotnet tool restore
dotnet ef database update --project ./backend/SupportTickets.Api/SupportTickets.Api.csproj
dotnet run --project ./backend/SupportTickets.Api/SupportTickets.Api.csproj --urls http://localhost:5080
```

In PowerShell, run:

```powershell
dotnet tool restore
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet ef database update --project .\backend\SupportTickets.Api
dotnet run --project .\backend\SupportTickets.Api --urls http://localhost:5080
```

If your terminal is already in the `backend` directory, use these project paths instead:

```bash
dotnet ef database update --project ./SupportTickets.Api/SupportTickets.Api.csproj
dotnet run --project ./SupportTickets.Api/SupportTickets.Api.csproj --urls http://localhost:5080
```

Production configuration is supplied externally: `ConnectionStrings__Tickets`, `AllowedHosts`, `Cors__AllowedOrigins__0`, and `Authentication__Jwt__SigningKey` are required. Rate limiting is configurable with `RateLimiting__PermitLimit`, `RateLimiting__WindowSeconds`, and `RateLimiting__AuthPermitLimit`. When deployed behind a reverse proxy, add ASP.NET Core forwarded-header middleware with explicit trusted proxy addresses before the rate limiter; arbitrary forwarded headers must not be trusted. Never log or return connection strings, SQL exceptions, request bodies, or bearer credentials.

## Endpoints

- `GET /api/tickets?status=open&search=printer&page=1&pageSize=10&sortBy=createdAt&sortDirection=desc`
- `GET /api/tickets/{id}`
- `POST /api/tickets`
- `PUT /api/tickets/{id}`
- `DELETE /api/tickets/{id}`
- `GET /api/tickets/summary`
- `GET /health/live`
- `GET /health/ready`

Invalid requests return 400 Problem Details, unknown ticket IDs return 404 Problem Details, and rate-limited requests return 429 Problem Details. Error bodies include an error code and trace ID without echoing submitted data. Ticket summaries are grouped by SQL Server queries; each change is persisted with EF Core's SaveChanges transaction.

## Tests

```powershell
dotnet test .\backend\SupportTickets.Tests\SupportTickets.Tests.csproj
```

The HTTP integration tests use a unique in-memory database for each test factory; production persistence remains SQL Server.

Accounts can be created through public registration (`POST /api/auth/register`) and signed in with `POST /api/auth/login`. Passwords are hashed; the API issues 30-minute bearer tokens and requires them for every ticket endpoint. Development and tests use an ephemeral signing key unless one is configured, so active tokens are invalidated when the API restarts. Every registered account can manage all tickets. Production must supply a stable `Authentication__Jwt__SigningKey` of at least 32 bytes through the deployment secret manager. Signing out clears the current tab's token; it remains valid until it expires. No email verification or password-reset flow is configured.
