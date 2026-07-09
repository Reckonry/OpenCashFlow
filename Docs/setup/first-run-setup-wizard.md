# First-Run Setup Wizard

OpenCashFlow includes a first-run setup wizard for fresh self-hosted installations.

The wizard configures application data only. It does not create PostgreSQL users, create databases, change database permissions, or perform infrastructure provisioning. Database connectivity must already be configured through Docker Compose, environment variables, or the host deployment configuration.

## Local Docker Flow

```bash
cp .env.example .env
docker compose up --build
```

Then open:

```text
http://localhost:5200
```

If no company and no administrator exist, the WebApp redirects to:

```text
/Setup
```

## Setup Inputs

The first-run setup form asks for:

- company name;
- owner/admin first name;
- optional owner/admin last name;
- admin email;
- language;
- currency;
- country;
- timezone.

The setup wizard does not ask for an administrator password. OpenCashFlow generates a strong temporary password server-side.

## What Setup Creates

On a fresh instance, setup creates:

- the first company/tenant;
- the first administrator user;
- standard self-hosted roles;
- the administrator role assignments;
- the company staff link for the administrator;
- the administrator contact email;
- an initial zero cash balance for the company.

Cash Custody domain contracts exist, but there is no persisted multi-cash-account schema yet. Until that implementation lands, setup seeds the current legacy company-level `CashBalance` record rather than a new `CashAccount`.

## Temporary Password

After successful setup, the WebApp shows the generated temporary administrator password exactly once in the setup POST response.

Store it immediately. Refreshing or revisiting setup after completion does not show the password again.

The password is:

- generated server-side with a cryptographic random generator;
- hashed before storage;
- never stored as plaintext;
- never logged intentionally by setup code;
- marked as temporary by setting the first admin to require password change after login.

## Setup Lock

Setup is only available while the instance has no company and no administrator user.

After setup completes:

- `GET /v1/Setup/status` reports `RequiresSetup = false`;
- `POST /v1/Setup` returns a conflict instead of creating another instance;
- the WebApp `/Setup` page redirects to login.

If the database is partially configured, for example a company exists but no admin user exists, setup refuses to continue. That state requires an explicit recovery procedure rather than silent repair.

## First Login

Use the administrator email and the temporary password shown after setup completes.

After login, OpenCashFlow redirects the user to the password-change screen because the first administrator is created with `UserMustChangePassword = true`.

## Security Notes

- Do not expose an unconfigured instance publicly.
- Set real secrets and database credentials before any public or shared deployment.
- Treat `.env.example` as a template only.
- Do not paste generated setup passwords into issue reports, logs, screenshots, or support channels.
- If the temporary password is lost before first login, use a controlled password reset or database recovery procedure.
