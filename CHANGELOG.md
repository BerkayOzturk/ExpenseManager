# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [Unreleased]

### Added
- Recurring expenses (monthly) with upcoming view
- Responsive tables (card layout on small screens)
- Touch-friendly buttons and nav
- PWA support (installable, offline list cache)
- Mobile nav: user and logout on separate row
- Quick date filters (This month, Last month, This year) on Expenses
- Spending-over-time bar chart by month
- Budgets: budget vs spent, over-budget highlight
- Settings: default currency, date format, theme, language
- i18n: English and Turkish
- JWT authentication and user-scoped data
- Categories, Expenses, Budgets, Recurring, Settings
- Onion Architecture backend (.NET 8), React + Vite frontend

### Documentation & project
- ARCHITECTURE.md, DEPLOYMENT.md
- .editorconfig, .gitignore
- GitHub Actions CI (backend build, frontend lint + build)
- Docker and docker-compose
- LICENSE (MIT), SECURITY.md, CONTRIBUTING.md
- Issue and PR templates, CHANGELOG
- Rebrand: **Coin Canvas** (app name, PWA manifest, JWT issuer/audience, docs)
