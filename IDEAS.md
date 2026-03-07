# Expense Manager – Ideas to develop further

Pick what fits your goals (learning, portfolio, or real use). Mix backend, frontend, and product ideas as you like.

---

## Backend (.NET / Onion)

- **Authentication & users**  
  Add JWT (or Identity) so each user only sees their own categories and expenses. Register/login endpoints, user-scoped repositories or tenant filter in queries.

- **Pagination & sorting**  
  `ListExpenses` and `ListCategories`: add `page`, `pageSize`, `sortBy`, `sortDirection` so the API scales and the frontend can paginate.

- **EF Core migrations**  
  Replace `EnsureCreated` with proper migrations (`Add-Migration`, `Update-Database`) so you can evolve the schema without losing data.

- **Recurring expenses**  
  New entity e.g. `RecurringExpense` (amount, interval, category, start/end). A job or background service creates `Expense` records from recurrences (e.g. daily/weekly/monthly).

- **Budgets**  
  Entity e.g. `Budget` (category or “total”, amount, period: month/year). API to get budget vs spent; optional alerts when over budget.

- **Export & reporting**  
  Endpoints that return CSV or PDF (e.g. expenses in a date range, by category). Use a library for PDF generation.

- **Soft delete**  
  Add `DeletedAtUtc` (or `IsDeleted`) to entities; filter out deleted rows in queries; no physical delete (or only after a retention period).

- **Audit / history**  
  Log who changed what and when (e.g. audit table or event sourcing lite). Useful for “undo” or compliance.

- **Tests**  
  Unit tests for domain and application (handlers, validators); integration tests for API or repositories against a test DB.

- **Docker & deployment**  
  Dockerfile for API (and optionally frontend); docker-compose with API + DB; deploy to Azure/AWS or a VPS.

---

## Frontend (React)

- **Auth flow**  
  Login/register pages, store token (e.g. localStorage or httpOnly cookie), send `Authorization` header, protect routes (redirect to login when not authenticated).

- **Dashboard**  
  Summary cards: total spent (filtered period), by category, comparison vs previous period. Reuse your existing chart data or add a small “summary” API.

- **Charts**  
  Keep pie by category; add a bar/line chart “spending over time” (e.g. by month or week) using the same Recharts or another library.

- **Budgets in the UI**  
  If you add budgets on the backend: set budget per category (or global), show progress bars and a simple “over budget” warning.

- **Recurring expenses UI**  
  List/create/edit recurring expenses; optional “upcoming” view (next 3 months of generated expenses).

- **Better UX**  
  Loading skeletons, toast notifications (e.g. “Expense added”), confirm dialogs for delete, optimistic updates (update list before API responds).

- **Export from UI**  
  “Export to CSV” button that calls your export API or builds CSV client-side from current list.

- **Settings**  
  Default currency, date format, theme (e.g. dark mode), optional language.

- **Mobile-friendly**  
  Responsive tables (cards on small screens), touch-friendly buttons, optional PWA (installable, offline list cache).

---

## Product / features (backend + frontend)

- **Tags**  
  In addition to one category per expense, allow multiple tags (e.g. “work”, “reimbursable”). New entity + many-to-many; filter and group by tag in lists and charts.

- **Receipts / attachments**  
  Store file references (or blobs) per expense; upload from UI; show thumbnail or link in expense detail.

- **Multiple “wallets” or accounts**  
  E.g. Cash, Bank, Card. Each expense belongs to one; dashboard shows balance per wallet and total.

- **Currency conversion**  
  For “total spent” across currencies: store exchange rates (table or external API) and show one total in a chosen currency; pie or bar “by category” in base currency.

- **Saving goals**  
  E.g. “Vacation: 2000 EUR”. Track progress; optional “transfer” from “expenses” to “saved” (or separate goal entity).

- **Notifications**  
  “Over budget” or “recurring expense due” via in-app badge or email (backend job + email sender).

- **Import**  
  Import from CSV or bank export (mapping columns to amount, date, category); duplicate detection.

---

## Quick wins (small scope)

1. **Total and count in list response**  
   API returns `{ items, totalCount }` for expenses; frontend shows “Showing 1–20 of 45”.

2. **Date range presets**  
   Frontend: “This month”, “Last month”, “This year” buttons that set From/To.

3. **Search**  
   Filter expenses by description (backend: `description.Contains(query)`; frontend: search box).

4. **Remember filters**  
   Store From/To/category in `localStorage` or URL query so the view is restored on reload.

5. **Category order**  
   Add `SortOrder` or `DisplayOrder` to Category; sort categories in dropdowns and lists.

Use this file as a backlog: pick one or two items, implement them, then choose the next. If you tell me your priority (e.g. “auth first” or “dashboard + charts”), I can outline concrete steps or code changes for that part.
