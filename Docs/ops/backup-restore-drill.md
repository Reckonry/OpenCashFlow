# Backup And Restore Drill

This drill verifies that an OpenCashFlow PostgreSQL database can be backed up, restored into a clean database, and used
by the application after restore.

It is written for local Docker Compose evaluation. Production operators must adapt hostnames, credentials, retention,
encryption, and storage targets to their environment.

## Local Evaluation Drill

Prerequisites for the root Docker Compose stack:

- Docker and Docker Compose;
- a running local evaluation stack started with `docker compose up --build`;
- an explicitly disposable local database volume, or written approval to test against the current local volume.

For an isolated clean-install smoke stack, see `Docs/testing/clean-install-smoke.md` and
`scripts/smoke/clean-install-smoke.sh`.

Representative data should include:

- first company/admin;
- successful login;
- payment;
- cash balance and cash ledger entry.

## Automated Isolated Smoke Drill

The preferred local drill uses the disposable clean-install smoke stack and does not touch the default local
`docker-compose.yml` database volume:

```bash
scripts/smoke/backup-restore-smoke-drill.sh
```

The script:

1. runs `scripts/smoke/clean-install-smoke.sh` with `SMOKE_KEEP_STACK=1`;
2. creates a PostgreSQL custom-format backup from the smoke database;
3. restores it into a second database named `opencashflow_restore` in the same isolated PostgreSQL container;
4. verifies representative company, payment and cash ledger data;
5. destroys only the disposable smoke stack and volume unless `BACKUP_RESTORE_KEEP_STACK=1` is set.

Useful overrides:

```bash
SMOKE_API_PORT=16100 \
SMOKE_WEB_PORT=16200 \
SMOKE_DB_PORT=16432 \
BACKUP_RESTORE_DB=opencashflow_restore_check \
scripts/smoke/backup-restore-smoke-drill.sh
```

## Commands

Start the local evaluation stack:

```bash
docker compose up --build
```

Create a dump from the local database:

```bash
mkdir -p reports/ops
docker compose exec -T db pg_dump -U postgres -d opencashflow --format=custom \
  > reports/ops/opencashflow-local.dump
```

Create a restore target database inside the same disposable PostgreSQL container:

```bash
docker compose exec -T db createdb -U postgres opencashflow_restore
```

Restore the dump:

```bash
docker compose exec -T db pg_restore -U postgres -d opencashflow_restore --clean --if-exists \
  < reports/ops/opencashflow-local.dump
```

Verify representative restored data:

```bash
docker compose exec -T db psql -U postgres -d opencashflow_restore \
  -tAc 'select count(*) from "Companies"; select count(*) from "Payments"; select count(*) from "CashLedgers";'
```

Expected result after the smoke data load:

```text
1
1
1
```

Clean up:

```bash
docker compose exec -T db dropdb -U postgres opencashflow_restore
```

## Local Result - 2026-07-08

Status: **performed against the isolated clean-install smoke stack**.

Script:

```bash
scripts/smoke/backup-restore-smoke-drill.sh
```

Backup command used:

```bash
pg_dump -U postgres -d opencashflow --format=custom
```

Restore command used:

```bash
pg_restore -U postgres -d opencashflow_restore --clean --if-exists
```

Verification queries:

```sql
select count(*) from "Companies";
select count(*) from "Payments";
select count(*) from "CashLedgers";
select count(*) from "Companies"
where "CompanyName" = 'OpenCashFlow Backup Restore Smoke Company';
select count(*) from "Payments"
where "Description" = 'Clean install smoke payment';
select count(*) from "CashLedgers"
where "RefType" = 'Payment' and "Delta" = 25.75;
```

Result:

```text
Companies: 1
Payments: 1
CashLedgers: 1
SmokeCompanyMatches: 1
SmokePaymentMatches: 1
SmokeCashLedgerMatches: 1
```

This proves the disposable local smoke database can be backed up and restored. It does not prove production backup
retention, encryption, off-host storage, restore ownership, RPO or RTO.

## Production Requirements

Production backup design must define:

- backup frequency;
- retention period;
- encryption at rest;
- off-host/off-region storage;
- restore owner;
- restore target environment;
- RPO and RTO;
- alerting when backups fail or become stale.

At least one restore drill should be performed before any production-ready claim.
