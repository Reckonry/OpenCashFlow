# Changelog

All notable changes to OpenCashFlow will be documented in this file.

This project does not have a stable release yet. The format follows the spirit of
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the project intends to use Semantic Versioning after the
first stable release.

## Unreleased

### Status

- Developer Preview / Early Self-Hosted Preview.
- Not production-ready.
- Public API, database schema, and deployment guidance may still change.

### Current Focus

- Stabilize self-hosted setup.
- Improve test coverage for authorization, tenant isolation, payments, cash ledger, and setup.
- Remove active references to legacy SaaS/Admin/Stripe surfaces.
- Improve documentation and open-source contribution flow.
- Continue Clean Architecture cleanup.

## [0.1.0-preview.1] - 2026-07-09

### Status

- First public Developer Preview / Early Self-Hosted Preview package.
- Not production-ready.
- Intended for local evaluation, contributor review, and early self-hosted feedback.

### Added

- First-run setup wizard for fresh self-hosted installations.
- First company/tenant and first administrator bootstrap flow.
- Strong temporary administrator password generation, shown once after setup.
- First-login password change requirement for the generated administrator password.
- Setup lock after initial configuration.
- Setup documentation and smoke-test report.
- Local preview packaging script under `scripts/release/package-preview.sh`.
- Preview release notes under `Docs/releases/0.1.0-preview.1.md`.

### Fixed

- Added the missing Kerberos/GSSAPI runtime dependency to the API Docker image to remove the
  `libgssapi_krb5.so.2` startup warning from PostgreSQL connection initialization.

### Known Limitations

- OpenCashFlow remains a developer preview and is not suitable for regulated or business-critical production use.
- The application assumes PostgreSQL connectivity is already configured through environment variables, Docker Compose, or host configuration.
- The setup wizard configures application data only; it does not provision PostgreSQL users, databases, or infrastructure.
- Cash Custody domain contracts exist, but persisted multi-cash-account custody is not implemented yet.
- Cash forecast and Safe-to-Pay style decisions should be treated as WIP/experimental until Cash Custody persistence and reconciliation are complete.

## Versioning Policy

No stable `1.0.0` release has been published yet.

Future release intent:

- `MAJOR`: incompatible public API, database, deployment, or contract changes.
- `MINOR`: backwards-compatible features or module additions.
- `PATCH`: bug fixes, documentation fixes, and security updates.

Until `1.0.0`, breaking changes may occur in minor or preview releases and must be called out in this changelog.
