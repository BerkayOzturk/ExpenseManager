# Deployment

This document describes how to build and run Coin Canvas for production or production-like environments.

## Prerequisites

- .NET 8 SDK (backend)
- Node 18+ and npm (frontend build)
- Optionally: Docker and Docker Compose

## Build for production

### Backend

```bash
dotnet publish src/ExpenseManager.API/ExpenseManager.API.csproj -c Release -o ./publish/api
```

Configuration is read from `appsettings.json` and environment variables. Set at least:

- `ConnectionStrings__Default` – SQLite path or other provider connection string
- `Jwt__Key` – Secret key for JWT signing (long, random string)
- `Jwt__Issuer` and `Jwt__Audience` – Optional; defaults in appsettings
- **Optional (social login):** `Google__ClientId` – Google OAuth 2.0 client ID (Web application). Frontend needs the same value as `VITE_GOOGLE_CLIENT_ID` at build time.

Run:

```bash
cd publish/api
./ExpenseManager.API
```

Or with explicit URL:

```bash
./ExpenseManager.API --urls "http://0.0.0.0:5032"
```

### Frontend

```bash
cd frontend
npm ci
npm run build
```

Output is in `frontend/dist/`. Serve the contents with any static file server (e.g. nginx, or the API can host the SPA in production if configured).

Set the API base URL: the app uses relative `/api` when served from the same origin. If the API is on another host in production, configure the frontend to use that API URL (e.g. env at build time or runtime config).

## Docker

**Local (two containers):**

```bash
docker compose up -d
```

- **API**: Port 5032; **Frontend**: port 80. See `docker-compose.yml`.

**Production (one container, API + SPA):**

The root `Dockerfile` builds the frontend and API into a single image. The API serves the SPA from the same origin (no CORS). Use this for public deployment.

```bash
docker build -t expensemanager .
docker run -p 8080:8080 -e Jwt__Key="your-secret-key-min-32-chars" -e ConnectionStrings__Default="Data Source=/app/data/expensemanager.db" -v expensemanager-data:/app/data expensemanager
```

---

## Before going public

- [ ] **JWT secret**: Set `Jwt__Key` on the host to a long random value (e.g. `openssl rand -base64 32`). Do not use the default from appsettings.
- [ ] **Environment**: Host sets `ASPNETCORE_ENVIRONMENT=Production` (Railway/Render do this by default). Swagger is disabled in production.
- [ ] **Database persistence**: On Railway, add a **Volume** with mount path **`/data`** so the SQLite file at `/data/expensemanager.db` persists. Without this, redeploys wipe all users and data.
- [ ] **Custom domain**: Add your domain (e.g. coincanvas.net) in the host’s networking settings and configure DNS as instructed.
- [ ] **Smoke test**: After deploy, test register → login → add expense → log out. Try from a phone browser.
- [ ] **Privacy / terms** (optional): If you collect personal data (email, expenses), consider adding a Privacy policy and Terms of use and link them in the footer.

---

## Going public (one URL)

To let anyone (e.g. your friend on their phone) use the app over the internet:

1. **Use the root `Dockerfile`** – One service serves both API and frontend at the same URL (e.g. `https://yourapp.up.railway.app`).
2. **Deploy to a host** – Push your repo to GitHub, then connect it to one of the options below. Set the required environment variables (especially `Jwt__Key`). The host will build from the root `Dockerfile` and give you a public HTTPS URL.

### Railway

1. Sign up at [railway.app](https://railway.app) and connect GitHub.
2. **New Project** → **Deploy from GitHub repo** → select your Coin Canvas repo.
3. Railway will detect the root `Dockerfile`. If it picks something else, set **Settings → Build → Dockerfile path** to `Dockerfile` (root).
4. **Variables**: Add `Jwt__Key` (long random string, e.g. 32+ chars).
5. **Persist data (required so users and data survive redeploys):**
   - Open your service → **Settings** → **Volumes** (or **Variables** tab and look for Volumes).
   - Click **Add Volume** (or **New Volume**). Set the **mount path** to **`/data`**.
   - The app is configured to store the SQLite database at `/data/expensemanager.db`. Once the volume is mounted at `/data`, all users, passwords (hashes), and expense data will persist across redeploys.
   - If you already deployed without a volume and lost data, add the volume now and redeploy. New data will then persist; old data cannot be recovered.
6. **Settings → Networking**: Generate a **Public domain**. Your app will be at `https://<your-app>.up.railway.app`.
7. Railway usually sets `PORT`; if the app fails to start, add variable `ASPNETCORE_URLS` = `http://0.0.0.0:${PORT}` (or the port Railway shows in the dashboard).

### Render

1. Sign up at [render.com](https://render.com) and connect GitHub.
2. **New → Web Service** → select your repo.
3. **Environment**: Docker. **Dockerfile path**: `Dockerfile` (root).
4. **Environment Variables**: Add `Jwt__Key` (long random string).
5. **Create Web Service**. Render assigns a URL like `https://<name>.onrender.com`.
6. If the app does not listen on Render’s port, set `ASPNETCORE_URLS` = `http://0.0.0.0:10000` (or the port Render shows; often 10000).

### After deploy

- Share the **HTTPS URL** with your friend. They open it in their mobile browser (or desktop), register, and use the app.
- **Optional**: In production you may want to disable Swagger. You can set `ASPNETCORE_ENVIRONMENT=Production` (most hosts do) and in code only add `app.UseSwagger(); app.UseSwaggerUI();` when not Production.

## Environment variables (API)

| Variable | Description |
|----------|-------------|
| `ConnectionStrings__Default` | Database path. In the production Dockerfile the default is `Data Source=/data/expensemanager.db` so that a volume mounted at `/data` keeps data across redeploys. Override here if you use a different path. |
| `Jwt__Key` | Secret for JWT signing |
| `Jwt__Issuer` | Issuer claim (optional) |
| `Jwt__Audience` | Audience claim (optional) |
| `Admin__SecretKey` | Optional. If set, allows access to `/api/admin/*` when request header `X-Admin-Key` matches. Used by the secret admin page to list users. **Never commit this value**; set it only in the host (e.g. Railway variables). |
| `ASPNETCORE_URLS` | Listen URLs (e.g. `http://0.0.0.0:5032`) |

Do not commit secrets. Use a secret manager or environment in your hosting platform.

### Secret admin (optional)

To view a summary of registered users (email, expense count, category count), set `Admin__SecretKey` on the host to a long random value. Then open `https://your-domain/admin`, enter that value when prompted, and use the page. The admin key is **not** stored in the repo or in Docker images; only in the host’s environment. If `Admin__SecretKey` is not set, admin endpoints return 401.

## CORS

If the frontend is served from a different origin than the API, configure CORS in the API to allow that origin (and methods/headers you use). Default development CORS is in `Program.cs`; adjust for production origins.

## Database

- SQLite: ensure the process has write access to the directory containing the database file. For new schema (e.g. after adding features), the app creates missing tables on startup via `DatabaseInitializer`; for major schema changes you may need to delete the DB or run migrations.
- For production at scale, consider switching to SQL Server or PostgreSQL via EF Core and a separate connection string.
