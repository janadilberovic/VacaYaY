# VacaYAY Web

Next.js (App Router) + TypeScript frontend for the VacaYAY leave-management API. It implements
the `VacaYAY Employee Dashboard` design: employee dashboard, leave requests, HR review queue,
HR analytics, and employee / leave-type management — with light/dark theming.

The UI is fully client-rendered and talks to the existing .NET API; it holds no business logic
of its own. Auth is JWT bearer stored in `localStorage`.

## Prerequisites

- Node 20+
- The VacaYAY API running locally (see the repo root `CLAUDE.md` for its user-secrets setup:
  `Jwt:SigningKey` and `ConnectionStrings:DefaultConnection`, plus a running MySQL).

## Run

1. Start the API from the repo root (listens on http `:5266`, https `:7273`):

   ```
   dotnet run --project src/VacaYAY.Api
   ```

2. Start the web app:

   ```
   cd web
   npm install
   npm run dev
   ```

   Open http://localhost:5173 and sign in with an account HR provisioned. First login prompts
   a password change.

### How it reaches the API

`npm run dev` proxies same-origin `/api/*` requests to the API (default `http://localhost:5266`,
the profile `dotnet run` uses), so there are no CORS or cert prompts in the browser. To target the
https profile instead, set `API_TARGET`:

```
API_TARGET=https://localhost:7273 npm run dev
```

The dev script sets `NODE_TLS_REJECT_UNAUTHORIZED=0` so the proxy accepts the self-signed dev cert
when using the https target. For production, the API also has a CORS policy — set allowed origins
via `Cors:AllowedOrigins` in the API config.

## Scripts

- `npm run dev` — dev server on `:5173` with the `/api` proxy
- `npm run build` — production build
- `npm run start` — serve the production build on `:5173`
- `npm run typecheck` — `tsc --noEmit`

## Notes / known limitations

- **Archived lists.** The API's `GET /employees` and `GET /leave-types` apply the soft-delete
  filter, so archived rows aren't returned. Archive/restore work, and this UI shows the item under
  an "Archived" section for the rest of the session, but archived items won't reappear after a
  reload (there's no list endpoint that includes them). Surfacing them would need an API change.
- **Working-day preview.** The request modal shows a weekend-only estimate while you pick dates;
  the authoritative figure (which also excludes Serbian public holidays) comes back on the created
  request from the API.
- **Employee role field.** The design's create form omits a role, but the API requires one, so the
  Add-employee modal includes a Role select (defaults to Employee).
