# Migration Map

This map classifies current code and states where it should move during the Clean Architecture migration.

## Summary

| Current location | Current content | Target |
|---|---|---|
| `OpenCashFlow.Shared/DTOs` | API/WebApp DTOs | Keep in `Shared` temporarily; later `Shared/Contracts` |
| `OpenCashFlow.Shared/Models/Payments` | EF-shaped payment models | Eventually split: pure rules to `Domain`, EF models to `Infrastructure/Persistence` |
| `OpenCashFlow.Shared/Models/Cash` | EF-shaped cash balance/ledger models | Domain rules to `Domain/Cash`; EF persistence to `Infrastructure/Persistence` |
| `OpenCashFlow.Shared/Models/Companies` | Company, fiscal data, staff and legacy SaaS schema | Core company concepts to `Domain/Companies`; EF models to `Infrastructure`; legacy SaaS kept isolated |
| `OpenCashFlow.Shared/Models/Identity` | ASP.NET Identity-shaped persistence models | `Infrastructure/Auth` or `Infrastructure/Persistence/Identity` |
| `OpenCashFlow.Shared/Data` | DbContext, factory and migrations | `Infrastructure/Persistence` in a dedicated migration phase |
| `OpenCashFlow.Shared/Services` | SMTP/Slack implementations | `Infrastructure/Email` and `Infrastructure/Notifications` |
| `OpenCashFlow.Shared/Core` | Password/cookie/secret/config helpers | Split between `Application/Security` abstractions and `Infrastructure/Auth` implementations |
| `OpenCashFlow.Shared/Mappings` | AutoMapper profile | `Application` or `API` depending on DTO direction |
| `OpenCashFlow.Shared/Enums` | Mixed domain, permissions and UI constants | Domain enums to `Domain`; permissions/contracts to `Shared` or `Application/Security` |
| `OpenCashFlow.API/Services` | Use cases and orchestration | `Application` |
| `OpenCashFlow.API/Repositories/Interfaces` | Repository contracts | `Application/Abstractions` or feature folders |
| `OpenCashFlow.API/Repositories` | EF repository implementations | `Infrastructure/Repositories` |
| `OpenCashFlow.API/Controllers` | HTTP endpoints | Stay in `API`, become thinner |
| `OpenCashFlow.WebApp/Services` | API client services | Stay in `WebApp`, consume contracts |

## Implemented Slices

### Payments/CreatePayment Application Use Case

`PaymentService.AddPaymentAsync` delegates create-payment validation, entry type normalization, date normalization and cash delta calculation to:

```text
OpenCashFlow.Application/Payments/CreatePayment/CreatePaymentUseCase
```

### Payments/CreatePayment Application Ports

Application now defines the first ports for create-payment orchestration:

```text
OpenCashFlow.Application/Abstractions/IPaymentReader
OpenCashFlow.Application/Abstractions/IPaymentWriter
OpenCashFlow.Application/Abstractions/IDailyPaymentWriter
OpenCashFlow.Application/Abstractions/ICashLedgerReader
OpenCashFlow.Application/Abstractions/ICashLedgerWriter
OpenCashFlow.Application/Abstractions/IAuditWriter
OpenCashFlow.Application/Abstractions/IUnitOfWork
```

`CreatePaymentOrchestrator` now owns:

- idempotency check by `RequestId`;
- payment creation through `IPaymentWriter`;
- daily payment aggregation through `IDailyPaymentWriter`;
- cash-like method decision;
- cash ledger application through `ICashLedgerWriter`;
- audit write through `IAuditWriter`.
- create-payment transaction boundary through `IUnitOfWork`.

Update/delete payment orchestration now lives in:

```text
OpenCashFlow.Application/Payments/UpdatePayment/UpdatePaymentUseCase
OpenCashFlow.Application/Payments/UpdatePayment/UpdatePaymentOrchestrator
OpenCashFlow.Application/Payments/DeletePayment/DeletePaymentUseCase
OpenCashFlow.Application/Payments/DeletePayment/DeletePaymentOrchestrator
```

Those flows own:

