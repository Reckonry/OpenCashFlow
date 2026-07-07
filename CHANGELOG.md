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

## Versioning Policy

No stable `1.0.0` release has been published yet.

Future release intent:

- `MAJOR`: incompatible public API, database, deployment, or contract changes.
- `MINOR`: backwards-compatible features or module additions.
- `PATCH`: bug fixes, documentation fixes, and security updates.

Until `1.0.0`, breaking changes may occur in minor or preview releases and must be called out in this changelog.
