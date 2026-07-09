# GitHub Workflows

OpenCashFlow is currently a Developer Preview / Early Self-Hosted Preview. The public repository uses conservative validation workflows only. It does not include deployment, production release, auto-promotion, or branch auto-merge automation.

## Active Workflows

### `quality-gates.yml`

Runs on non-draft pull requests targeting `main` or `development`, and on manual `workflow_dispatch`.

Gates:

- `dotnet restore OpenCashFlow.sln`
- `dotnet list OpenCashFlow.sln package --vulnerable --include-transitive`
- `dotnet build OpenCashFlow.sln --configuration Release --no-restore`
- `dotnet test OpenCashFlow.sln --configuration Release --no-build --logger "console;verbosity=normal"`
- `docker compose config`

Permissions are read-only:

```yaml
permissions:
  contents: read
```

Skipped tests remain visible in CI output because the test command uses normal console verbosity. Skipped tests must be fixed or documented in the testing backlog before production-readiness claims.

### `publish-preview-images.yml`

Runs when a preview tag matching `v*-preview.*` is pushed.

The workflow builds and publishes preview images to GitHub Container Registry:

- `ghcr.io/reckonry/opencashflow-api:<tag>`
- `ghcr.io/reckonry/opencashflow-webapp:<tag>`

Permissions are limited to repository read access and package publishing:

```yaml
permissions:
  contents: read
  packages: write
```

The workflow is release packaging only. It does not deploy any environment.

### `zap-baseline.yml`

Runs OWASP ZAP baseline scans for non-draft pull requests targeting `main`.

The workflow starts the Docker Compose stack, scans the WebApp and API, uploads reports, and fails on Medium or High findings.

### `zap-full.yml`

Runs OWASP ZAP full scans for non-draft pull requests targeting `main`.

The full scan is heavier than the baseline scan and is intended as a security gate for changes approaching the main branch.

## Removed Legacy Workflows

The following workflows were removed because they represented stale promotion/release automation and broad write permissions that do not match the current public developer-preview state:

- `ci-wip-to-master.yml`
- `ci-master-to-staging.yml`
- `ci-staging-to-production.yml`

Those workflows referenced `master`, staging, production, release creation, artifact promotion, and `contents: write`. The public repository should not auto-promote branches or create production-looking releases.

## Deferred Gates

The following checks are intentionally deferred until they can be introduced without noisy churn:

- repo-wide `dotnet format --verify-no-changes`;
- stricter analyzer rules;
- treating all warnings as errors;
- SBOM publishing as a required PR gate.
