# OpenCashFlow Roadmap

OpenCashFlow is in **Developer Preview / Early Self-Hosted Preview**.

This roadmap is intentionally conservative. It documents what must improve before the project should be presented as a
serious public self-hosted product.

## Current Status

- Core solution builds on .NET 10.
- API, WebApp, Domain, Application, Infrastructure, Contracts, and tests are separated.
- Stripe/Billing/Admin SaaS runtime has been removed from the core runtime.
- Historical Billing/Stripe schema and migration notes may remain for compatibility.
- Docker Compose supports local evaluation.
- The project is not production-ready.

## Short-Term Priorities

1. Database integration test migration
   - Move historical database-level tests into an explicit integration-test suite or archive them with evidence.
   - Prefer a repeatable Testcontainers-backed path for persistence constraints, tenant isolation, and migration checks.
   - Keep excluded DB test coverage visible until it is migrated or intentionally retired.

2. Production hardening
   - Validate TLS/reverse proxy, cookie/JWT settings, SMTP, logging, rate limiting, and operational defaults.
   - Keep Docker Compose defaults clearly scoped to local evaluation.
   - Turn production-readiness claims into evidence-backed checks before any stable release.

3. Backup/restore drill
   - Execute a real backup and restore against a fresh database.
   - Document RPO/RTO assumptions and operator steps.
   - Verify restored instances can pass a minimal smoke test.

4. Upgrade/migration drill
   - Exercise EF migrations against a copied database.
   - Document rollback expectations and release migration notes.
   - Keep schema changes out of stable releases unless migration behavior is proven.

5. GitHub alert closeout
   - Reconcile GitHub dependency alerts against the default branch and current dependency files.
   - Classify any remaining alerts by NuGet, Docker base image, GitHub Action, frontend package, or stale removed file.
   - Keep advisory status documented until the GitHub Security tab is clean or intentionally dismissed.

6. Frontend/static asset cleanup
   - Continue extracting inline Razor scripts/styles into versioned static assets.
   - Remove remaining CDN dependencies where local assets are available.
   - Keep CSP free of `unsafe-inline` and `unsafe-eval`.

7. Infrastructure/Contracts boundary reduction
   - Review Infrastructure dependencies on public Contracts and remove DTO coupling where practical.
   - Keep Contracts limited to stable API/shared boundary types.
   - Keep EF entities owned by Infrastructure.

## Medium-Term Direction

- Modular architecture for optional features.
- Advanced reporting and export hardening.
- Forecasting module exploration.
- Italian e-invoicing as an optional module, not a core blocker.
- Better operational docs for upgrades and backups.

## Not Production Ready Yet

Before a production-ready claim, the project needs:

- explicit integration-test handling for database-level coverage;
- documented backup and restore process;
- verified backup and restore drill;
- verified upgrade/migration drill;
- production secrets and hardening guidance validated against real deployment settings;
- security review of auth, reset-password, PIN/fast-login, tenant isolation, and authorization;
- at least one repeatable full Docker smoke test;
- GitHub dependency alert closeout;
- clear release artifacts and versioning policy.

## Release Policy

There are no stable releases yet. Future stable releases should use Semantic Versioning and maintain a changelog entry
for every release.