- update validation and normalization;
- payment update/delete through `IPaymentWriter`;
- previous daily-payment effect removal and new effect application;
- cash ledger void/apply/reapply/update decisions;
- update/delete audit writes;
- update/delete transaction boundaries through `IUnitOfWork`.

Payment read/report/calendar flows now live in:

```text
OpenCashFlow.Application/Payments/GetPayments/GetPaymentsUseCase
OpenCashFlow.Application/Payments/GetPaymentDetail/GetPaymentDetailUseCase
OpenCashFlow.Application/Payments/Reports/GetPaymentReportsUseCase
OpenCashFlow.Application/Payments/Calendar/GetPaymentCalendarUseCase
```

Those query use cases use read ports with Application records, not EF models or public DTOs:

```text
OpenCashFlow.Application/Payments/Ports/IPaymentQueryReader
OpenCashFlow.Application/Payments/Ports/IPaymentReportReader
OpenCashFlow.Application/Payments/Ports/IPaymentCalendarReader
OpenCashFlow.Application/Payments/Queries/PaymentListQuery
OpenCashFlow.Application/Payments/Queries/PaymentListItem
OpenCashFlow.Application/Payments/Queries/PaymentDetailResult
OpenCashFlow.Application/Payments/Queries/DailyPaymentResult
OpenCashFlow.Application/Payments/Queries/PaymentCalendarEventResult
```

Infrastructure adapters implement those ports:

```text
OpenCashFlow.Infrastructure/ApplicationAdapters/PaymentReaderAdapter
OpenCashFlow.Infrastructure/ApplicationAdapters/PaymentWriterAdapter
OpenCashFlow.Infrastructure/ApplicationAdapters/DailyPaymentWriterAdapter
OpenCashFlow.Infrastructure/ApplicationAdapters/CashLedgerWriterAdapter
OpenCashFlow.Infrastructure/ApplicationAdapters/AuditWriterAdapter
```

Payment read port implementations live in:

```text
OpenCashFlow.Infrastructure/Payments/PaymentQueryReader
OpenCashFlow.Infrastructure/Payments/PaymentReportReader
OpenCashFlow.Infrastructure/Payments/PaymentCalendarReader
```

Cash/audit persistence and create-payment unit of work now live in Infrastructure:

```text
OpenCashFlow.Infrastructure/Cash/ICashLedgerRepository
OpenCashFlow.Infrastructure/Cash/CashLedgerRepository
OpenCashFlow.Infrastructure/Audit/IAuditRepository
OpenCashFlow.Infrastructure/Audit/AuditRepository
OpenCashFlow.Infrastructure/Persistence/EfUnitOfWork
```

Payment repository contracts and EF implementation are now split:

```text
OpenCashFlow.Application/Payments/Repositories/IPaymentRepository
OpenCashFlow.Infrastructure/Payments/PaymentRepository
OpenCashFlow.Infrastructure/Payments/PaymentRepository.PaymentReports
OpenCashFlow.Infrastructure/Payments/PaymentRepository.Calendar
```

Payment method/document type lookup CRUD now has dedicated Application use cases and Infrastructure EF ports:

```text
OpenCashFlow.Application/Payments/Lookups
OpenCashFlow.Application/Payments/PaymentMethods
OpenCashFlow.Application/Payments/DocumentTypes
OpenCashFlow.Application/Payments/Ports/IPaymentMethodReader
OpenCashFlow.Application/Payments/Ports/IPaymentMethodWriter
OpenCashFlow.Application/Payments/Ports/IDocumentTypeReader
OpenCashFlow.Application/Payments/Ports/IDocumentTypeWriter
OpenCashFlow.Infrastructure/Payments/Lookups/PaymentMethodReader
OpenCashFlow.Infrastructure/Payments/Lookups/PaymentMethodWriter
OpenCashFlow.Infrastructure/Payments/Lookups/DocumentTypeReader
OpenCashFlow.Infrastructure/Payments/Lookups/DocumentTypeWriter
```

