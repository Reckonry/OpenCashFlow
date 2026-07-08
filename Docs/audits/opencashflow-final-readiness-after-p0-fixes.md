# OpenCashFlow Final Readiness Audit After P0 Fixes

Date: 2026-07-08  
Branch: `audit/final-readiness-after-p0-fixes`  
Base HEAD verified: `ffb2878`

## Scope

This audit validates the repository after residue cleanup, OSS readiness work, CI quality gates, dependency alert reconciliation, UI maintainability cleanup, and P0 skipped-test fixes.

This is a readiness audit, not a production approval.

## Commands Run

```bash
git status --short
dotnet sln OpenCashFlow.sln list
dotnet build OpenCashFlow.sln --configuration Release --no-restore
dotnet test OpenCashFlow.sln --configuration Release --no-build
docker compose config
dotnet list OpenCashFlow.sln package --vulnerable --include-transitive
```

Results:

- `git status --short`: clean before creating this audit report.
- `dotnet sln OpenCashFlow.sln list`: 9 active projects; no `OpenCashFlow.Admin`, no `OpenCashFlow.Shared`.
- `dotnet build OpenCashFlow.sln --configuration Release --no-restore`: passed, 0 warnings, 0 errors.
- `dotnet test OpenCashFlow.sln --configuration Release --no-build`: passed, 237 passed, 18 skipped, 0 failed.
- `docker compose config`: passed.
- `dotnet list OpenCashFlow.sln package --vulnerable --include-transitive`: no vulnerable NuGet packages found in any solution project.

## What Improved Since The Previous Final Audit

- Legacy Admin source is gone from active `src`; `OpenCashFlow.sln` no longer references it.
- `OpenCashFlow.Shared` is gone from the solution and core references.
- Tracked residue such as `.agent`, `myapp.context`, and environment-specific `env/.env.api.*` files is gone.
- OSS packaging now exists: `README.md`, `CONTRIBUTING.md`, `CHANGELOG.md`, `CODE_OF_CONDUCT.md`, issue templates, PR template, roadmap, security notes, and third-party notices.
- CI has conservative quality gates and no legacy promotion workflows.
- ZAP workflows run only on non-draft PRs to `main`.
- Dependency reconciliation removed inactive vendored frontend `package.json` manifests that likely caused npm Dependabot alerts.
- `Payments/Index.cshtml` was reduced from 1208 to 1045 lines with extracted partials and CSS.
- P0 skipped tests for authorization, tenant isolation, payment lookup isolation, and locked payment fields were re-enabled and pass.

## P0 Skipped Tests

The P0 skipped tests are fixed.

Confirmed in `Docs/testing/skipped-tests-backlog.md` and by the current test run:

- Company unauthorized create/update/delete/read cases now fail with `403`.
- Company cross-tenant detail/update cases now fail with `403`.
- Payment invalid-role access now fails with `403`.
- Payment create with tenant mismatch now fails with `400`.
- Payment update with cross-tenant payment method/document type now fails with `400`.
- Payment update attempting to mutate `UserID` now fails with `400`.

Current skipped count: 18.

Remaining skipped tests are P1/P2 and concentrated in:

- Company create/update/delete CRUD implementation.
- Company duplicate validation.
- Company list filtering.
- Company expired-state semantics.
- Company `MaxUsers`/employee limit decision.
- Non-existent company detail route behavior.

These are no longer the same class of P0 authorization/tenant-isolation blockers, but they still prevent a stable-release claim.

## Vulnerability Scan

Local command:

```bash
dotnet list OpenCashFlow.sln package --vulnerable --include-transitive
```

Result: no vulnerable packages reported for:

- `OpenCashFlow.API`
- `OpenCashFlow.Test`
- `OpenCashFlow.WebApp`
- `OpenCashFlow.Application`
- `OpenCashFlow.Infrastructure`
- `OpenCashFlow.Domain`
- `OpenCashFlow.Domain.Tests`
- `OpenCashFlow.Application.Tests`
- `OpenCashFlow.Contracts`

NuGet sources used:

- `https://api.nuget.org/v3/index.json`
- `https://nuget.pkg.github.com/GoMyRO/index.json`

## GitHub Dependency Alerts

