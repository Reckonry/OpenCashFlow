# Docker Compose Release Install

OpenCashFlow preview releases are shipped as a Docker Compose stack.

The release package is intended for local evaluation and early self-hosted testing. It is not a production-ready deployment profile. Before exposing OpenCashFlow to other users or networks, review the hardening guidance in `Docs/ops/`.

For a picture-based walkthrough, see `Docs/setup/docker-compose-visual-guide.md`.

## Stack Layout

The release stack runs three services:

- `db`: PostgreSQL database.
- `api`: OpenCashFlow API service.
- `webapp`: OpenCashFlow WebApp service.

These are separate containers because they have different responsibilities:

- PostgreSQL owns persistent data.
- The API owns migrations, application logic, authentication, authorization, and data access.
- The WebApp owns browser-facing pages and talks to the API over the internal Compose network.

Keeping them separate makes the runtime easier to reason about, update, inspect, and eventually operate behind a reverse proxy.

## How Docker Compose Connects The Services

Docker Compose creates a private network for the stack.

Inside that network:

- the API connects to PostgreSQL using the hostname `db`;
- the WebApp connects to the API using the hostname `api`;
- users access the WebApp through `http://localhost:5200`;
- the API health endpoint is exposed on `http://localhost:5100/health`.

The public browser never connects directly to the `db` container.

## Download And Start

Download the preview package:

```bash
curl -LO https://github.com/Reckonry/OpenCashFlow/releases/download/v0.1.0-preview.2/OpenCashFlow-0.1.0-preview.2.tar.gz
# or:
# wget https://github.com/Reckonry/OpenCashFlow/releases/download/v0.1.0-preview.2/OpenCashFlow-0.1.0-preview.2.tar.gz
```

Extract it:

```bash
tar -xzf OpenCashFlow-0.1.0-preview.2.tar.gz
cd OpenCashFlow-0.1.0-preview.2
```

Create your local environment file:

```bash
cp .env.example .env
```

Edit `.env`, then start the stack:

```bash
docker compose -f docker-compose.release.yml up -d
```

Open:

```text
http://localhost:5200
```

On a fresh database, the WebApp redirects to `/Setup`.

## What `.env` Configures

The `.env` file is read by Docker Compose and passed to the containers as environment variables.

Important values:

- `APP_URL`: public WebApp URL used by redirects and links.
- `JWT_SECRET`: signing secret for authentication tokens.
- `AUTO_MIGRATE`: whether the API applies EF migrations on startup.
- `SMTP_HOST`, `SMTP_PORT`, `SMTP_USERNAME`, `SMTP_PASSWORD`, `SMTP_FROM`: optional email settings.
- `POSTGRES_DB`, `POSTGRES_USER`, `POSTGRES_PASSWORD`: database name and credentials, if you override the defaults.
- `API_PORT`, `WEBAPP_PORT`, `POSTGRES_PORT`: host ports, if you need to avoid local conflicts.
- `OPENCASHFLOW_VERSION`: image tag to run, for example `v0.1.0-preview.2`.

The release Compose file includes local defaults for evaluation, but you should set explicit values in `.env` for any shared environment.

## Values You Must Change Before Shared Use

Change these before exposing the stack beyond your own local machine:

- `JWT_SECRET`: use a long random value. Do not use the example value.
- `POSTGRES_PASSWORD`: use a real database password.
- `APP_URL`: set the actual HTTPS URL users will open.
- SMTP settings, if password reset or outbound email should work.

Also add TLS and a reverse proxy before public exposure. Do not expose the default local evaluation stack directly to the Internet.

## First-Run Setup

When the database is empty, OpenCashFlow is unconfigured.

After startup:

1. Open `http://localhost:5200`.
2. The WebApp redirects to `/Setup`.
3. Enter the company name, first administrator name, email, language, currency, country, and timezone.
4. OpenCashFlow creates the first company/tenant and administrator.
5. OpenCashFlow generates a temporary administrator password.
6. The password is shown exactly once.
7. Log in with the administrator email and generated password.
8. Change the password when prompted.

The setup wizard configures application data only. It does not create PostgreSQL users, create databases, change database permissions, or provision infrastructure.

## Stop The Stack

Stop containers but keep the database volume:

```bash
docker compose -f docker-compose.release.yml stop
```

Stop and remove containers while keeping the database volume:

```bash
docker compose -f docker-compose.release.yml down
```

Do not run `docker compose down -v` unless you intentionally want to remove the PostgreSQL volume and lose local data.

## Update The Stack

For a newer preview tag:

1. Read the release notes.
2. Back up the database.
3. Update `OPENCASHFLOW_VERSION` in `.env`, or use the new package with its default image tag.
4. Pull images and restart:

```bash
docker compose -f docker-compose.release.yml pull
docker compose -f docker-compose.release.yml up -d
```

If `AUTO_MIGRATE=true`, the API applies pending migrations on startup. For important data, test the update on a copied database before updating the primary instance.

## Backup

Create a PostgreSQL custom-format backup:

```bash
docker compose -f docker-compose.release.yml exec db \
  pg_dump -U "${POSTGRES_USER:-postgres}" \
  -d "${POSTGRES_DB:-opencashflow}" \
  -Fc \
  -f /tmp/opencashflow.backup

docker compose -f docker-compose.release.yml cp \
  db:/tmp/opencashflow.backup ./opencashflow.backup
```

Store the backup somewhere outside the Compose project directory.

## Restore

Restore into a clean PostgreSQL database only after reviewing the backup/restore drill:

```text
Docs/ops/backup-restore-drill.md
```

Do not overwrite a live database without a tested recovery plan.

## Useful Checks

Check container status:

```bash
docker compose -f docker-compose.release.yml ps
```

Check logs:

```bash
docker compose -f docker-compose.release.yml logs api
docker compose -f docker-compose.release.yml logs webapp
docker compose -f docker-compose.release.yml logs db
```

Check API health:

```bash
curl -fsS http://localhost:5100/health
```

## Current Limitations

- This is a Developer Preview / Early Self-Hosted Preview.
- The release Compose file is an evaluation path, not a complete production deployment.
- Cash Custody persistence is not complete yet.
- Backup/restore and upgrade procedures must be validated for your own environment.
- Secrets, TLS, monitoring, and operational runbooks are your responsibility before shared use.
