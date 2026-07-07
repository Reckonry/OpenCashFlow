# Migration Map

This map records the current Clean Architecture ownership after the payments/cash slices and the removal of `OpenCashFlow.Shared`.

## Current Ownership

| Area | Owner |
| --- | --- |
| Public API/WebApp DTOs and response envelope | `OpenCashFlow.Contracts` |
| Neutral public enums/constants | `OpenCashFlow.Contracts` |
| Domain primitives and rules | `OpenCashFlow.Domain` |
| Use cases and application ports | `OpenCashFlow.Application` |
| Payment create/update/delete orchestration | `OpenCashFlow.Application/Payments` |
| Payment list/detail/report/calendar queries | `OpenCashFlow.Application/Payments` |
| Payment method/document type use cases | `OpenCashFlow.Application/Payments` |
| Module manifest/registry prototype | `OpenCashFlow.Application/Modules` |
| EF entities | `OpenCashFlow.Infrastructure/Persistence/Entities` |
| DbContext, design-time factory and migrations | `OpenCashFlow.Infrastructure/Persistence` |
| EF repositories/readers/writers | `OpenCashFlow.Infrastructure` |
| Cash/audit persistence | `OpenCashFlow.Infrastructure/Cash`, `OpenCashFlow.Infrastructure/Audit` |
| Email, notification and auth implementations | `OpenCashFlow.Infrastructure` |
| HTTP controllers and boundary mapping | `OpenCashFlow.API` |
| MVC UI and API client services | `OpenCashFlow.WebApp` |

## Implemented Slices

### Payments Commands

Create, update and delete payment orchestration now lives in Application:

```text
OpenCashFlow.Application/Payments/CreatePayment
OpenCashFlow.Application/Payments/UpdatePayment
OpenCashFlow.Application/Payments/DeletePayment
```

These use cases own validation, idempotency decisions, daily-payment aggregation, cash ledger decisions, audit writing and unit-of-work boundaries through Application ports.

### Payment Queries

List, detail, report and calendar reads now pass through Application query use cases and read ports:

```text
OpenCashFlow.Application/Payments/GetPayments
OpenCashFlow.Application/Payments/GetPaymentDetail
OpenCashFlow.Application/Payments/Reports
OpenCashFlow.Application/Payments/Calendar
OpenCashFlow.Application/Payments/Ports
```

Infrastructure implements the query readers and maps EF entities to Application records.

### Payment Lookups

Payment methods and document types now have dedicated Application use cases and Infrastructure EF readers/writers:

```text
OpenCashFlow.Application/Payments/PaymentMethods
OpenCashFlow.Application/Payments/DocumentTypes
OpenCashFlow.Infrastructure/Payments/Lookups
```

`PaymentService.PaymentMethod.cs` and `PaymentService.DocumentType.cs` now adapt public DTOs to Application commands/queries and map results back to public DTOs.

### Payment Persistence

The broad `IPaymentRepository` has been removed. Payment persistence now uses smaller ports:

```text
OpenCashFlow.Application/Payments/Persistence/IPaymentPersistenceReader
OpenCashFlow.Application/Payments/Persistence/IPaymentPersistenceWriter
OpenCashFlow.Application/Payments/Persistence/IDailyPaymentPersistence
OpenCashFlow.Infrastructure/Payments/Persistence
```

### EF Persistence

EF persistence is owned by Infrastructure:

```text
OpenCashFlow.Infrastructure/Persistence/ApplicationDbContext
OpenCashFlow.Infrastructure/Persistence/ApplicationDbContextFactory
OpenCashFlow.Infrastructure/Persistence/Migrations
OpenCashFlow.Infrastructure/Persistence/Entities
```

The EF migration command is:

```bash
dotnet ef migrations list \
  --project src/OpenCashFlow.Infrastructure/OpenCashFlow.Infrastructure.csproj \
  --startup-project src/OpenCashFlow.API/OpenCashFlow.API.csproj
```

The helper script `scripts/create-migration.sh` also points to the Infrastructure project and writes migrations under `Persistence/Migrations`.

### Shared Removal

`OpenCashFlow.Shared` has been removed from the solution and all core project references. Its previous contents were moved as follows:

- contracts to `OpenCashFlow.Contracts`;
- services/options/helpers to `OpenCashFlow.Application` or `OpenCashFlow.Infrastructure`;
- mapping profile to `OpenCashFlow.API`;
- module records to `OpenCashFlow.Application`;
- EF entities and legacy schema to `OpenCashFlow.Infrastructure/Persistence/Entities`.

## Legacy SaaS Schema

Legacy SaaS/Billing persistence types such as `Plan`, `Plan_Feature`, `Plan_Price`, `Company_Subscription`, `Company_Renewal` and `Stripe_Webhook_Event` remain only as EF schema compatibility entities under Infrastructure. They must not be registered as Billing/Stripe runtime services in the core.

## WebApp Infrastructure Decoupling

`OpenCashFlow.WebApp` no longer references `OpenCashFlow.Infrastructure`. EF-shaped models previously consumed by views/services were replaced with:

- contract models in `OpenCashFlow.Contracts` for API/WebApp JSON exchange;
- local WebApp view models for Razor-only form/detail/list rendering.

