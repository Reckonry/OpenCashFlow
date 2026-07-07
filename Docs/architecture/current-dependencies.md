# Current Dependencies

This document captures the current dependency shape after dismantling `OpenCashFlow.Shared`.

## Solution Projects

The active core solution contains:

```text
src/OpenCashFlow.API/OpenCashFlow.API.csproj
src/OpenCashFlow.Application/OpenCashFlow.Application.csproj
src/OpenCashFlow.Contracts/OpenCashFlow.Contracts.csproj
src/OpenCashFlow.Domain/OpenCashFlow.Domain.csproj
src/OpenCashFlow.Infrastructure/OpenCashFlow.Infrastructure.csproj
src/OpenCashFlow.WebApp/OpenCashFlow.WebApp.csproj
tests/OpenCashFlow.Application.Tests/OpenCashFlow.Application.Tests.csproj
tests/OpenCashFlow.Domain.Tests/OpenCashFlow.Domain.Tests.csproj
tests/OpenCashFlow.Test/OpenCashFlow.Test.csproj
```

`src/OpenCashFlow.Admin` still exists physically as legacy code, but it is not part of the core solution/runtime.

## Project References

```text
OpenCashFlow.API
  -> OpenCashFlow.Application
  -> OpenCashFlow.Contracts
  -> OpenCashFlow.Infrastructure

OpenCashFlow.Application
  -> OpenCashFlow.Domain

OpenCashFlow.Infrastructure
  -> OpenCashFlow.Application
  -> OpenCashFlow.Contracts
  -> OpenCashFlow.Domain

OpenCashFlow.WebApp
  -> OpenCashFlow.Contracts

OpenCashFlow.Test
  -> OpenCashFlow.API
  -> OpenCashFlow.Contracts
  -> OpenCashFlow.Infrastructure

OpenCashFlow.Contracts
  -> no project references

OpenCashFlow.Domain
  -> no project references
```

## Responsibility Distribution

### OpenCashFlow.API

- HTTP controllers.
- Middleware and startup composition.
- Boundary mapping between public DTOs and application/persistence results.
- Dependency injection composition for Application and Infrastructure.

### OpenCashFlow.Application

- Use cases and orchestration.
- Application ports.
- Payment/cash/audit module contracts.
- No EF Core, ASP.NET, AutoMapper, MailKit, MimeKit, Infrastructure, API or `OpenCashFlow.Shared` dependency.
- Does not reference `OpenCashFlow.Contracts`; API maps public DTOs to Application commands/results.

### OpenCashFlow.Domain

- Pure domain primitives and rules.
- No dependency on EF Core, ASP.NET, Infrastructure, Application, Contracts or Shared.

### OpenCashFlow.Infrastructure

- EF Core persistence.
- `ApplicationDbContext`, design-time factory and migrations.
- EF entities under `Persistence/Entities`.
- Repository/readers/writers implementing Application ports.
- Email, notifications, auth helpers, logging constants and unit of work.
- Employee create/update/PIN and auth reset-token persistence implementations.
- Auth registration/login/fast-login implementations for EF user reads/writes, password hashing, PIN verification, fast-login cookie signing and JWT issuing.
- Auth audit persistence through `IAuthAuditWriter`.

### OpenCashFlow.Contracts

- Public API/WebApp DTOs.
- Public response envelope.
- Neutral public enums/constants.
- No EF Core, AutoMapper, MailKit, MimeKit or runtime service implementations.

### OpenCashFlow.WebApp

- MVC controllers, views and API client services.
- Consumes `OpenCashFlow.Contracts`.
- Uses local WebApp view models for Razor-only form/list/detail models.
- Does not reference `OpenCashFlow.Infrastructure`.
- Owns WebApp-only security header composition, including CSP nonce generation and the Razor tag helper that applies nonces to script/style elements.

## Removed Project

`OpenCashFlow.Shared` has been removed from the solution and from all core project references. It no longer owns DTOs, EF entities, runtime services, mappings, options, helpers, enums or migrations.

## Auth Boundary

Registration, login, fast-login, fast-login cookie generation, account confirmation and resend confirmation now enter `OpenCashFlow.Application/Auth` use cases from the API boundary. Infrastructure implements the auth ports for EF reads/writes, password hashing, PIN verification, cookie signing, JWT issuing and optional notifications.

`AuthenticationService` remains an API adapter for public DTO/response shape and HTTP-bound audit context collection. It no longer depends on `IEmployeeRepository` or `ApplicationDbContext`; auth audit persistence is implemented by Infrastructure through `IAuthAuditWriter`.

## Fase 3J Non-Auth API Services

`OpenCashFlow.API` no longer uses `ApplicationDbContext` directly in non-auth services/controllers/repositories. Roles, cash, user management, audit logs and health checks now cross the Application boundary and are implemented by Infrastructure ports/readers/writers.

`OpenCashFlow.Infrastructure` now additionally owns:

- `Roles/RoleReader`;
- `Cash/CashReader` and `Cash/CashWriter`;
- `UserManagement/UserManagementStore`;
- `AuditLog/AuditLogStore`;
- `Health/DatabaseHealthReader`.

Next cleanup: remove residual EF entity types from public controller/service signatures where they still appear as compatibility types, replacing them with Contracts or Application records as appropriate.

## Fase 3K API Boundary Cleanup

`OpenCashFlow.API` controllers, services and service interfaces no longer import `OpenCashFlow.Infrastructure.Persistence.Entities`. Payment daily report responses now use the neutral `OpenCashFlow.Contracts.DTOs.Payments.Payment_DailyPayments` contract.

Dependency direction remains:

- API -> Contracts/Application/Infrastructure composition;
- Application -> Domain only;
- WebApp -> Contracts only;
- Infrastructure -> EF entities/persistence.

## .NET 10 LTS Migration

The active solution now targets `net10.0` across core and test projects. `global.json` selects the .NET 10 SDK line with feature roll-forward enabled.

Runtime package baseline:

- ASP.NET Core, EF Core and Microsoft.Extensions packages use `10.0.9`.
- `Npgsql.EntityFrameworkCore.PostgreSQL` uses `10.0.2`.
- `System.IdentityModel.Tokens.Jwt` uses `8.19.1`.
- `System.Linq.Dynamic.Core` uses `1.7.2`.
- `Microsoft.OpenApi` is pinned to `2.10.0` to avoid the vulnerable `2.0.x` transitive resolution.
- `Swashbuckle.AspNetCore` uses `10.2.3` because version 6.x is not compatible with the safe Microsoft.OpenApi 2.x namespace layout.

Docker images use `mcr.microsoft.com/dotnet/sdk:10.0` and `mcr.microsoft.com/dotnet/aspnet:10.0`. GitHub Actions use `10.0.x`.

Deferred dependency tracks:

- `Polly` 7 -> 8 requires a focused resilience-policy migration.
- `Sentry` 5 -> 6 requires telemetry initialization review.
- `Asp.Versioning` 8 -> 10 requires route/version compatibility testing.
- `coverlet`, `Testcontainers` and `Microsoft.NET.Test.Sdk` major upgrades should be handled as test-infrastructure work.

## WebApp CSP Hardening

The WebApp now emits a nonce-based CSP without `unsafe-inline` or `unsafe-eval`. `OpenCashFlow.WebApp.Security.CspNonceTagHelper` applies the request nonce to Razor-rendered `<script>` and `<style>` tags, allowing the legacy Razor views to keep working while the frontend is progressively moved toward external JS/CSS assets.

The ZAP Baseline workflow keeps the structured JSON quality gate and no longer contains a CSP allowlist. New Medium/High ZAP findings fail the workflow.
