# Current Dependencies

This document captures the current dependency shape before the Clean Architecture migration.

## Solution Projects

The active core solution currently contains:

```text
src/OpenCashFlow.API/OpenCashFlow.API.csproj
src/OpenCashFlow.Shared/OpenCashFlow.Shared.csproj
src/OpenCashFlow.WebApp/OpenCashFlow.WebApp.csproj
tests/OpenCashFlow.Test/OpenCashFlow.Test.csproj
```

`src/OpenCashFlow.Admin` still exists physically as legacy code, but it is not part of the core solution/runtime.

## Project References

```text
OpenCashFlow.API
  -> OpenCashFlow.Shared

OpenCashFlow.WebApp
  -> OpenCashFlow.Shared

OpenCashFlow.Test
  -> OpenCashFlow.API
  -> OpenCashFlow.Shared

OpenCashFlow.Shared
  -> no project references
```

## Current Responsibility Distribution

### OpenCashFlow.API

Current API responsibilities:

- HTTP controllers.
- API middleware and startup composition.
- Repository interfaces and EF repository implementations.
- Application/business services.
- Authentication, registration, reset password and token flows.
- Setup flow orchestration.
- Payment, cash, company, employee, role and audit orchestration.
- AutoMapper registration.

Clean Architecture target:

- Keep HTTP concerns only: controllers, middleware, API startup and request/response wiring.
- Move use cases and service interfaces to `OpenCashFlow.Application`.
- Move EF repository implementations and persistence concerns to `OpenCashFlow.Infrastructure`.

### OpenCashFlow.WebApp

Current WebApp responsibilities:

- MVC controllers and views.
- Web UI services that call the API.
- SignalR hub and UI-specific handlers.
- Local UI models.
- Static assets and localization resources.

Clean Architecture target:

- Keep UI behavior, views, MVC controllers and API client services.
- Continue consuming shared contracts/DTOs while `Shared` is reduced.
- Avoid domain/business rules in WebApp controllers/services.

### OpenCashFlow.Shared

Current `Shared` is overloaded. It contains:

- EF Core `ApplicationDbContext` and design-time factory.
- EF Core migrations and model snapshot.
- Domain-like models for company, payments, cash, identity, terms and audit.
- Legacy SaaS schema models: `Plan`, `Plan_Feature`, `Plan_Price`, `Company_Subscription`, `Company_Renewal`, `Stripe_Webhook_Event`.
- DTOs and API response wrapper.
- Enums and constants.
- AutoMapper profile.
- Email and Slack service implementations.
- Options/configuration objects.
- Security helpers such as password hashing, cookie signing and secret generation.
- Module manifest/registry prototypes.

Clean Architecture target:

- Reduce `Shared` to DTOs/contracts, neutral constants and cross-process contract types.
- Move EF Core, migrations, repository implementations, email and persistence concerns to `Infrastructure`.
- Move pure domain rules/types to `Domain`.
- Move use-case contracts and application abstractions to `Application`.

### OpenCashFlow.Test

Current tests reference API and Shared directly and include:

- API/integration-style tests.
- Unit tests against current API services/repositories.
- Database tests, some currently excluded from compile.
- Test factories and fixtures.

Clean Architecture target:

- Keep existing test project during migration.
- Add focused layer tests later only when behavior has moved:
  - `OpenCashFlow.Domain.Tests`
  - `OpenCashFlow.Application.Tests`
  - `OpenCashFlow.Infrastructure.Tests`
  - `OpenCashFlow.API.Tests`

## Current Improper Couplings

- `Shared` depends on EF Core, Npgsql, MailKit, MimeKit and AutoMapper, so every consumer of DTOs also receives infrastructure dependencies.
- `API` contains both use cases and EF repositories.
- `API` depends directly on `Shared.Data.ApplicationDbContext`.
- `WebApp` consumes Shared domain/EF-shaped models in some views.
- `Shared` still contains legacy SaaS/Stripe schema for database compatibility.
- Migrations live in `Shared`, which blocks making `Shared` a contracts-only package.

## Current Safe Boundary

The safest first migration boundary is additive:

```text
Domain         new project, no dependencies
Application    new project, depends on Domain
Infrastructure new project, depends on Application, Domain and temporarily Shared
API            additionally references Application and Infrastructure
WebApp         remains on Shared contracts for now
Shared         unchanged until extraction starts
```

