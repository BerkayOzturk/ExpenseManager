# Security

## Reporting a vulnerability

If you discover a security issue in this project, please report it responsibly.

- **Do not** open a public GitHub issue for security vulnerabilities.
- Email the maintainers (or open a private security advisory on GitHub if this repo has that enabled) with a description of the issue, steps to reproduce, and any suggested fix if you have one.
- We will acknowledge your report and work on a fix. We may ask for more detail.

## Security-related configuration

- **Secrets**: Do not commit API keys, JWT signing keys, connection strings, or the admin secret. Use environment variables or a secret manager (see [DEPLOYMENT.md](DEPLOYMENT.md)).
- **JWT**: Set a long, random `Jwt__Key` in production. Rotate it if it may have been exposed.
- **Admin**: The optional secret admin (e.g. `/admin` page) is protected by `Admin__SecretKey` (env). Set it only on the host; never in code or GitHub. If unset, admin routes are disabled.
- **CORS**: Configure allowed origins for the API in production so only your frontend origin can call it.
- **HTTPS**: In production, serve the app over HTTPS and configure the API and frontend accordingly.
