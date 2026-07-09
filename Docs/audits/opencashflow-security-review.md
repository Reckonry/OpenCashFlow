# OpenCashFlow Application Security Review

Date: 2026-07-08

Perspective: Senior Application Security Engineer.

Scope: observable repository evidence only. This review intentionally ignores code style and focuses on authentication, authorization, tenant isolation, secrets, dependency hygiene, Docker, OWASP, headers, CSP, supply chain, GitHub workflows, credential management and attack surface.

## Executive Security Verdict

OpenCashFlow is not ready to be exposed to the public Internet or deployed for customers without a focused security hardening pass.

It is ready for a controlled external penetration test against a disposable pre-production environment. A pentest would be useful now because the application has enough real security surface to test: JWT auth, role policies, tenant scoping, password reset, PIN/fast-login, setup, payments, cash ledger, audit, CSP, Docker deployment and ZAP workflows.

It is not ready for a public bug bounty. A public bounty would create noise and reputational risk before production deployment assumptions, dependency alerts, reverse-proxy/TLS behavior, Docker hardening and operational security are proven.

## Evidence Reviewed

Reviewed areas:

- `src/OpenCashFlow.API/AppStart/05_Auth.cs`
- `src/OpenCashFlow.API/AppStart/04_RateLimiting.cs`
- `src/OpenCashFlow.API/AppStart/11_Cors.cs`
- `src/OpenCashFlow.API/AppStart/14_MiddlewarePipeline.cs`
- `src/OpenCashFlow.WebApp/Program.cs`
- auth use cases under `src/OpenCashFlow.Application/Auth`
- auth infrastructure under `src/OpenCashFlow.Infrastructure/Auth`
- API controllers for Company, Payment, Authentication and DevEmail
- Dockerfiles and `docker-compose.yml`
- GitHub workflows under `.github/workflows`
- dependency files and package references
- security/ops docs.

Observable baseline:

- JWT bearer auth validates issuer, audience, signing key, lifetime and expiration.
- API has role policies: `InstanceAdmin`, `CompanyAdmin`, `CompanyMember`.
- Auth endpoints use a specific `auth-limiter`.
- WebApp emits CSP, frame, content-type, referrer and permissions headers.
- ZAP baseline and full scan workflows exist for PRs to `main`.
- Quality gate includes `dotnet list OpenCashFlow.sln package --vulnerable --include-transitive`.
- No active `package.json`, `package-lock.json`, `yarn.lock`, or `pnpm-lock.yaml` files were found.
- Root Docker Compose is clearly local-evaluation oriented, with local defaults.

## Authentication

### Strengths

- JWT validation is configured for issuer, audience, signing key, lifetime and expiration.
- Token clock skew is narrowed to 1 minute.
- API rejects missing JWT secret and secrets shorter than 32 characters.
- Login use case returns generic invalid credential failure for missing or incorrect credentials.
- Password reset token flow uses token generation, token storage, validity checks and invalidation after reset.
- Auth endpoints are rate-limited.
- WebApp auth cookie is `HttpOnly` where token-bearing cookies are set.

### Concerns

- API sets `RequireHttpsMetadata = false` in JWT bearer configuration. That is acceptable in local evaluation, but unsafe as a production default if not overridden or justified.
- WebApp sets cookie `Secure = Request.IsHttps` in at least the normal login flow. No `UseForwardedHeaders` configuration was found. Behind a TLS-terminating reverse proxy, the app may see HTTP and issue non-Secure cookies unless proxy headers are configured correctly.
- Fast-login/PIN exists as an alternative auth flow. It has cookie signing and rate/lockout logic in the WebApp, but it remains a high-value feature that deserves dedicated adversarial testing before Internet exposure.
- Password reset returns success only when a user is found in the Application use case. Depending on controller behavior, this can create account enumeration risk. This needs explicit verification.
- There is no observed server-side JWT revocation/blacklist. Logout appears cookie/client driven.

### Security Rating

Authentication is credible for a preview, but not ready for hostile Internet traffic without proxy, cookie, reset-token and fast-login review.

## Authorization

### Strengths

