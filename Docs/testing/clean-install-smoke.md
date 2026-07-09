# Clean Install Smoke Test

OpenCashFlow has a scripted clean-install smoke path:

```text
scripts/smoke/clean-install-smoke.sh
```

The script starts a disposable Docker Compose stack, applies the normal application startup path, completes initial
setup, logs in, creates a payment, and verifies the cash effect.

## Scope

The smoke currently verifies:

- clean PostgreSQL database starts from an empty volume;
- API `/health` becomes healthy;
- WebApp `/Login` renders;
- `/v1/Setup/status` reports setup is required;
- `/v1/Setup` creates the first company/admin;
- `/v1/Authentication/login` returns a JWT;
- JWT contains `TenantID` and `UserID`;
- payment method and document type lookups are readable with the JWT;
- `/v1/Payment` creates a payment;
- `/v1/Payments` returns the created payment;
- `/v1/admin/cash/current` reports a positive balance after the cash payment;
- `/v1/admin/cash/ledger` contains an entry for the created payment.

This is a smoke test, not a full browser E2E suite. It intentionally tests the public HTTP path with deterministic data
and a disposable database.

## Prerequisites

- Docker and Docker Compose;
- `curl`;
- `python3` for JSON/JWT parsing in the script;
- local ports available:
  - API: `15100`;
  - WebApp: `15200`;
  - PostgreSQL: `15432`.

Override ports if needed:

```bash
SMOKE_API_PORT=16100 SMOKE_WEB_PORT=16200 SMOKE_DB_PORT=16432 scripts/smoke/clean-install-smoke.sh
```

## Running

```bash
scripts/smoke/clean-install-smoke.sh
```

The script uses this Compose project name by default:

```text
opencashflow-smoke
```

It uses a dedicated Compose file:

```text
scripts/smoke/docker-compose.clean-install.yml
```

The smoke Compose file has isolated container names and host ports so it does not collide with the default OpenCashFlow
local stack.

## Cleanup

The script runs:

```bash
docker compose -p opencashflow-smoke -f scripts/smoke/docker-compose.clean-install.yml down -v --remove-orphans
```

on exit by default. This removes the disposable smoke database volume.

To keep the stack after a failure for inspection:

```bash
SMOKE_KEEP_STACK=1 scripts/smoke/clean-install-smoke.sh
```

Then inspect logs with:

```bash
docker compose -p opencashflow-smoke -f scripts/smoke/docker-compose.clean-install.yml logs
```

## Known Limits

- It does not drive the Razor UI with a browser.
- It does not verify every dashboard widget.
- It does not verify password reset, fast-login PIN, exports, or admin settings.
- It assumes setup admin login remains available immediately after setup.

Recommended future work:

- add a browser-level smoke for Login, Setup, Payments, and Cash pages;
- run this smoke in CI as an optional or scheduled job with Docker available;
- add a backup/restore drill that runs after the smoke creates data.
