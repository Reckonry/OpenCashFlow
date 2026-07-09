# First-Run Setup Smoke

Date: 2026-07-09

Branch: `feature/first-run-setup-wizard`

Release packaging note: `0.1.0-preview.1` includes the Docker runtime fix for the `libgssapi_krb5.so.2` warning observed during this smoke.

## Scope

This smoke validated the first-run setup wizard against a clean Docker Compose stack.

The smoke used the local Docker Compose services:

- WebApp: `http://localhost:5200`
- API: `http://localhost:5100`
- PostgreSQL: Docker Compose `db` service with a fresh `opencashflow_pgdata` volume

No deployment was performed.

## Commands Used

Clean stack:

```bash
docker compose down -v
docker compose up -d --build
```

Fast repeat of the WebApp form path after images were built:

```bash
docker compose down -v
docker compose up -d
```

Health and setup status:

```bash
curl -i http://localhost:5100/health
curl -i http://localhost:5200/
curl -i http://localhost:5200/Login
curl -i http://localhost:5100/v1/Setup/status
```

WebApp setup form:

```bash
curl -sS -c /private/tmp/ocf-web.cookies \
  -o /private/tmp/ocf-web-setup.html \
  http://localhost:5200/Setup
```

The antiforgery token was read from the setup form and posted back to `POST /Setup` with:

- company name: `Web Smoke Workshop SRL`
- admin email: `web-owner-smoke@example.local`
- admin first name: `Web`
- admin last name: `Owner`
- language: `it`
- currency: `EUR`
- country: `IT`
- timezone: `Europe/Rome`

The temporary password was captured from the setup completion response and was not written to this document.

API login and password change:

```bash
curl -sS -X POST http://localhost:5100/v1/Authentication/login \
  -H "Content-Type: application/json" \
  -d '{"username":"web-owner-smoke@example.local","password":"<temporary-password>"}'

curl -sS -i -X POST http://localhost:5100/v1/Authentication/change-password-required \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <first-login-token>" \
  -d '{"newPassword":"<new-password>"}'

curl -sS -X POST http://localhost:5100/v1/Authentication/login \
  -H "Content-Type: application/json" \
  -d '{"username":"web-owner-smoke@example.local","password":"<new-password>"}'
```

Log check:

```bash
docker logs opencashflow-api
docker logs opencashflow-webapp
```

The generated temporary password was searched explicitly in API and WebApp logs and was not found.

## Results

| Check | Result |
| --- | --- |
| Clean DB/container state | Passed. `docker compose down -v` removed containers and `opencashflow_pgdata`. |
| Docker Compose startup | Passed. `api`, `webapp`, and `db` started. |
| API health | Passed. `GET /health` returned `{"status":"healthy","database":"ok"}`. |
| WebApp root | Passed. `GET /` returned `302` to `http://localhost:5200/Login`. |
| Unconfigured login redirect | Passed. `GET /Login` returned `302 Location: /Setup`. |
| Setup status before setup | Passed. API returned `requiresSetup=true`, `hasCompanies=false`, `hasAdminUsers=false`. |
| WebApp setup form | Passed. `GET /Setup` returned the first setup page with antiforgery token and no password input fields. |
| Complete setup through WebApp form | Passed. `POST /Setup` returned `200 OK` with the `Setup complete` page. |
| Temporary password shown once | Passed. Password appeared in the setup completion response. After setup, `GET /Setup` returned `302 Location: /Account/Login` and did not replay the password. |
| Login with temporary password | Passed. Login returned `success=true` and `requiresPasswordChange=true`. |
| Change password | Passed. `POST /v1/Authentication/change-password-required` returned `200 OK`. |
| Login with new password | Passed. Login returned `success=true` and `requiresPasswordChange=false`. |
| Setup locked after configuration | Passed. `GET /Setup` redirected to login after configuration. |
| Second setup POST | Passed. `POST /v1/Setup` returned `409 Conflict`. |
| Plaintext generated password in logs | Passed. Exact generated temporary password was not present in API or WebApp logs. |

## Preview Release Validation

The clean-install smoke script was rerun on `release/0.1.0-preview` after aligning it with the generated temporary password contract.

Result:

- `scripts/smoke/clean-install-smoke.sh` passed.
- The script completed setup, read the generated temporary password from the setup response, logged in, changed the required password, logged in again, created a smoke payment, and verified the cash ledger effect.
- The API Docker image in this branch includes `libgssapi-krb5-2`.

## Issues Found

The API container logs this startup message:

```text
Cannot load library libgssapi_krb5.so.2
Error: libgssapi_krb5.so.2: cannot open shared object file: No such file or directory
```

The application still started and the health endpoint reported the database as healthy. This should be tracked separately because it creates noisy operational logs and may indicate a missing native package in the runtime image.

This warning is addressed in the `0.1.0-preview.1` release branch by installing the `libgssapi-krb5-2` runtime package in the API image.

During one repeated invalid-login check immediately after several auth attempts, the auth rate limiter returned `503 Service Unavailable`. That is consistent with rate limiting behavior during smoke repetition, not a setup failure.

## Screenshots

No screenshots were captured. The smoke used HTTP checks and saved local response HTML under `/private/tmp` during the run.

## Remaining Gaps

- The smoke did not exercise a full browser UI interaction beyond HTTP form submission.
- The setup wizard currently creates the legacy company-level `CashBalance`, not a persisted Cash Custody `CashAccount`; the Cash Custody persistence model is not implemented yet.
- The `libgssapi_krb5.so.2` startup log was addressed after this smoke by adding the missing runtime package to the API Docker image.