GitHub had been reporting 20 dependency alerts on the default branch.

Based on observable local evidence, those alerts are likely stale or default-branch related:

- The active solution vulnerability scan is clean.
- Inactive vendored frontend `package.json` manifests were removed from `wwwroot/libs`.
- `Docs/security/dependency-alert-reconciliation.md` documents that those npm manifests were likely the alert source.

This cannot be fully closed from local evidence alone. The maintainer must verify GitHub Security after the relevant branch is merged into the repository default branch and GitHub rescans. Any remaining alert should be classified as NuGet, npm, Docker image, or GitHub Actions.

## Repository And Project Structure

Active solution projects:

- `src/OpenCashFlow.API`
- `src/OpenCashFlow.Application`
- `src/OpenCashFlow.Contracts`
- `src/OpenCashFlow.Domain`
- `src/OpenCashFlow.Infrastructure`
- `src/OpenCashFlow.WebApp`
- `tests/OpenCashFlow.Application.Tests`
- `tests/OpenCashFlow.Domain.Tests`
- `tests/OpenCashFlow.Test`

Observed dependency shape:

- API -> Application, Contracts, Infrastructure.
- Application -> Domain.
- Infrastructure -> Application, Contracts, Domain.
- WebApp -> Contracts.
- Domain -> no project references.
- Contracts -> no project references.

Boundary checks:

- No `OpenCashFlow.Shared` references in active code.
- No EF/ASP.NET/API/Infrastructure dependency in Domain/Application scan.
- WebApp does not reference Infrastructure.
- API controllers/services/interfaces do not import Infrastructure EF entities.

Remaining architecture debt:

- Infrastructure still references Contracts. This is documented and should be reduced when persistence mappings are fully separated from public contracts.
- API still owns some DTO/result mapping glue. This is acceptable for a boundary layer but should be monitored.
- Legacy Billing/Stripe EF schema artifacts remain in Infrastructure for compatibility and are documented as non-runtime legacy.

## Documentation Readiness

Strong improvements:

- README clearly says **Developer Preview / Early Self-Hosted Preview**.
- README explicitly says not production-ready.
- Roadmap is conservative.
- Contributing guide aligns with architecture and security goals.
- Security docs cover dependency policy, CSP, auth, legacy Billing/Stripe status, and .NET 10 baseline.
- Third-party notices document vendored asset obligations and dependency baseline.

Remaining documentation gaps:

- Production hardening is still more guidance than proven operational procedure.
- Backup/restore and upgrade policy need real dry-run evidence before production claims.
- The old Markdown issue template `.github/ISSUE_TEMPLATE/bug_report.md` still coexists with the newer YAML bug form. This is not a blocker, but it is duplicate OSS surface.

## CI And Security Workflow Readiness

Current workflows:

- `quality-gates.yml`
- `zap-baseline.yml`
- `zap-full.yml`

Positive findings:

- Quality gates run restore, dependency audit, Release build, Release tests, and Docker Compose config.
- Workflow permissions are read-only for quality gates.
- Legacy staging/production promotion workflows are removed.
- ZAP workflows are limited to non-draft PRs targeting `main`.

Remaining gaps:

- `dotnet format --verify-no-changes` is deferred.
- Warning-as-error policy is deferred.
- SBOM generation is deferred.
- ZAP status was not rerun in this local audit.

## Repository Hygiene

Fixed:

- `src/OpenCashFlow.Admin` is absent.
- `OpenCashFlow.Shared` is absent from the solution.
- No tracked `.agent`, `myapp.context`, `.DS_Store`, or `env/.env.api.*` files were found.
- `.gitignore` ignores `.DS_Store`, `.agent/`, and `myapp.context`.
- The `env` directory is absent; sanitized root `.env.example` exists.

Observed local-only residue:

- `Docs/.DS_Store` exists on disk but is ignored and not tracked.

This is not a repository blocker, but the local working copy should be cleaned before packaging an archive.

## Current Production Readiness

Not production-ready.

Reasons:

- 18 skipped tests remain in company CRUD/filtering/state/user-limit areas.
- Company write endpoints still have incomplete behavior.
- Backup/restore and upgrade/migration procedures are documented at a high level but not proven by a repeatable release drill in this audit.
- Production deployment hardening has not been validated with TLS, rotated secrets, external PostgreSQL, backups, restore, and observability.
- ZAP workflows exist, but this audit did not execute ZAP locally or inspect a successful GitHub run.
- GitHub dependency alerts still need default-branch Security tab verification after merge/rescan.

