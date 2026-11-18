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

Set `ASPNETCORE_ENVIRONMENT=Development` to load the local SQL Server settings. Set the database password once in .NET User Secrets (not in a tracked file), from the repository root:

```shell
dotnet user-secrets set DB_PASSWORD "<your SQL Server password>" --project ./backend/SupportTickets.Api/SupportTickets.Api.csproj
```

In Git Bash:

```bash
export ASPNETCORE_ENVIRONMENT=Development
dotnet tool restore
dotnet ef database update --project ./backend/SupportTickets.Api/SupportTickets.Api.csproj
dotnet watch --non-interactive --project ./backend/SupportTickets.Api/SupportTickets.Api.csproj run
```

In PowerShell, run:

```powershell
dotnet tool restore
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet ef database update --project .\backend\SupportTickets.Api
dotnet watch --non-interactive --project .\backend\SupportTickets.Api run
```

If your terminal is already in the `backend` directory, use these project paths instead:

```bash
dotnet ef database update --project ./SupportTickets.Api/SupportTickets.Api.csproj
dotnet watch --non-interactive --project ./SupportTickets.Api/SupportTickets.Api.csproj run
```

Keep the watcher running in the terminal while developing. Supported C# edits use Hot Reload; changes that require a new process are rebuilt and restarted automatically, without an interactive prompt. The API's HTTP launch profile binds to `http://localhost:5080`.

Production configuration is supplied externally: `DB_DIALECT=mssql`, `DB_HOST`, `DB_PORT`, `DB_DATABASE`, `DB_USERNAME`, `DB_PASSWORD`, `AllowedHosts`, `Cors__AllowedOrigins__0`, and `Authentication__Jwt__SigningKey` are required. Rate limiting is configurable with `RateLimiting__PermitLimit`, `RateLimiting__WindowSeconds`, and `RateLimiting__AuthPermitLimit`. When deployed behind a reverse proxy, add ASP.NET Core forwarded-header middleware with explicit trusted proxy addresses before the rate limiter; arbitrary forwarded headers must not be trusted. Never log or return database credentials, SQL exceptions, request bodies, or bearer credentials.

## Endpoints

- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/refresh`
- `POST /api/auth/logout`
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

On startup, EF Core applies pending migrations from `Migrations` to create or alter schema objects without recreating tables or truncating existing rows. Ensure the configured SQL Server account has permission to apply schema changes; review migrations before deployment.

Accounts can be created through public registration (`POST /api/auth/register`) and signed in with `POST /api/auth/login`. Passwords are hashed. A 15-minute access JWT and a rotating 14-day refresh token are delivered only in `HttpOnly` cookies; refresh-token hashes are persisted in the database. `POST /api/auth/refresh` rotates the single-use refresh token, and `POST /api/auth/logout` revokes it and clears both cookies. The frontend must send credentialed requests. Development cookies use `SameSite=Lax`; production cookies use `SameSite=None; Secure` and therefore require HTTPS. Browser write requests must use an origin configured in `Cors:AllowedOrigins`; CORS is credentialed but never permits wildcard origins. Configure all deployed frontend origins explicitly.

Development and tests use an ephemeral signing key unless one is configured, so active access tokens are invalidated when the API restarts. Every registered account can manage all tickets. Production must supply a stable `Authentication__Jwt__SigningKey` of at least 32 bytes through the deployment secret manager. A cleared access cookie does not invalidate an already issued stateless JWT; it expires within 15 minutes. No email verification or password-reset flow is configured.