## Company, Employee And Setup Use Cases

Setup is now behind Application use cases and Infrastructure ports:

```text
OpenCashFlow.Application/Setup/GetSetupStatus
OpenCashFlow.Application/Setup/CompleteSetup
OpenCashFlow.Application/Setup/Ports
OpenCashFlow.Infrastructure/Setup
```

`SetupController` no longer depends on `ApplicationDbContext`, opens EF transactions, creates roles/users/companies directly, or hashes passwords. It maps the public setup DTO to `CompleteSetupCommand` and maps the result back to HTTP responses.

Company read and invoice queries now use Application use cases and Infrastructure readers:

```text
OpenCashFlow.Application/Companies/GetCompany
OpenCashFlow.Application/Companies/GetCompanies
OpenCashFlow.Application/Companies/Invoices
OpenCashFlow.Application/Companies/Ports/ICompanyReader
OpenCashFlow.Infrastructure/Companies/CompanyReader
```

Employee list/detail/delete/update-profile now use Application use cases and Infrastructure readers/writers:

```text
OpenCashFlow.Application/Employees/GetEmployees
OpenCashFlow.Application/Employees/GetEmployeeDetail
OpenCashFlow.Application/Employees/DeleteEmployee
OpenCashFlow.Application/Employees/UpdateMyProfile
OpenCashFlow.Application/Employees/Ports
OpenCashFlow.Infrastructure/Employees
```

Resolved in Fase 3G: Company/Employee/Setup use cases now use Application-owned record models. API services map `OpenCashFlow.Contracts` DTOs to Application commands/results and back to public DTOs. `OpenCashFlow.Application` no longer references `OpenCashFlow.Contracts`.

Employee create/update/PIN and auth forgot/reset password now use Application use cases and Infrastructure ports for EF persistence, hashing, token storage and notifications.

## Next Recommended Slices

1. Move company write/update/delete flows behind Application ports when those endpoints are reactivated or formalized.
2. Continue reducing direct `ApplicationDbContext` usage in non-auth API services such as user-management, cash and role administration.
3. Split EF entity configuration from entity classes where useful.
4. Isolate legacy Billing/Stripe schema into a clearly named legacy folder or optional module migration plan.

## Auth Registration/Login/FastLogin Use Cases

Fase 3H moved the remaining auth flows behind Application use cases:

```text
OpenCashFlow.Application/Auth/Login
OpenCashFlow.Application/Auth/FastLogin
OpenCashFlow.Application/Auth/Register
OpenCashFlow.Application/Auth/AccountConfirmation
OpenCashFlow.Application/Auth/Ports
OpenCashFlow.Infrastructure/Auth
```

API `AuthenticationService` now maps public contracts to Application commands/results for login, fast-login, fast-login cookie generation, registration, confirmation, resend confirmation and token regeneration. Infrastructure owns EF user reads/writes, password hashing, PIN verification, cookie signing, JWT issuing and notification delivery.

`IEmployeeRepository` and its API implementation were removed.

## Auth Audit Port

Fase 3I moved auth audit persistence behind an Application port and Infrastructure writer:

```text
OpenCashFlow.Application/Auth/Audit/AuthAuditEvent
OpenCashFlow.Application/Auth/Ports/IAuthAuditWriter
OpenCashFlow.Infrastructure/Auth/AuthAuditWriter
```

`AuthenticationService` still collects HTTP-bound values such as IP address and User-Agent, but it no longer depends on `ApplicationDbContext` and no longer writes directly to `Admin_AuditLog_DS`.

## Non-Auth API Persistence Removal

Fase 3J removed direct `ApplicationDbContext` usage from non-auth API services/controllers.

Migrated areas:

- `RoleService` now uses `IGetRolesUseCase` and `IRoleReader`; EF role reads live in `OpenCashFlow.Infrastructure/Roles`.
- `CashService` now delegates to Application cash use cases/ports; cash balance and ledger persistence live in `OpenCashFlow.Infrastructure/Cash`.
- `UserManagementService` now maps admin DTOs to Application records and delegates to `IUserManagementUseCase`; EF user/role/company-staff persistence lives in `OpenCashFlow.Infrastructure/UserManagement`.
- `AuditLogService` now maps audit DTOs/events to `IAuditLogUseCase`; audit log persistence lives in `OpenCashFlow.Infrastructure/AuditLog`.
- `HealthController` now uses `IDatabaseHealthReader`; database connectivity checks live in Infrastructure.

API services may still reference public `OpenCashFlow.Contracts` DTOs and collect HTTP-bound context such as current user, IP address and User-Agent. They must not inject `ApplicationDbContext`, access `DbSet`, call `SaveChangesAsync`, or open EF transactions.

## API Boundary EF Entity Cleanup

Fase 3K removed residual `OpenCashFlow.Infrastructure.Persistence.Entities` imports from API controllers, API services and API service interfaces. The only runtime type that still crossed the payment report boundary, `Payment_DailyPayments`, was replaced with a neutral contract under `OpenCashFlow.Contracts.DTOs.Payments` preserving the existing JSON shape.

Result: API boundary code uses Contracts, Application records, primitives and HTTP types only. Infrastructure remains the owner of EF persistence entities.
