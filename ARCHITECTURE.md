# Architecture

Expense Manager uses **Onion Architecture** (Clean Architecture style) on the backend and a simple React SPA on the frontend.

## Backend layers

| Layer | Project | Role | Depends on |
|-------|---------|------|------------|
| **Domain** | `ExpenseManager.Domain` | Entities, value objects, domain rules and exceptions | None |
| **Application** | `ExpenseManager.Application` | Use cases (CQRS via MediatR), validation (FluentValidation), ports (repository interfaces, `IUnitOfWork`, `IAuthService`, `ICurrentUserService`) | Domain only |
| **Infrastructure** | `ExpenseManager.Infrastructure` | EF Core (SQLite), Identity, JWT, repositories, `DatabaseInitializer` | Domain, Application |
| **API** | `ExpenseManager.API` | Controllers, Swagger, middleware, pipeline behaviors, `ICurrentUserService` implementation | Application, Infrastructure (no direct Domain references) |

- **Dependency rule**: Dependencies point inward. Domain has no dependencies. Application references only Domain. Infrastructure and API reference Application (and Domain only via Application types where needed).
- **Domain exceptions** are caught in the Application pipeline and rethrown as application-level exceptions so the API does not depend on the Domain assembly.
- **Persistence**: Repositories and `IUnitOfWork` live in Application (interfaces) and Infrastructure (implementations). DbContext is in Infrastructure.

## Key patterns

- **CQRS**: Commands and queries via MediatR; one handler per command/query.
- **Validation**: FluentValidation in the Application layer; validated before handlers run.
- **Auth**: JWT bearer; user ID from `ICurrentUserService`; all data scoped by `UserId`.

## Frontend

- **React 18 + TypeScript + Vite** in `frontend/`.
- **Routing**: React Router; public routes (login, register) and protected routes (app shell with nav).
- **State**: React context for auth and settings; local state in pages.
- **API**: Fetch to same-origin `/api` (proxied to backend in dev); JWT in `Authorization` header.
- **i18n**: Simple translation map (EN/TR) driven by user settings.

## Where to add things

- **New entity**: Domain entity → Application repository interface + DTOs and commands/queries → Infrastructure repository + DbContext → API controller.
- **New endpoint**: Application command/query + handler → API controller action.
- **New frontend page**: Add route in `App.tsx`, add nav link, create page under `frontend/src/pages/`.
