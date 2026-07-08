# Contributing To OpenCashFlow

Thank you for considering a contribution to OpenCashFlow.

OpenCashFlow is currently a **Developer Preview / Early Self-Hosted Preview**. Contributions should improve reliability,
clarity, security, and maintainability without pretending the project is production-ready.

## Good Contribution Areas

- Fix or reduce skipped tests.
- Improve setup and self-hosted documentation.
- Add focused coverage for authorization, tenant isolation, payments, cash ledger, and setup.
- Remove legacy SaaS/Admin/Stripe references from active product surfaces.
- Improve UI maintainability without redesigning the product.
- Improve security documentation and dependency hygiene.

## Before You Start

1. Check existing issues and docs.
2. Keep changes small and reviewable.
3. Avoid unrelated refactors.
4. Do not introduce new frameworks without prior discussion.
5. Do not add production-looking secrets, credentials, or environment-specific files.

## Local Checks

Run these before opening a pull request:

```bash
dotnet restore OpenCashFlow.sln
dotnet build OpenCashFlow.sln --configuration Release --no-restore
dotnet test OpenCashFlow.sln --configuration Release --no-build
docker compose config
```

If a check cannot run locally, document the reason in the pull request.

## Pull Request Expectations

- Explain the problem and the chosen fix.
- Mention user-facing behavior changes.
- Include tests for risky logic changes.
- Keep public API and JSON shape stable unless the PR explicitly targets a breaking change.
- Update docs when behavior, setup, security, or project status changes.

## Coding Guidelines

- Preserve the Clean Architecture boundaries:
  - Domain has no EF, ASP.NET, UI, or infrastructure dependencies.
  - Application defines use cases and ports.
  - Infrastructure owns EF, persistence, email, auth implementations, and external integrations.
  - Contracts contains stable API/shared contracts only.
  - WebApp should not depend on Infrastructure.
- Do not reintroduce `OpenCashFlow.Shared`.
- Do not reintroduce Billing/Stripe/Admin runtime into the core.

## Security

Do not disclose vulnerabilities in public issues. Follow [SECURITY.md](SECURITY.md).

## License

By contributing, you agree that your contribution is licensed under the AGPL-3.0 license used by this repository.