## Current Developer Preview Readiness

Suitable as a public Developer Preview / Early Self-Hosted Preview, with honest positioning.

Reasons:

- Build/test/compose/advisory checks pass locally.
- OSS docs and templates exist.
- README does not oversell maturity.
- P0 auth/tenant/payment skipped tests were fixed.
- Legacy SaaS/Admin runtime is removed from active source.
- Dependency alert reconciliation is documented.

The project should still be framed as evaluation-ready, contributor-ready, and architecture-review-ready, not production-ready.

## Updated Scores

| Area | Score | Evidence |
| --- | ---: | --- |
| Repository first impression | 8 | OSS docs exist, README is honest, residue mostly removed. Duplicate bug template and local ignored `.DS_Store` remain. |
| Repository organization | 8 | Clean project layout; Admin/Shared removed; active modules are understandable. |
| Documentation quality | 8 | Strong status, roadmap, security and contribution docs; production procedures still need proven runbooks. |
| Architecture boundaries | 8 | Domain/Application/WebApp boundaries are clean; Infrastructure -> Contracts remains a debt. |
| Code quality | 7 | Build has 0 warnings; large UI and some API mapping/service debt remain. |
| Project structure | 8 | Solution is coherent and self-hosted; legacy schema artifacts remain isolated but present. |
| Test reliability | 7 | 237 pass, 18 skipped, 0 failed; P0 skips fixed; P1/P2 company skips remain. |
| Security posture | 7 | NuGet scan clean, CSP/ZAP gates exist, P0 auth/tenant tests fixed; production hardening and GitHub alert closure still pending. |
| OSS readiness | 8 | README, contributing, changelog, code of conduct, templates, roadmap present; duplicate issue template should be removed. |
| Developer experience | 7 | Quickstart and CI gates exist; still needs smoother smoke-test/runbook evidence. |
| Production readiness | 4 | Not production-ready; company CRUD gaps, skipped tests, operational proof and GitHub alert verification remain. |
| Professional appearance | 8 | Much improved; remaining rough edges are now specific and tractable. |

## Remaining Blockers Before Stable Release

1. Resolve or intentionally remove the 18 skipped Company tests.
2. Implement or explicitly remove incomplete Company create/update/delete route contracts.
3. Decide `MaxUsers` behavior in self-hosted mode.
4. Prove backup/restore with a documented dry run.
5. Prove upgrade/migration procedure with a documented dry run.
6. Verify GitHub dependency alerts close after default-branch merge/rescan, or document/dismiss any stale alerts.
7. Run and record successful ZAP Baseline and Full Scan on GitHub after merge.
8. Remove duplicate `.github/ISSUE_TEMPLATE/bug_report.md` or decide intentionally to keep both issue-template formats.
9. Continue extracting inline frontend scripts/styles and removing CDN residue.
10. Reduce Infrastructure dependency on Contracts where practical.

## Recommended Next PRs

1. `test/company-crud-contract-coverage`: implement Company create/update/delete or remove/adjust the route contract, then re-enable related tests.
2. `test/company-filtering-coverage`: implement supported filters or remove unsupported query claims.
3. `ops/backup-restore-drill`: add a real backup/restore runbook and test it against Docker Compose PostgreSQL.
4. `security/github-alert-closeout`: verify GitHub Security alerts after default-branch merge and document remaining ecosystems.
5. `ci/zap-run-evidence`: capture successful ZAP Baseline/Full workflow run links in release readiness docs.
6. `docs/issue-template-cleanup`: remove the duplicate legacy Markdown bug template.
7. `frontend/static-assets-csp-cleanup`: continue moving inline JS/CSS to static files.

## Final Verdict

- Public Developer Preview: yes, with the current README wording.
- Production-ready: no.
- Stable-release-ready: no.
- Suitable for contributors: yes.
- Suitable for architecture/security review: yes.
- Suitable for business-critical financial use: no.

The repository now looks like a serious early open-source project. It does not yet look like a proven production product.