- API controllers use `[Authorize]`, role attributes and policies.
- Company write/delete endpoints require `CompanyAdmin` or `InstanceAdmin`.
- Company tenant access checks compare route tenant ID to the `TenantID` claim, with `InstanceAdmin` bypass.
- Tests exist for unauthorized company/payment/lookup access, forbidden cross-company access and locked payment field behavior.

### Concerns

- `CompanyController.GetAllCompanies` is protected only by `CompanyMember`; comments indicate a TODO for `InstanceAdmin`. If this endpoint returns all companies, it is a potential horizontal data exposure unless the service layer scopes results for non-admin users. This should be treated as high priority.
- `PaymentController.GetPayments` honors caller-supplied `IncludeAllUsers` and tenant filters are set only if missing. This must be proven to prevent client-supplied tenant override. Tests exist around cross-company behavior, but this remains a sensitive access-control surface.
- Some tests accept either `Forbidden` or `NotFound`, which may be intentional anti-enumeration, but also makes exact access-control semantics less crisp.

### Security Rating

Authorization has meaningful coverage, but company listing and payment filtering should be reviewed manually and with negative tests before exposure.

## Tenant Isolation

### Strengths

- Tenant ID is embedded in JWT claims.
- Company APIs have explicit `CanAccessTenant` checks.
- Infrastructure queries frequently filter by `TenantID`.
- Payment and company tests cover wrong-company and cross-company scenarios.

### Concerns

- Tenant isolation appears implemented at service/query level rather than guaranteed by database row-level security. That can be acceptable, but it puts more burden on test coverage and code review.
- Historical DB tests remain excluded from compilation under `tests/OpenCashFlow.Test/Tests/db/**/*.cs`; they include persistence and cross-company concerns. Some of the valuable scenarios have been migrated, but this remains a residual assurance gap.
- `InstanceAdmin` bypass is broad. It needs explicit audit logging, operational controls and clear business rules.

### Security Rating

Tenant isolation is promising but should be a primary pentest target.

## Secrets And Credential Management

### Strengths

- `.env.example` exists.
- Only example/local env files were found in the shallow search.
- Secret management docs explicitly forbid real checked-in secrets and describe rotation.
- Root Compose uses env vars for JWT/SMTP overrides.

### Concerns

- Root Docker Compose defaults include:
  - `POSTGRES_PASSWORD=postgres`
  - API connection string with `Password=postgres`
  - fallback JWT secret.
- There is no secret-manager integration, Docker secrets, Kubernetes secrets or SOPS-based workflow.
- Rotation procedures are documented, not automated.
- No evidence of secret scanning workflow was found.

### Security Rating

Acceptable for local evaluation; not acceptable for production.

## Dependency Hygiene And Supply Chain

### Strengths

- Core packages target .NET 10.
- Microsoft packages are aligned on `10.0.9`.
- Quality gate runs NuGet vulnerability audit.
- Dependency alert reconciliation document exists.
- No npm manifests or lockfiles were found, reducing false npm advisory surface from vendored static assets.

### Concerns

- This review did not independently complete a local `dotnet list ... --vulnerable` scan. CI should be treated as authoritative for that gate.
- Docker base images use floating tags such as `mcr.microsoft.com/dotnet/aspnet:10.0`, `sdk:10.0` and `postgres:16-alpine`; images are not digest-pinned.
- GitHub Actions use floating major tags such as `actions/checkout@v4`, `actions/setup-dotnet@v4`, `actions/upload-artifact@v4`.
- No SBOM generation workflow was observed.
- No container image vulnerability scan workflow was observed.
- Vendored frontend libraries remain under `wwwroot/libs`; package manifests are gone, but static asset provenance still needs periodic review.

### Security Rating

Good NuGet direction; incomplete supply-chain hardening for production.

## Docker Security

### Strengths

- Multi-stage Dockerfiles reduce final image size.
- Compose has PostgreSQL healthcheck.
- Separate smoke Compose exists for disposable test runs.

### Concerns

- Dockerfiles do not set a non-root `USER`.
- Containers have no read-only filesystem, dropped capabilities, `no-new-privileges`, seccomp profile or resource limits.
- PostgreSQL is exposed on host port `5432`.
- Compose is production-looking enough to be dangerous but contains local-only defaults.
- No image signing or provenance evidence.

