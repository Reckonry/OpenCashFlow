# Dependency Alert Reconciliation

Date: 2026-07-07
Branch: `security/reconcile-dependency-alerts`

## Goal

GitHub reported dependency vulnerability alerts on the default branch while the local .NET solution scan reported no vulnerable NuGet packages. This note reconciles the difference and documents what was changed.

## Commands Run

```bash
dotnet list OpenCashFlow.sln package --vulnerable --include-transitive
dotnet list OpenCashFlow.sln package --outdated
find . -name package.json -o -name package-lock.json -o -name yarn.lock -o -name pnpm-lock.yaml
dotnet nuget list source
gh auth status
```

## NuGet Result

`dotnet list OpenCashFlow.sln package --vulnerable --include-transitive` completed successfully with the configured NuGet sources and reported no vulnerable packages for:

- `OpenCashFlow.API`
- `OpenCashFlow.WebApp`
- `OpenCashFlow.Application`
- `OpenCashFlow.Infrastructure`
- `OpenCashFlow.Domain`
- `OpenCashFlow.Contracts`
- `OpenCashFlow.Test`
- `OpenCashFlow.Application.Tests`
- `OpenCashFlow.Domain.Tests`

Local NuGet sources:

- `https://api.nuget.org/v3/index.json`
- `https://nuget.pkg.github.com/GoMyRO/index.json`

The GitHub source can make sandboxed package scans slow or inconclusive, so CI remains the authoritative place to run the dependency audit with normal network access.

## Outdated NuGet Packages

`dotnet list OpenCashFlow.sln package --outdated` showed available updates, but they are not vulnerability fixes and were not applied in this cleanup:

- `Asp.Versioning.Mvc` 8.1.1 -> 10.0.0
- `Asp.Versioning.Mvc.ApiExplorer` 8.1.1 -> 10.0.0
- `Microsoft.OpenApi` 2.10.0 -> 3.8.0
- `Polly` 7.2.3 -> 8.7.0
- `Sentry.AspNetCore` 5.9.0 -> 6.6.0
- `Serilog.AspNetCore` 9.0.0 -> 10.0.0
- `coverlet.collector` 6.0.4 -> 10.0.1
- `Microsoft.NET.Test.Sdk` 17.14.1 -> 18.7.0
- `Testcontainers` 4.6.0 -> 4.13.0
- `Testcontainers.PostgreSql` 4.6.0 -> 4.13.0

These are major or tooling updates and should be reviewed in separate PRs with targeted compatibility checks. This branch intentionally avoids broad framework or tooling upgrades.

## Root Cause Found Locally

The repository contained vendored frontend package manifests under:

```text
src/OpenCashFlow.WebApp/wwwroot/libs/**/package.json
```

Those files came from upstream frontend packages and included old runtime, dev, and test dependencies such as bundlers, test runners, webpack, gulp, parcel, eslint, karma, mocha, puppeteer, and related transitive dependency roots.

They are not used by OpenCashFlow's .NET build, Docker build, CI workflows, or runtime asset serving. The application serves the already-vendored static JS/CSS assets, not an npm-installed dependency graph.

GitHub Dependabot can still treat nested `package.json` files as active npm manifests and raise alerts against their dependency metadata. That is the most likely explanation for the mismatch:

- local NuGet advisory scan is clean;
- GitHub reports dependency alerts;
- the repo contained npm manifests outside the .NET solution.

## Fix Applied

Removed all vendored `package.json` files from `src/OpenCashFlow.WebApp/wwwroot/libs`.

This keeps the static runtime assets in place and removes only npm metadata that is not used by the product build. Upstream `README` and `LICENSE` files remain in the vendored asset directories where present, so release attribution can still be reviewed.

After the cleanup, this command returns no files:

```bash
find . -name package.json -o -name package-lock.json -o -name yarn.lock -o -name pnpm-lock.yaml
```

## GitHub Security Tab Limitation

The local `gh` CLI is not authenticated:

```text
The token in default is invalid.
```

Because of that, this branch could not read the GitHub Dependabot alert list directly. After this PR is merged, maintainers should open GitHub Security > Dependabot alerts and confirm whether the 20 alerts close automatically. Any remaining alerts should be classified by ecosystem:

- NuGet: should be unexpected because the solution scan is clean.
- npm: likely stale if no package manifests remain.
- Docker: review base images separately.
- GitHub Actions: update pinned actions if GitHub flags an action advisory.

## Remaining Alerts

Expected after merge:

- npm alerts caused by vendored package manifests should close or become stale after GitHub rescans the default branch.

Possible remaining work:

- Dismiss stale npm alerts with a note pointing to this document if GitHub keeps historical alerts for removed manifests.
- Resolve any Docker or GitHub Actions alerts separately if they remain open.
- Keep `dotnet list OpenCashFlow.sln package --vulnerable --include-transitive` in CI as the NuGet gate.

## Decision

No NuGet package was upgraded in this branch because no vulnerable NuGet package was found.

No broad major upgrades were applied because the outdated list contains compatibility work, not confirmed active vulnerabilities.

The only repository change is removal of inactive frontend package manifests that were likely causing GitHub to treat vendored static assets as npm projects.
