# Coin Canvas (.NET 8, Onion Architecture)

**Coin Canvas** is an expense-tracking app. Backend with strict Onion Architecture:

| Layer | Role | Dependencies |
|-------|------|--------------|
| **Domain** | Entities, value objects, domain rules | None |
| **Application** | Use-cases (CQRS/MediatR), validation, persistence ports (interfaces) | Domain only |
| **Infrastructure** | EF Core SQLite, repositories, DB init | Domain, Application |
| **API** | Controllers, Swagger, middleware, pipeline | Application, Infrastructure only (no Domain) |

Domain exceptions are converted to application exceptions in the Application pipeline so the API never references Domain.

**Documentation**

- [ARCHITECTURE.md](ARCHITECTURE.md) – Layers, patterns, and where to add new features.
- [DEPLOYMENT.md](DEPLOYMENT.md) – Build, run, and deploy (including Docker and **going public** with Railway/Render).
- [MOBILE.md](MOBILE.md) – Build and run the **mobile app** (iOS/Android) with Capacitor.
- [CONTRIBUTING.md](CONTRIBUTING.md) – How to contribute.
- [SECURITY.md](SECURITY.md) – How to report security issues.

---

## User's view: how to use this project

### 1. Run the API

From the repo root:

```bash
dotnet build ExpenseManager.sln
dotnet run --project src/ExpenseManager.API/ExpenseManager.API.csproj
```

- The API listens at **http://localhost:5032** (see `Properties/launchSettings.json`; change the proxy in `frontend/vite.config.ts` if you use another port)
- The database file `expensemanager.db` is created automatically in the API project directory on first run.
- **After enabling auth:** If you had an existing `expensemanager.db` from before JWT was added, delete it so the app can recreate the schema (with Identity and `UserId` on categories/expenses).
- **After adding Budgets:** If the database was created before the Budgets feature, delete `expensemanager.db` so the app can recreate the schema including the `Budgets` table.

### 2. Authentication (JWT)

- **Register:** `POST /api/auth/register` with `{ "email": "...", "password": "..." }` (password min 6 chars, requires digit, upper and lower case).
- **Login:** `POST /api/auth/login` with `{ "email": "...", "password": "..." }`. Both return `{ "userId", "email", "token" }`.
- **Protected endpoints:** Categories and expenses require `Authorization: Bearer <token>`. Each user only sees their own data.

### 3. Open Swagger (interactive UI)

In a browser go to:

**http://localhost:5032/swagger**

From there you can:

- **Categories**: list all, create, get by id, update, delete
- **Expenses**: list (with optional filters), create, get by id, update, delete

### 4. Example API usage

All category and expense requests require the JWT: `Authorization: Bearer <token>` (from login/register).

**Create a category**

```http
POST http://localhost:5032/api/categories
Content-Type: application/json
Authorization: Bearer <your-token>

{ "name": "Food" }
```

Response: `201` with body like `{ "id": "...", "name": "Food" }`.

**Create an expense**

```http
POST http://localhost:5032/api/expenses
Content-Type: application/json

{
  "amount": 25.50,
  "currency": "USD",
  "occurredOn": "2026-03-07",
  "description": "Lunch",
  "categoryId": "<category-guid-from-above>"
}
```

`categoryId` is optional. Dates use `YYYY-MM-DD`.

**List expenses (optional filters)**

```http
GET http://localhost:5032/api/expenses?from=2026-03-01&to=2026-03-31&categoryId=<guid>
```

**List categories**

```http
GET http://localhost:5032/api/categories
```

**Budgets**

- `GET /api/budgets` – list all budgets
- `GET /api/budgets/summary` – list budgets with spent amount and over-budget flag
- `GET /api/budgets/{id}` – get one budget
- `POST /api/budgets` – body: `{ "categoryId": null | "<guid>", "amount": 500, "currency": "USD", "year": 2026, "month": 3 | null }` (omit or null `month` for yearly budget)
- `PUT /api/budgets/{id}` – body: `{ "amount": 600, "currency": "USD" }`
- `DELETE /api/budgets/{id}` – delete a budget

**Settings** (per user)

- `GET /api/settings` – get current user’s settings (default currency, date format, theme, language)
- `PUT /api/settings` – body: `{ "defaultCurrency": "USD", "dateFormat": "yyyy-MM-dd", "theme": "light" | "dark" | "system", "language": null | "en" }`

### 5. Error responses

- **400** – Validation or domain rule failed (body describes errors)
- **404** – Category or expense not found
- **500** – Unexpected server error

All error responses are JSON (e.g. Problem Details).

---

## Frontend (React + Vite)

The frontend is in the `frontend` folder.

**Run the app**

1. Start the backend API (see above) on **http://localhost:5032**.
2. From the repo root:

```bash
cd frontend
npm install
npm run dev
```

3. Open **http://localhost:5173** in the browser.

Vite proxies `/api` to the backend, so the app talks to the same-origin API.

**What you can do**

- **Log in / Register**: Create an account or sign in; the app stores the JWT and sends it with every request. Each user only sees their own categories and expenses.
- **Expenses** (home): Filter by date range and category, add/edit/delete expenses (amount, currency, date, category, description).
- **Categories**: Add, edit, and delete categories; expenses can be linked to a category.
- **Budgets**: Set a budget per category or for total spending, by month or year. View budget vs spent; over-budget rows are highlighted.
- **Settings**: Default currency (used when adding expenses/budgets), date format, theme (light/dark/system), and optional language.
