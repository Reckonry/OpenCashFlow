# Upgrade And Migration Drill

OpenCashFlow uses EF Core migrations in `OpenCashFlow.Infrastructure`. This document defines the minimum migration drill
expected before a production release.

The project is not production-ready until upgrade and rollback behavior is proven against a production-like database
copy.

## Production Migration Policy

Recommended production settings:

- set `AUTO_MIGRATE=false`;
- take a database backup before applying migrations;
- run migrations as an explicit release step;
- record migration IDs before and after;
- validate application health after migrations;
- keep rollback expectations documented.

Automatic startup migrations are acceptable for local evaluation and disposable smoke tests, but they are risky in
production because application startup can alter schema.

## Inspection Commands

List migrations:

```bash
dotnet ef migrations list \
  --project src/OpenCashFlow.Infrastructure/OpenCashFlow.Infrastructure.csproj \
  --startup-project src/OpenCashFlow.API/OpenCashFlow.API.csproj
```

Record applied migrations from a running database:

```bash
docker compose exec -T db psql -U postgres -d opencashflow \
  -tAc 'select "MigrationId" from "__EFMigrationsHistory" order by "MigrationId";'
```

## Drill Plan

For each release candidate:

1. Restore a recent production backup into a staging database.
2. Record current migration IDs.
3. Apply pending migrations with the release build.
4. Start API and WebApp against the migrated copy.
5. Run clean-install or post-upgrade smoke checks.
6. Verify critical flows:
   - login;
   - setup status;
   - payment list/detail;
   - payment create/update/delete;
   - cash balance/ledger;
   - employee/company access;
   - audit log write path.
7. Record migration IDs after upgrade.
8. Decide rollback path:
   - restore backup for destructive migrations;
   - or apply explicit down migration only if it has been tested.

## Local Result - 2026-07-08

Status: **not fully performed in this branch**.

Not performed:

- clean install with an isolated disposable smoke stack;
- upgrade from an older released database snapshot;
- rollback test;
- migration against a copied production-like dataset.

Reason:

- no stable production release or archived production-like database snapshot exists in the repository;
- the current `development` branch does not contain a dedicated isolated clean-install smoke stack suitable as a
  migration drill harness;
- the root Docker Compose file is suitable for local evaluation, but its fixed container names and standard ports make
  destructive automated migration drills unsafe on shared developer machines.

Future exact test plan:

1. Preserve a database dump from the previous release candidate.
2. Restore it into a disposable PostgreSQL instance.
3. Run the new release with `AUTO_MIGRATE=false`.
4. Apply migrations explicitly.
5. Run the clean-install smoke equivalent against the upgraded data.
6. Restore the pre-upgrade dump to prove rollback by restore.

Until this drill is performed with a real prior schema/data snapshot, production upgrade readiness remains unproven.