Temporary compromise: `IPaymentRepository` currently references `OpenCashFlow.Shared` EF-shaped payment and daily-payment models. Lookup CRUD has been removed from that repository, but payment write/read-for-update and daily aggregation still need a later split.

The API still owns:

- public DTO adaptation for create/update/delete/list/detail/report/calendar/payment methods/document types;

This is intentional. It moves create/update/delete payment orchestration, read/report/calendar query orchestration, adapter registration, transaction boundaries and persistence implementation out of the API service without changing controller routes, DTOs, EF models or migrations. No `Infrastructure -> API` dependency was introduced.

Payment method/document type CRUD no longer uses `IPaymentRepository` or AutoMapper inside `PaymentService`.

## Domain Candidates

Move only after separating from EF attributes and persistence navigation concerns:

- `Payment`
- `Payment_EntryType` or a replacement `EntryType` value object/enum
- `Payment_Method_LookUps` as a domain lookup concept, if kept core
- `Payment_DocumentType_LookUp` as a domain lookup concept, if kept core
- `CashBalance`
- `CashLedger`
- `Company` core identity and workspace data
- `AuditEventType`
- Value objects:
  - `Money`
  - `TenantId`
  - `UserId`
  - `PaymentId`
  - `PaymentAmount`

Do not move EF-backed classes directly if doing so would drag EF attributes or migrations into `Domain`.

## Application Candidates

Move use-case orchestration here:

- Setup:
  - setup status
  - first company/admin creation
- Authentication:
  - login orchestration
  - registration policy
  - forgot/reset password
  - token refresh policy
- Company:
  - get/update company settings
  - employee/user management
- Payments:
  - create payment
  - update payment
  - delete payment
  - list/filter payments
  - payment document types
  - payment methods
- Cash:
  - get balance
  - list ledger
  - rebuild balance
- Audit:
  - write audit event
  - list audit log
- Export:
  - payments CSV
  - cash ledger CSV

Repository interfaces should move with the use cases, not stay in API.

## Infrastructure Candidates

Move implementation details here:

- `ApplicationDbContext`
- design-time DbContext factory
- migrations
- EF configurations
- EF repositories
- SMTP email sender
- Slack notifier
- password hash/token/cookie signing implementations if they require external libraries or environment secrets
- current-user accessor if backed by ASP.NET `HttpContext`
- audit persistence writer

Temporary dependency on `OpenCashFlow.Shared` is allowed until EF models and DTOs are split.

## Shared Candidates

Keep temporarily:

- `ApiResponse`
- request/response DTOs used by both API and WebApp
- neutral constants/enums required by API and WebApp
- module manifest contracts if they remain runtime-neutral

Move later:

- EF models
- migrations
- AutoMapper profile
- services
- security helpers with implementation details

## Legacy SaaS Schema

These types should not enter `Domain` or `Application` core:

- `Plan`
- `Plan_Feature`
- `Plan_Price`
- `Company_Subscription`
- `Company_Renewal`
- `Stripe_Webhook_Event`
- Stripe fields on company/subscription models

Current handling:

- Keep them as persistence legacy while migrations/schema still require them.
- Do not register Stripe/Billing runtime services.
- When safe, isolate them under an optional billing module or archive migration path.

## First Implementation Order

1. Add base projects and references.
2. Add Domain value objects that do not touch EF.
3. Add one Application use case for payment creation while repository code remains in API.
4. Add Application ports for payment persistence, cash ledger and audit.
5. Move create-payment orchestration to Application while API keeps transaction and daily aggregation.
6. Move payment repository interface to Application.
7. Move payment EF repository implementation to Infrastructure.
8. Move cash/audit persistence implementations behind Application ports.
9. Reduce `PaymentService.AddPaymentAsync` to command/result mapping.
10. Move update/delete payment orchestration behind Application use cases.
11. Reduce `PaymentService.UpdatePaymentAsync` and `PaymentService.DeletePaymentAsync` to command/result mapping.
12. Move list/detail/report/calendar payment reads behind Application query use cases.
13. Move payment method/document type lookup CRUD behind Application use cases.
14. Register moved services from API.
15. Run build/test after each slice.
