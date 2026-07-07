# Clean Architecture Plan

OpenCashFlow will move toward Clean Architecture incrementally. The first rule is that every step must keep the self-hosted product buildable, testable and runnable with Docker.

## Target Layers

```text
OpenCashFlow.Domain
  Pure business concepts and rules.
  No EF Core, ASP.NET, HTTP, JWT, SMTP, AutoMapper or UI dependencies.

OpenCashFlow.Application
  Use cases, application services, repository interfaces and application abstractions.
  Depends on Domain.

OpenCashFlow.Infrastructure
  EF Core, migrations, repository implementations, email, auth helpers and audit persistence.
  Depends on Application and Domain.
  May temporarily depend on Shared while legacy models/DbContext are extracted.

OpenCashFlow.API
  HTTP API, controllers, middleware and dependency injection composition.
  Depends on Application and Infrastructure.
  Consumes Contracts for public DTOs.

OpenCashFlow.WebApp
  MVC web UI, views, UI controllers and API client services.
  Consumes API/contracts and must not contain domain rules.

OpenCashFlow.Contracts
  Public DTOs/contracts, neutral constants and response wrappers only.

OpenCashFlow.Shared
  Removed from the core solution.
```

## Migration Principles

- No big-bang move.
- No business behavior changes during structural migration.
- No Stripe/Billing/Admin runtime dependency reintroduced into the core.
- Keep EF migrations intact until an explicit migration strategy moves them.
- Prefer simple services and interfaces over adding framework dependencies.
- Keep existing tests green after each step.
- Move one vertical slice at a time after the base projects exist.

## Phase 0 - Documentation And Dependency Baseline

Status: completed for the initial baseline.

Outputs:

- `Docs/architecture/current-dependencies.md`
- `Docs/architecture/clean-architecture-plan.md`
- `Docs/architecture/migration-map.md`

Purpose:

- Make the current coupling explicit.
- Define where each type of code should eventually live.
- Avoid accidental domain/infrastructure mixing during later moves.

## Phase 1 - Base Projects

Status: completed for the initial baseline.

Create:

```text
src/OpenCashFlow.Domain
src/OpenCashFlow.Application
src/OpenCashFlow.Infrastructure
```

Initial references:

```text
OpenCashFlow.Domain
  no project references

OpenCashFlow.Application
  -> OpenCashFlow.Domain

OpenCashFlow.Infrastructure
  -> OpenCashFlow.Application
  -> OpenCashFlow.Domain
  -> OpenCashFlow.Contracts

OpenCashFlow.API
  -> OpenCashFlow.Application
  -> OpenCashFlow.Infrastructure
  -> OpenCashFlow.Contracts

OpenCashFlow.WebApp
  -> OpenCashFlow.Contracts
  -> OpenCashFlow.Infrastructure temporary bridge for EF-shaped view models
```

No logic moves in this phase.

## Phase 2 - Domain Extraction

Move only pure concepts first:

- Payment entry type and amount rules.
- Tenant/company identity value objects.
- Cash ledger invariants.
- Audit event categories.
- Payment method/document type concepts if they can be separated from EF attributes.

Avoid moving EF entities if that would force migration churn. When needed, introduce clean domain models and map from EF persistence models later.

## Phase 3 - Application Use Cases

Extract use cases after the domain boundary is stable:

- Setup status and setup completion.
- Create/update/delete/list payments.
- Cash balance and ledger queries.
- Company settings.
- Auth/reset password orchestration.
- Audit writing.
- Export/report use cases.

Application should define interfaces such as:

- `IApplicationDbContext`
- `ICurrentUser`
- `IEmailSender`
- `IAuditWriter`
- Repository interfaces per aggregate/use case.

## Phase 4 - Infrastructure Extraction

Move persistence and implementation details:

- `ApplicationDbContext`
- Migrations
- EF repositories
- Email/SMTP sender
- Audit persistence
- Password hashing/token helpers if they remain infrastructure concerns
- HTTP claims current-user implementation

The API should eventually register infrastructure through:

```csharp
services.AddOpenCashFlowInfrastructure(configuration);
```

## Phase 5 - Shared Cleanup

Reduce Shared to:

- API/WebApp DTOs.
- Contracts.
- Neutral constants/enums.
- Response wrappers.

Remove from Shared:

- EF Core and Npgsql dependencies.
- `ApplicationDbContext` and migrations.
- Repository/service implementations.
- Email/SMTP implementation.
- Domain entities.
- Legacy SaaS schema after an explicit schema decision.

## Phase 6 - Test Architecture

Do not split tests prematurely. Add new test projects when code moves:

```text
tests/OpenCashFlow.Domain.Tests
tests/OpenCashFlow.Application.Tests
tests/OpenCashFlow.Infrastructure.Tests
tests/OpenCashFlow.API.Tests
```

First useful targets:

- Payment domain rules.
- Cash ledger consistency.
- Setup use case.
- Reset password flow.
- Tenant isolation.
- EF repository behavior.

## Recommended Next Slice

After the base projects compile, the safest first real slice is payments/cash:

1. Define pure `EntryType`, `Money`, `TenantId`, `PaymentId` in Domain.
2. Add payment/cash use-case interfaces in Application.
3. Move only validation logic that does not require EF.
4. Keep EF models and repositories in their current location until the use-case boundary is proven.
