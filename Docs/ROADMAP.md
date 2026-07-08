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

1. Repository hygiene
   - Remove tracked local/internal residue.
   - Remove or archive legacy Admin source outside active `src`.
   - Keep only sanitized environment examples.

2. Test reliability
   - Reduce skipped tests.
   - Document remaining skipped tests with explicit risk and required fixes.
   - Prioritize authorization, tenant isolation, company CRUD, payment isolation, cash ledger, and setup.

3. Security posture
   - Keep dependency advisories at zero.
   - Continue CSP and frontend hardening.
   - Document threat model, secrets management, backup/restore, and production hardening.

4. Developer experience
   - Improve quickstart reliability.
   - Add conservative CI quality gates.
   - Document common local troubleshooting paths.

5. UI maintainability
   - Split very large Razor views.
   - Move inline scripts/styles into static assets where practical.
   - Preserve existing behavior while reducing maintenance risk.

6. Architecture cleanup
   - Continue moving orchestration out of API services.
   - Keep WebApp independent from Infrastructure.
   - Keep Contracts limited to stable API/shared contracts.

## Medium-Term Direction

- Modular architecture for optional features.
- Advanced reporting and export hardening.
- Forecasting module exploration.
- Italian e-invoicing as an optional module, not a core blocker.
- Better operational docs for upgrades and backups.

## Not Production Ready Yet

Before a production-ready claim, the project needs:

- no high-risk skipped tests without tracked justification;
- documented backup and restore process;
- documented upgrade/migration policy;
- production secrets guidance;
- security review of auth, reset-password, PIN/fast-login, tenant isolation, and authorization;
- at least one repeatable full Docker smoke test;
- clear release artifacts and versioning policy.

## Release Policy

There are no stable releases yet. Future stable releases should use Semantic Versioning and maintain a changelog entry
for every release.