### Security Rating

Docker is suitable for local evaluation. It is not hardened for Internet-facing production.

## OWASP Review

### A01 Broken Access Control

Primary risk. Tenant isolation and role enforcement exist, but company listing, payment filters and `InstanceAdmin` bypass require adversarial testing.

### A02 Cryptographic Failures

JWT validation is present. Risks remain around fallback secrets, cookie Secure behavior behind proxies, no observed server-side token revocation and operational secret handling.

### A03 Injection

EF Core reduces SQL injection risk. Payment export uses CSV escaping for quotes. Dynamic filtering dependencies exist (`System.Linq.Dynamic.Core`), so filter/sort inputs should stay constrained.

### A04 Insecure Design

Fast-login/PIN and self-hosted setup are sensitive design areas. They may be valid, but require threat-model review and abuse testing.

### A05 Security Misconfiguration

High risk in deployment defaults: exposed DB port, default DB password, fallback JWT secret, `AUTO_MIGRATE=true`, direct HTTP ports and missing forwarded-header handling.

### A06 Vulnerable And Outdated Components

CI includes advisory scan, but Docker/GitHub Action/container image scanning are incomplete.

### A07 Identification And Authentication Failures

Rate limiting exists. Password reset and fast-login remain priority review areas. Account enumeration behavior must be verified.

### A08 Software And Data Integrity Failures

No digest pinning, SBOM or image signing. GitHub workflows are conservative but not fully supply-chain hardened.

### A09 Security Logging And Monitoring Failures

Audit logging exists for application actions, but operational security monitoring, alerting and incident response are not complete.

### A10 SSRF

No obvious SSRF-heavy surface was identified in this pass, but email/template/link generation and any future import/export features should be reviewed.

## Headers And CSP

### Strengths

- WebApp emits:
  - `Content-Security-Policy`
  - `X-Frame-Options: DENY`
  - `X-Content-Type-Options: nosniff`
  - `Referrer-Policy: strict-origin-when-cross-origin`
  - `Permissions-Policy`.
- CSP avoids `unsafe-inline` and `unsafe-eval`.
- CSP uses per-request nonce.
- ZAP baseline does not appear to allowlist CSP warnings.

### Concerns

- CSP still allows `https://cdn.jsdelivr.net` for scripts and styles.
- CSP nonce is still needed for inline Razor scripts/styles in some areas.
- API does not appear to emit the same full security header set.
- `connect-src` includes localhost and host.docker.internal values intended for local/container evaluation.
- No explicit `Strict-Transport-Security` header was observed in API; WebApp uses HSTS outside development.

### Security Rating

CSP is materially better than many preview apps, but still not final production posture.

## GitHub Workflows

### Strengths

- Quality gates run on non-draft PRs and manual dispatch.
- Permissions are minimal in quality gate (`contents: read`).
- Quality gate includes restore, vulnerability audit, Release build, Release tests and Compose validation.
- ZAP baseline and full scans run on non-draft PRs to `main`.
- Legacy promotion/deploy workflows are absent from current workflow list.

### Concerns

- ZAP full scan quality gate parses HTML by counting `Medium` and `High` strings, which is less reliable than structured JSON/XML parsing.
- Actions are not SHA-pinned.
- No explicit dependency-review action was observed.
- No CodeQL workflow was observed.
- No secret scanning workflow or documentation gate was observed, though GitHub may provide secret scanning outside repository workflows.
- No SBOM/container scan workflow was observed.

### Security Rating

Good PR baseline. Not yet enterprise-grade supply-chain CI.

## Attack Surface

High-value surfaces:

- Login and JWT issuing.
- Fast-login/PIN flow.
- Password reset token generation/validation.
- Registration and account confirmation.
- Setup/bootstrap first admin flow.
- Company CRUD and tenant boundary.
- Payment create/update/delete and export.
- Cash ledger.
- Audit log.
- Swagger/OpenAPI if exposed in production.
- Docker-exposed PostgreSQL port.
- Vendored frontend libraries.

Primary attack classes to test:

