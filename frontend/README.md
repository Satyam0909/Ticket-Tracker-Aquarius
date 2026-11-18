# Frontend

React + TypeScript ticket dashboard. Requires Node.js 24 or later. The source is organized by feature, with shared API, constants, and utilities under `src/shared`.

## Local development

From this directory:

```powershell
npm ci
npm run dev
```

The development server binds to loopback at `http://127.0.0.1:5173` and proxies `/api` to `http://localhost:5080`. Set `VITE_API_BASE_URL` in a local `.env` file when using a separately hosted API; do not put credentials or long-lived access tokens in `VITE_*` variables, because these values are bundled into the browser application.

## Checks

```powershell
npm test
npm run lint
npm run format:check
npm run build
```

`npm run build` performs TypeScript project checks and creates the static production build in `dist/`. The `public/_headers` file provides a baseline for static hosts that support Netlify-style headers; update `connect-src` if the production API is hosted on another origin. Users can register and sign in with app-managed accounts. The API requires a bearer token for all ticket endpoints; tokens are kept in tab-scoped session storage and expire after 30 minutes. Public registration grants full access to all tickets, and email verification/password reset are not configured.
