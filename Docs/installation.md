# OpenCashFlow Installation

OpenCashFlow is self-hosted by default. A fresh instance starts with no company and no users; the web app redirects to
`/Setup` and creates the first company plus the first `InstanceAdmin`.

## Docker Compose

1. Copy the environment template:

```bash
cp .env.example .env
```

2. Edit `.env` and set at least `JWT_SECRET` to a random value of 64+ characters.

3. Start the stack:

```bash
docker compose up --build
```

4. Open `http://localhost:5200` and complete the setup wizard.

The default Docker path sets `AUTO_MIGRATE=true` explicitly so a non-technical install can create/update the database on
startup. SMTP variables can stay empty for first setup.

## Manual Install

Requirements:

- .NET 9 SDK
- PostgreSQL 16+

Create a database and export configuration:

```bash
export DEFAULT_CONN_STRING='Host=localhost;Database=opencashflow;Username=postgres;Password=postgres'
export JwtSettings__SecretKey='change-me-use-a-random-secret-with-at-least-64-characters'
export AUTO_MIGRATE=true
```

Then run:

```bash
dotnet restore OpenCashFlow.sln
dotnet build OpenCashFlow.sln
dotnet run --project src/OpenCashFlow.API/OpenCashFlow.API.csproj
dotnet run --project src/OpenCashFlow.App/OpenCashFlow.App.csproj
```

Open the App URL and complete `/Setup`.

## Upgrades And Migrations

For small local installs, keep `AUTO_MIGRATE=true` and restart the API after pulling a new version.

For production, prefer manual migrations:

```bash
export DEFAULT_CONN_STRING='Host=...;Database=...;Username=...;Password=...'
dotnet ef database update --project src/OpenCashFlow.Shared --startup-project src/OpenCashFlow.API
```

Then run the API with `AUTO_MIGRATE=false`.

## Backup And Restore

Backup:

```bash
pg_dump "$DEFAULT_CONN_STRING" --format=custom --file opencashflow.backup
```

Restore:

```bash
createdb opencashflow
pg_restore --dbname "$DEFAULT_CONN_STRING" --clean --if-exists opencashflow.backup
```

For Docker, run `pg_dump`/`pg_restore` against the `db` service or expose the mapped PostgreSQL port.

## Admin Password Reset

If SMTP is configured, use the normal forgot-password flow.

If SMTP is not configured and the only admin password is lost, restore from backup or have a technician reset the password
through a controlled maintenance script using the application password hasher. Do not add default admin credentials to a
production database.

## Reverse Proxy

Expose the App and API behind HTTPS and forward:

- `Host`
- `X-Forwarded-For`
- `X-Forwarded-Proto`

Health check endpoint:

```text
GET /health
```

The endpoint returns `200` when the API can connect to PostgreSQL and `503` when the database is unavailable.