- tenant ID tampering;
- role escalation;
- payment method/document type cross-tenant references;
- JWT audience/issuer confusion;
- cookie security behind reverse proxy;
- reset-token enumeration/replay;
- PIN brute force and lockout bypass;
- CSRF-like risks around cookie-authenticated WebApp actions;
- CSV injection in exports;
- CSP bypass;
- exposed dev/setup endpoints.

## Decision Answers

### Would I approve this for an external penetration test?

Yes, for a controlled non-production environment.

The app has enough security control surface to make a pentest valuable. Scope should include auth, tenant isolation, payment/cash flows, setup, reset password, fast-login/PIN, Docker config and CSP. Test data must be synthetic.

### Would I approve a bug bounty?

Not a public bug bounty.

I would approve a private invite-only assessment after fixing obvious deployment defaults, reverse-proxy cookie behavior, company listing authorization ambiguity and ZAP full scan parsing. A public bounty before Internet readiness would produce avoidable noise and risk.

### Would I expose this to the Internet?

No, not as-is.

Before Internet exposure, I would require:

- production reverse proxy with forwarded headers correctly configured;
- no fallback JWT/database secrets;
- PostgreSQL not publicly exposed;
- `AUTO_MIGRATE=false`;
- production CSP without CDN dependency or with justified source policy;
- full dependency/container scan;
- reviewed reset/PIN flows;
- explicit production security headers for API and WebApp.

### Would I deploy it for customers?

No.

For customers, the app needs stronger supply-chain controls, production secret management, Docker hardening, security monitoring, incident response, penetration test results and explicit closure of high-risk auth/tenant questions.

## Risk Ratings

| Area | Rating | Notes |
| --- | --- | --- |
| Authentication | Medium | Good JWT validation; fast-login/PIN, reset-token and reverse-proxy cookie behavior need review. |
| Authorization | Medium-High | Role policies exist; company list and filter override surfaces need proof. |
| Tenant isolation | Medium-High | Tests exist, but isolation is mostly application-layer and should be pentested. |
| Secrets | High for production | Defaults are local-only; no secret-manager integration. |
| Dependency hygiene | Medium | NuGet gate exists; Docker/action/SBOM/container scan gaps remain. |
| Docker | High for production | Local Compose is not hardened and exposes DB. |
| Headers/CSP | Medium | Strong CSP direction, but CDN and nonce debt remain. |
| GitHub workflows | Medium | Good gates, but no CodeQL/SBOM/dependency-review and unpinned actions. |
| Overall AppSec posture | Medium-High | Good developer-preview foundation; not Internet/customer ready. |

## Required Fixes Before Internet Exposure

1. Add and verify forwarded-header handling for API/WebApp behind TLS reverse proxy.
2. Ensure auth cookies are always `Secure` in production, independent of direct `Request.IsHttps` when behind proxy.
3. Remove or strictly scope company-wide list access for non-InstanceAdmin users.
4. Add explicit negative tests for client-supplied tenant/filter override in payment list/report/export paths.
5. Review password reset response behavior for account enumeration.
6. Threat-model and pentest fast-login/PIN.
7. Remove production fallback secrets and provide a production deployment template that requires explicit secret injection.
8. Disable public PostgreSQL exposure in production examples.
9. Set `AUTO_MIGRATE=false` in production examples.
10. Add Docker hardening: non-root user, resource limits and reduced privileges where possible.
11. Add container image vulnerability scanning.
12. Add CodeQL or equivalent SAST.
13. Add SBOM generation.
14. Pin GitHub Actions and container images by digest or document an update policy.
15. Replace ZAP full-scan HTML grep gate with structured report parsing.
16. Close or formally reconcile GitHub dependency alerts.

## Final Security Conclusion

OpenCashFlow has a serious security foundation for a developer preview. It is not a casual toy project: JWT validation, role policies, tenant tests, rate limiting, CSP, ZAP and dependency gates are visible.

That said, I would not expose it to the Internet or deploy it for customers today. The remaining issues are not cosmetic; they touch authentication transport assumptions, access-control ambiguity, production secrets, Docker hardening, supply-chain controls and operational security monitoring.

Security approval status: controlled pentest yes; public bug bounty no; Internet exposure no; customer deployment no.
