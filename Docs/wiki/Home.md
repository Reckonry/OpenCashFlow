# OpenCashFlow Wiki

OpenCashFlow is a **Developer Preview / Early Self-Hosted Preview** for small businesses that want a self-hosted cash control system.

The current public preview is intended for evaluation, demos, and early feedback. It is **not production-ready**.

## Start Here

- [Docker Compose Install](Docker-Compose-Install)
- [OpenCashFlow releases](https://github.com/Reckonry/OpenCashFlow/releases)
- [Repository README](https://github.com/Reckonry/OpenCashFlow#readme)

## What The Preview Includes

- Docker Compose release package with `db`, `api`, and `webapp` services.
- Published GHCR images for API and WebApp.
- First-run setup wizard for a clean database.
- Generated temporary administrator password shown once.
- Forced password change on first login.
- Setup lock after the instance is configured.

## What It Does Not Include Yet

- Production deployment guarantee.
- In-app PostgreSQL provisioning.
- Full Cash Custody persistence.
- Stable release support policy.
- Enterprise support or SLA.

## Wiki Source

The canonical source for these Wiki pages is kept in the main repository under:

```text
Docs/wiki/
```

Visual assets used by the Wiki are prepared under:

```text
Docs/assets/setup/
```

Publish Wiki updates manually after reviewing the rendered Markdown.
