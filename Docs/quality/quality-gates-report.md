# Quality Gates Report

Date: 2026-07-07

## Summary

This change replaces legacy promotion/release workflows with conservative quality gates suitable for a public Developer Preview / Early Self-Hosted Preview.

The repository should validate pull requests without auto-merging branches, creating releases, deploying, or requiring production secrets.

## Workflows Removed

Removed stale promotion workflows:

- `.github/workflows/ci-wip-to-master.yml`
- `.github/workflows/ci-master-to-staging.yml`
- `.github/workflows/ci-staging-to-production.yml`

Reasons:

- referenced `master`, staging, production, release creation, and artifact promotion;
- used broad `contents: write` permissions;
- included auto-promotion/merge behavior inappropriate for a public developer-preview repository;
- made the repository look production/release-ready before that has been proven.

## Workflows Added

Added:

- `.github/workflows/quality-gates.yml`

The workflow runs on non-draft pull requests targeting `main` or `development`, and on manual dispatch.

Gates:

- restore solution;
- run NuGet vulnerability audit;
- build Release;
- run Release tests with normal console verbosity so skipped tests remain visible;
- validate Docker Compose configuration.

Permissions:

- `contents: read` only.

## Repository Policy Files Added

Added:

- `.editorconfig`
- `Directory.Build.props`

The settings are deliberately conservative:

- normalize line endings and final newline expectations;
- preserve standard C# indentation;
- enable deterministic builds;
- set `ContinuousIntegrationBuild` only when `CI=true`.

No repo-wide formatting churn or aggressive analyzer policy was introduced.

## Risky Behavior Restricted

The public workflow set no longer contains:

- branch auto-merge to `master`;
- staging or production promotion;
- release creation;
- artifact promotion from RC to production;
- broad write permissions for normal validation.

ZAP workflows remain separate and continue to run only on non-draft pull requests targeting `main`.

## Deferred Improvements

Deferred until they can be introduced safely:

- `dotnet format --verify-no-changes` as a required gate;
- stricter analyzers;
- warning-as-error policy;
- SBOM generation as a required gate;
- making skipped tests fail CI.

Skipped tests are still visible in CI output. They should be reduced or tracked in `Docs/testing/skipped-tests-backlog.md` once that backlog branch is merged.

## Local Verification

Completed locally on branch `ci/quality-gates`:

```bash
git diff --check
dotnet build OpenCashFlow.sln --configuration Release --no-restore
dotnet test OpenCashFlow.sln --configuration Release --no-build
docker compose config
```

Results:

- `git diff --check`: passed.
- `dotnet build OpenCashFlow.sln --configuration Release --no-restore`: passed with 0 warnings and 0 errors.
- `dotnet test OpenCashFlow.sln --configuration Release --no-build`: passed with 225 passed, 30 skipped, 0 failed.
- `docker compose config`: passed.

Local note:

- `dotnet list OpenCashFlow.sln package --vulnerable --include-transitive` was attempted locally but did not complete in this sandboxed session. The CI workflow still includes the audit as a GitHub-hosted check where NuGet advisory access is expected to be available.
