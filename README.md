# Ticket Tracker

A modular full-stack support-ticket application. The backend is an ASP.NET Core Web API with EF Core and SQL Server; the frontend is React and TypeScript. The backend targets .NET 10, and the frontend requires Node.js 24 or later.

## Architecture

```text
frontend/
  src/shared/                  API client, constants, date and pagination utilities
  src/features/tickets/        Ticket types, API adapter, dashboard and UI components
backend/
  SupportTickets.Api/
    Api/                       Controllers, request contracts, validation and middleware
    Application/                Ticket use cases, contracts, query utilities and exceptions
    Domain/                     Ticket entity and allowed values
    Infrastructure/             EF Core persistence, repository and health checks
    Migrations/                 Versioned SQL Server schema changes
  SupportTickets.Tests/         HTTP integration tests using an isolated EF Core provider
```

The backend is a modular monolith: controllers depend on application services, which use repository interfaces; EF Core implementations and SQL Server configuration are isolated in Infrastructure. Request and success DTOs retain the existing `/api/tickets` routes and successful JSON shapes. Errors use sanitized RFC Problem Details with a stable `code` and `traceId`.

## Prerequisites

- .NET 10 SDK
- SQL Server listening at the configured host and port
- Node.js 24+ and npm

## Local setup

From the project root in Git Bash:

```bash
export ASPNETCORE_ENVIRONMENT=Development
dotnet user-secrets set DB_PASSWORD "<your SQL Server password>" --project ./backend/SupportTickets.Api/SupportTickets.Api.csproj
dotnet tool restore
dotnet ef database update --project ./backend/SupportTickets.Api/SupportTickets.Api.csproj
dotnet run --project ./backend/SupportTickets.Api/SupportTickets.Api.csproj --urls http://localhost:5080
```

Or, in PowerShell:

```powershell
dotnet tool restore
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet user-secrets set DB_PASSWORD "<your SQL Server password>" --project .\backend\SupportTickets.Api
dotnet ef database update --project .\backend\SupportTickets.Api
dotnet run --project .\backend\SupportTickets.Api --urls http://localhost:5080
```

Swagger UI is available at `http://localhost:5080/swagger`. The Vite development server proxies API requests to the backend:

```powershell
Set-Location .\frontend
npm ci
npm run dev
```

Open `http://127.0.0.1:5173`.

## Configuration and deployment

- Development SQL Server host, port, database, username, and dialect defaults are in `backend/SupportTickets.Api/appsettings.Development.json`; store `DB_PASSWORD` with `dotnet user-secrets` or set it in the shell. ASP.NET Core does not load `.env` files automatically.
- Do not put production credentials in appsettings files. Supply `DB_DIALECT=mssql`, `DB_HOST`, `DB_PORT`, `DB_DATABASE`, `DB_USERNAME`, and `DB_PASSWORD` through the host's environment/secret manager.
- Production startup requires the six `DB_*` settings above, a non-wildcard `AllowedHosts`, `Cors__AllowedOrigins__0`, and a stable `Authentication__Jwt__SigningKey` of at least 32 bytes. Configure additional CORS origins by adding indexed `Cors__AllowedOrigins__N` environment variables.
- The frontend accepts `VITE_API_BASE_URL` at build time. For a separate frontend/API origin, configure backend CORS and update the static host's `Content-Security-Policy` `connect-src` in `frontend/public/_headers`.
- Serve production traffic over HTTPS behind a trusted TLS-terminating host. Set rate-limit values with `RateLimiting__PermitLimit` and `RateLimiting__WindowSeconds`. If deployed behind a reverse proxy, add ASP.NET Core forwarded-header middleware with explicit trusted proxy addresses before the rate limiter; never trust arbitrary forwarded headers.
- On API startup, EF Core applies pending migrations from `backend/SupportTickets.Api/Migrations`. This creates or alters schema objects incrementally and preserves existing rows unless a migration explicitly changes or removes them; migrations should be reviewed before deployment. Ensure the configured database account can apply schema changes. Deployment should build the API and frontend separately and serve the frontend static build through a CDN/static host. Swagger is enabled only in Development; health endpoints are `/health/live` and `/health/ready`.

The app supports public account registration and sign-in using its own user table. Passwords are hashed, and the API requires a 30-minute bearer token for ticket endpoints. All registered users have full access to every ticket; do not make this service public unless that access policy is intended. Email verification and password reset are not configured. The frontend keeps the access token in tab-scoped session storage; signing out clears it locally but does not revoke the token before expiry.

## Validation

```powershell
dotnet test .\backend\SupportTickets.Tests\SupportTickets.Tests.csproj
Set-Location .\frontend
npm ci
npm test
npm run lint
npm run format:check
npm run build
```

The full development dependency audit currently reports a high-severity `picomatch` ReDoS advisory affecting the installed `picomatch@4.0.7` and transitive build/test tools; npm currently offers only a forced major downgrade that would replace the supported Vite/Vitest toolchain. No forced downgrade or unverified override was applied. `npm audit --omit=dev` reports zero production dependency vulnerabilities.
