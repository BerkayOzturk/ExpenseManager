# Contributing

Thanks for your interest in contributing to Expense Manager.

## Getting started

1. **Fork and clone** the repository.
2. **Run locally**:
   - Backend: `dotnet run --project src/ExpenseManager.API/ExpenseManager.API.csproj`
   - Frontend: `cd frontend && npm install && npm run dev`
   - See [README.md](README.md) for details.
3. **Architecture**: Read [ARCHITECTURE.md](ARCHITECTURE.md) to see where to add or change code.

## Making changes

- Create a **branch** (e.g. `feature/short-name` or `fix/issue-description`).
- Follow the existing **code style** (EditorConfig and project conventions).
- **Backend**: Keep Onion Architecture; add commands/queries in Application, implementations in Infrastructure or API as appropriate.
- **Frontend**: Use existing patterns (hooks, API client, translations).

## Before submitting

- **Build**: `dotnet build ExpenseManager.sln` and `cd frontend && npm run build` both succeed.
- **Lint**: `npm run lint` in `frontend/` passes.
- **Commit messages**: Use clear, short messages (e.g. `feat: add recurring expenses`, `fix: mobile nav logout`).

## Pull requests

- Open a PR against `main` or `develop` (see repo default).
- Describe what changed and why.
- CI will run build and lint; ensure it passes.

## Questions

Open a GitHub Discussion or an issue if you have questions or suggestions.
