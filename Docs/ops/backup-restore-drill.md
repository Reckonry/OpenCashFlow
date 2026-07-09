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

Status: **not performed as part of the original ops documentation branch**.

Reason:

- the clean-install smoke stack is now present in `development`, but this backup/restore drill has not yet been
  executed against it;
- the root `docker-compose.yml` uses fixed container names and standard host ports, so destructive testing against the
  default stack remains unsafe on a workstation that may already contain local OpenCashFlow data;
- the documented procedure intentionally avoids `docker compose down -v` against the standard local volume.

Future exact local proof:

1. Start the isolated smoke stack with `SMOKE_KEEP_STACK=1 scripts/smoke/clean-install-smoke.sh`.
2. Confirm representative data exists: company/admin, login, payment and cash ledger.
3. Run `pg_dump --format=custom`.
4. Restore into `opencashflow_restore`.
5. Verify counts for `Companies`, `Payments` and `CashLedgers`.
6. Destroy only the disposable stack/volume.

Until this proof is run, backup/restore remains a production-readiness blocker.

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
