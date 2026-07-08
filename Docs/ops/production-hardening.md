# Production Hardening

OpenCashFlow is a **Developer Preview / Early Self-Hosted Preview**. This document describes the minimum hardening work
expected before a real production deployment. It is not a production-readiness claim.

## Local Defaults Versus Production

The root `docker-compose.yml` is for local evaluation.

Local defaults include:

- PostgreSQL user/password `postgres/postgres`;
- a development JWT fallback secret;
- plain HTTP ports;
- `AUTO_MIGRATE=true`;
- empty SMTP settings;
- no external backup target;
- no reverse proxy configuration.

Production deployments must replace all of those defaults.

## Minimum Production Requirements

Before exposing an instance:

- terminate TLS at a trusted reverse proxy or load balancer;
- use a strong `JWT_SECRET` from a secret manager;
- use externally managed PostgreSQL or a hardened PostgreSQL service;
- disable public database port exposure;
- configure automated backups and restore testing;
- configure SMTP or intentionally disable flows that depend on email delivery;
- set explicit trusted origins and cookie domains;
- retain structured application logs;
- monitor API health, WebApp availability, PostgreSQL health, disk usage, and backup freshness;
- document an upgrade and rollback process.

## TLS And Reverse Proxy

OpenCashFlow containers expose HTTP internally. Production TLS should be terminated by a reverse proxy such as nginx,
Caddy, Traefik, a cloud load balancer, or an equivalent ingress controller.

Production requirements:

- force HTTPS;
- enable HSTS at the edge;
- forward `X-Forwarded-Proto` and `X-Forwarded-For`;
- restrict allowed hosts;
- keep API and WebApp origins explicit;
- avoid exposing API, WebApp, and PostgreSQL directly to the public internet unless intentionally designed.

Recommended production shape:

```text
Internet -> TLS reverse proxy -> WebApp/API containers -> private PostgreSQL
```

## JWT Secret Rotation

`JwtSettings__SecretKey` signs authentication tokens. Treat it as a high-value secret.

Rotation procedure:

1. Generate a new high-entropy secret.
2. Store it in the deployment secret manager.
3. Restart API and WebApp with the same new value.
4. Expect existing JWT sessions to fail validation.
5. Force users to log in again.
6. Revoke or delete the old secret from the secret manager.

Emergency rotation after suspected compromise should happen immediately.

## Database Credentials

Production must not use `postgres/postgres`.

Requirements:

- use a least-privilege application database role;
- store credentials in a secret manager or runtime secret injection mechanism;
- rotate credentials on a documented schedule;
- avoid committing `.env` files with real values;
- do not expose PostgreSQL on a public interface;
- require TLS to PostgreSQL when the database is outside the private host/network.

## External PostgreSQL

For production, prefer external PostgreSQL managed by the operator or platform.

Recommended settings:

- point `DEFAULT_CONN_STRING` at the production PostgreSQL endpoint;
- set `AUTO_MIGRATE=false` unless using a deliberately controlled startup migration process;
- run migrations as a separate release step;
- require regular `pg_dump` or storage-level backups;
- monitor replication, disk, connections, and slow queries.

## AUTO_MIGRATE Guidance

`AUTO_MIGRATE=true` is useful for local evaluation and disposable smoke tests.

Production recommendation:

- set `AUTO_MIGRATE=false`;
- run migrations in a controlled maintenance step;
- back up before applying migrations;
- run migrations against a staging copy first;
- record migration IDs before and after release;
- keep rollback expectations explicit.

Use automatic startup migrations only when the operator has accepted the risk that application startup may modify the
database schema.

## Logging And Observability Minimums

Minimum signals:

- API `/health`;
- WebApp login page availability;
- PostgreSQL connection health;
- authentication failures;
- password reset events;
- setup completion;
- payment create/update/delete events;
- cash ledger adjustments;
- background migration attempts;
- unhandled exceptions.

Minimum operational retention:

- application logs retained long enough for incident triage;
- reverse proxy access logs with sensitive values removed;
- PostgreSQL logs for connection and error events;
- backup job logs;
- restore drill logs.

Do not log passwords, JWTs, reset tokens, PINs, SMTP credentials, or raw secret values.

## Production Blockers Remaining

The project should not be described as production-ready until at least these items are proven:

- backup and restore drill repeated on the intended production topology;
- upgrade/migration drill repeated against a production-like database copy;
- GitHub dependency alerts fully closed or formally dismissed with evidence;
- browser-level clean-install smoke coverage;
- documented reverse proxy configuration tested end to end;
- security review of auth, fast-login/PIN, reset password, authorization, and tenant isolation.
