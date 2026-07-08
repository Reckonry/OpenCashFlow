# Secrets Management

OpenCashFlow local evaluation uses environment variables and `.env.example` templates. Production deployments must use a
real secret-management process.

Do not commit real `.env` files, passwords, tokens, private URLs, SMTP credentials, or database connection strings.

## Local Evaluation Defaults

The root Docker Compose file includes local defaults:

- PostgreSQL `postgres/postgres`;
- fallback JWT secret;
- empty SMTP values;
- local HTTP origins.

These values are acceptable only for disposable local evaluation.

## Required Production Secrets

At minimum, production operators must manage:

- `JwtSettings__SecretKey`;
- PostgreSQL username/password or managed database credentials;
- SMTP username/password, if email is enabled;
- reverse proxy TLS private keys or certificate automation credentials;
- any external observability sink credentials;
- backup storage credentials.

## Storage Requirements

Use one of:

- cloud secret manager;
- orchestrator secrets;
- encrypted deployment variables;
- managed platform secrets.

Avoid:

- checked-in `.env` files;
- secrets embedded in Docker images;
- secrets in command history;
- secrets in issue reports or logs;
- shared static secrets across environments.

## JWT Secret Rotation

Normal rotation:

1. Generate a new random value with at least 256 bits of entropy.
2. Store it in the secret manager.
3. Restart API and WebApp with the new value.
4. Confirm new logins work.
5. Remove the old value.

Impact:

- existing JWTs become invalid;
- users must log in again;
- fast-login flows should be rechecked.

Emergency rotation after leak:

1. Rotate immediately.
2. Restart all app nodes.
3. Review logs for suspicious access.
4. Notify affected operators/users according to the incident process.

## Database Credential Rotation

Recommended approach:

1. Create a new database role or password.
2. Grant required permissions.
3. Update secret manager.
4. Restart application services.
5. Verify `/health`.
6. Revoke the old credential.
7. Confirm old credential no longer connects.

Do not rotate by editing committed files.

## SMTP Credential Rotation

SMTP is optional. If enabled:

1. Rotate credentials at the provider.
2. Update runtime secrets.
3. Restart services.
4. Run password-reset and notification checks.
5. Remove old credentials from the provider.

## Incident Handling

If a secret is committed:

1. Treat it as compromised.
2. Rotate it immediately.
3. Remove it from active configuration.
4. Remove or rewrite repository history only after coordinating with maintainers.
5. Document the incident privately if exploitable details are involved.

Never post real tokens, reset links, PINs, JWTs, or database credentials in public issues.
