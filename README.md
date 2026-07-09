![OpenCashFlow](.github/OCF_Banner.png)

# OpenCashFlow

OpenCashFlow is an open-source, self-hosted cash-flow management system for small companies, consultants, accounting
studios, and teams that need a transparent way to track payments, cash movements, users, company setup, audit events,
and basic operational reporting.

## Project Status

**Developer Preview / Early Self-Hosted Preview**

OpenCashFlow is not production-ready yet. The repository is being stabilized in public with a focus on self-hosted core
workflows, clean architecture, security hardening, test coverage, and open-source project hygiene.

Current preview package: `0.1.0-preview.2`. See
[Docs/releases/0.1.0-preview.2.md](Docs/releases/0.1.0-preview.2.md).

Use it for evaluation, local development, architecture review, and early feedback. Do not use it for regulated or
business-critical financial operations until the release blockers in [Docs/ROADMAP.md](Docs/ROADMAP.md) are resolved.

## What OpenCashFlow Is

- A self-hosted ASP.NET Core and PostgreSQL application.
- A core cash-flow tool for payments, cash ledger, company setup, users, roles, dashboard, audit, and basic exports.
- A developer-preview codebase moving toward a clean architecture split:
  - `Domain`
  - `Application`
  - `Infrastructure`
  - `Contracts`
  - `API`
  - `WebApp`
- AGPL-licensed software intended to remain useful without mandatory SaaS services.

## What OpenCashFlow Is Not

- Not a production-ready accounting suite.
- Not a certified fiscal, tax, payroll, or invoicing product.
- Not a hosted SaaS that requires subscriptions to run.
- Not dependent on Stripe, Billing, pricing plans, customer portal flows, or a separate Admin runtime.
- Not a replacement for professional accounting review.

Historical Billing/Stripe/Admin artifacts may still appear in migration notes or schema-compatibility documentation.
They are legacy references, not active core runtime features.

## Current Scope

The self-hosted core currently focuses on:

- first-instance setup;
- company/workspace data;
- users, roles, and permissions;
- payment registration;
- cash ledger and balances;
- dashboard views;
- audit trail;
- basic API/WebApp operation;
- Docker Compose based local evaluation.

Planned module work is documented in [Docs/modules/architecture.md](Docs/modules/architecture.md).

## Tech Stack

- .NET 10
- ASP.NET Core
- Entity Framework Core
- PostgreSQL
- Bootstrap and jQuery in the WebApp
- Docker Compose for local evaluation

## Quickstart

### Requirements

- .NET 10 SDK
- Docker and Docker Compose
- PostgreSQL 16+ if running manually without Docker

### Build And Test

```bash
git clone https://github.com/Reckonry/OpenCashFlow.git
cd OpenCashFlow

dotnet restore OpenCashFlow.sln
dotnet build OpenCashFlow.sln --configuration Release --no-restore
dotnet test OpenCashFlow.sln --configuration Release --no-build
```

### Run With Docker Compose

```bash
cp .env.example .env
docker compose up --build
```

Local endpoints:

- WebApp: `http://localhost:5200`
- API health: `http://localhost:5100/health`
- PostgreSQL: `localhost:5432`

The default Docker Compose configuration is for local evaluation. Change secrets, database credentials, TLS, backups,
reverse proxy configuration, and operational settings before exposing any instance.

On a fresh database, opening the WebApp redirects to `/Setup`. The first-run wizard creates the first company and admin
user, generates a temporary password, shows it once, and then requires a password change after login. See
[Docs/setup/first-run-setup-wizard.md](Docs/setup/first-run-setup-wizard.md).

The application does not provision PostgreSQL users or databases from the WebApp. Database connectivity must already be
provided by Docker Compose, environment variables, or host configuration.

### Run A Preview Package

For tagged preview releases, download the package from GitHub Releases:

```bash
curl -LO https://github.com/Reckonry/OpenCashFlow/releases/download/v0.1.0-preview.2/OpenCashFlow-0.1.0-preview.2.tar.gz
# or:
# wget https://github.com/Reckonry/OpenCashFlow/releases/download/v0.1.0-preview.2/OpenCashFlow-0.1.0-preview.2.tar.gz
tar -xzf OpenCashFlow-0.1.0-preview.2.tar.gz
cd OpenCashFlow-0.1.0-preview.2
cp .env.example .env
docker compose -f docker-compose.release.yml up -d
```

The release Compose file uses published GHCR images instead of building from source:

- `ghcr.io/reckonry/opencashflow-api:v0.1.0-preview.2`
- `ghcr.io/reckonry/opencashflow-webapp:v0.1.0-preview.2`

On a fresh database, open `http://localhost:5200` and complete the first-run setup wizard.

### Run Manually

Configure `DEFAULT_CONN_STRING` or `ConnectionStrings:DefaultConnectionString`, then run:

```bash
dotnet run --project src/OpenCashFlow.API/OpenCashFlow.API.csproj
dotnet run --project src/OpenCashFlow.WebApp/OpenCashFlow.WebApp.csproj
```

Installation notes are in [Docs/installation.md](Docs/installation.md).

## Architecture

The active solution contains:

- `src/OpenCashFlow.API`
- `src/OpenCashFlow.Application`
- `src/OpenCashFlow.Contracts`
- `src/OpenCashFlow.Domain`
- `src/OpenCashFlow.Infrastructure`
- `src/OpenCashFlow.WebApp`
- `tests/*`

The current dependency shape is documented in
[Docs/architecture/current-dependencies.md](Docs/architecture/current-dependencies.md).

## Security

OpenCashFlow is still an early preview. Security-sensitive areas such as authorization, tenant isolation, password reset,
PIN/fast-login flows, dependency security, CSP, and deployment hardening are active stabilization areas.

Before reporting a vulnerability, read [SECURITY.md](SECURITY.md). Do not open public issues containing secrets, tokens,
passwords, private deployment details, or exploitable vulnerability details.

## Contributing

Contributions are welcome, especially around tests, documentation, security hardening, setup reliability, and core
self-hosted workflows.

Read [CONTRIBUTING.md](CONTRIBUTING.md) before opening a pull request. For smaller first contributions, prefer:

- documentation corrections;
- missing test coverage;
- issue reproduction cases;
- cleanup of legacy references;
- small UI maintainability improvements.

## Roadmap

See [Docs/ROADMAP.md](Docs/ROADMAP.md). The roadmap is intentionally conservative and does not claim production
readiness.

## Release And Versioning

There are no stable releases yet. Until the project reaches a first stable release, changes may be breaking and migration
paths may be incomplete.

The intended future policy is Semantic Versioning:

- `MAJOR` for incompatible API/database/runtime changes;
- `MINOR` for backwards-compatible features;
- `PATCH` for fixes and security updates.

Release notes will be tracked in [CHANGELOG.md](CHANGELOG.md).

## License

OpenCashFlow is licensed under the GNU Affero General Public License v3.0. See [LICENSE](LICENSE).

If you run a modified version as a network service, the AGPL requires you to provide the corresponding source code to
users of that service.
