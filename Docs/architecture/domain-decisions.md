# Domain Decisions

This document records decisions made while introducing the first pure Domain slice.

## Payments/Cash Slice

The first domain slice introduces primitives and rules for payments and cash without changing runtime behavior.

Created primitives:

- `TenantId`
- `UserId`
- `Money`
- `PaymentId`
- `PaymentAmount`
- `PaymentEntryType`
- `CashLedgerEntryType`

Created rules:

- `PaymentRules.GetCashDelta`
- `CashBalanceRules.Apply`

## Why Separate Domain Primitives From Current EF Models

Current payment, company and cash persistence models live in `OpenCashFlow.Infrastructure/Persistence/Entities` and are EF-shaped:

- They carry persistence attributes and navigation properties.
- They are used by `OpenCashFlow.Infrastructure.Persistence.ApplicationDbContext`.
- They are tied to the current migrations and database schema.
- Some models still contain legacy SaaS/Billing fields kept for migration compatibility.

Moving those models directly into `OpenCashFlow.Domain` would drag EF Core and schema history into the pure domain layer. Instead, the first step creates small domain primitives that represent stable business concepts without persistence concerns.

## Shared Removal

`OpenCashFlow.Shared` has been removed from the core solution. Its former responsibilities now live in these layers:

- public DTOs and response contracts in `OpenCashFlow.Contracts`;
- EF Core persistence entities in `OpenCashFlow.Infrastructure/Persistence/Entities`;
- AutoMapper profile in `OpenCashFlow.API/Mapping`;
- email/slack ports in `OpenCashFlow.Application/Abstractions`;
- email/slack/security implementations in `OpenCashFlow.Infrastructure`;
- legacy SaaS/Billing schema entities such as `Plan`, `Company_Subscription`, `Company_Renewal` and `Stripe_Webhook_Event` in Infrastructure as schema compatibility only.

This keeps Domain and Application free from EF Core, ASP.NET, AutoMapper, SMTP and Shared-era coupling while preserving runtime behavior.

## Mapping Strategy For Later Phases

The next extraction phases should map between persistence models and domain primitives at the Application/Infrastructure boundary.

Recommended direction:

```text
Infrastructure persistence model
  -> Application use case input
  -> Domain primitive/rule
  -> Application result
  -> DTO/contract
```

Examples:

- `Payment.Amount` maps to `PaymentAmount`.
- `Payment.EntryType` maps to `PaymentEntryType`.
- `Company.TenantID` maps to `TenantId`.
- `CashLedger.Delta` is derived through `PaymentRules.GetCashDelta` or `CashBalanceRules.Apply`.

Do not expose EF models from Domain. Do not add EF attributes to Domain primitives.

## Current Compromises

- WebApp uses Contracts plus local view models and no longer references Infrastructure.
- Domain tests validate primitives independently from current API behavior.
- Negative cash balances are allowed at the domain rule level because the current product has not yet defined a hard overdraft rule.
- `Money.From` allows negative values for calculated balances and deltas, while `Money.NonNegative` and `Money.Positive` enforce stricter creation rules where required.

## Application Use Case: CreatePayment

`OpenCashFlow.Application` now contains the first application use case:

```text
OpenCashFlow.Application/Payments/CreatePayment/
  CreatePaymentCommand
  CreatePaymentResult
  ICreatePaymentUseCase
  CreatePaymentUseCase
```

The use case validates the same creation concerns that were previously embedded directly in the API service:

- tenant id and user id must be present;
- payment id and request id must be present;
- amount must be greater than zero;
- entry type remains compatible with the existing public contract: `Income` or `Outcome`;
- payment method and document type are required;
- payment date is normalized to UTC;
- cash delta is calculated through domain rules.

The use case intentionally returns the existing `Income`/`Outcome` strings instead of exposing the internal domain primitive values `Inflow`/`Outflow`. This preserves API/WebApp behavior while allowing the domain model to use clearer cash-flow terminology internally.

## Application Ports And CreatePayment Orchestration

The create-payment path now has Application-owned ports:

```text
OpenCashFlow.Application/Abstractions/
  IPaymentReader
  IPaymentWriter
  IDailyPaymentWriter
  ICashLedgerReader
  ICashLedgerWriter
  IAuditWriter
  IUnitOfWork
```

`CreatePaymentOrchestrator` coordinates:

- validation through `CreatePaymentUseCase`;
- idempotency lookup by `RequestId`;
- payment creation;
- daily payment aggregation;
- cash ledger application for cash-like methods;
- payment-created audit write.
- transaction boundary for create payment through `IUnitOfWork`.

The ports exchange Application records (`PaymentDraft`, `PaymentSnapshot`, `PaymentMethodSnapshot`) instead of EF models or public DTOs.

## Application Use Cases: UpdatePayment And DeletePayment

`OpenCashFlow.Application` now also owns payment update and delete orchestration:

```text
OpenCashFlow.Application/Payments/UpdatePayment/
  UpdatePaymentCommand
  UpdatePaymentResult
  IUpdatePaymentUseCase
  UpdatePaymentUseCase
  IUpdatePaymentOrchestrator
  UpdatePaymentOrchestrator

OpenCashFlow.Application/Payments/DeletePayment/
  DeletePaymentCommand
  DeletePaymentResult
  IDeletePaymentUseCase
  DeletePaymentUseCase
  IDeletePaymentOrchestrator
  DeletePaymentOrchestrator
```

These flows preserve the existing runtime behavior:

- update validates amount, entry type, tenant, user, method, document type and date;
- update removes the previous daily-payment effect and applies the new one;
- update handles cash transitions: cash to non-cash, non-cash to cash and cash amount/type changes;
- delete remains a soft delete;
- delete removes the daily-payment effect;
- delete voids cash ledger only for cash-like methods;
- update/delete audit writes are handled through `IAuditWriter`;
- update/delete transaction boundaries live behind `IUnitOfWork`.

## Application Query Use Cases: Payment Reads And Reports

Payment read/report flows now use Application query use cases:

```text
OpenCashFlow.Application/Payments/GetPayments/
  GetPaymentsQuery
  IGetPaymentsUseCase
  GetPaymentsUseCase

OpenCashFlow.Application/Payments/GetPaymentDetail/
  GetPaymentDetailQuery
  IGetPaymentDetailUseCase
  GetPaymentDetailUseCase

OpenCashFlow.Application/Payments/Reports/
  IGetPaymentReportsUseCase
  GetPaymentReportsUseCase

OpenCashFlow.Application/Payments/Calendar/
  GetPaymentCalendarQuery
  IGetPaymentCalendarUseCase
  GetPaymentCalendarUseCase
```

The new read ports are:

```text
OpenCashFlow.Application/Payments/Ports/IPaymentQueryReader
OpenCashFlow.Application/Payments/Ports/IPaymentReportReader
OpenCashFlow.Application/Payments/Ports/IPaymentCalendarReader
```

These ports return Application records from `OpenCashFlow.Application/Payments/Queries` instead of EF models or public DTOs. Infrastructure implements the ports with EF readers:

```text
OpenCashFlow.Infrastructure/Payments/PaymentQueryReader
OpenCashFlow.Infrastructure/Payments/PaymentReportReader
OpenCashFlow.Infrastructure/Payments/PaymentCalendarReader
```

## Temporary API Responsibilities

`PaymentService` now keeps only the outer API/runtime concerns for create, update, delete, list, detail, report and calendar:

- public DTO adaptation.

Payment persistence now uses small Application ports instead of the previous broad `IPaymentRepository`:

```text
OpenCashFlow.Application/Payments/Persistence/IPaymentPersistenceReader
OpenCashFlow.Application/Payments/Persistence/IPaymentPersistenceWriter
OpenCashFlow.Application/Payments/Persistence/IDailyPaymentPersistence
```

Infrastructure implements these ports with EF classes:

```text
OpenCashFlow.Infrastructure/Payments/Persistence/PaymentPersistenceReader
OpenCashFlow.Infrastructure/Payments/Persistence/PaymentPersistenceWriter
OpenCashFlow.Infrastructure/Payments/Persistence/DailyPaymentPersistence
OpenCashFlow.Infrastructure/Cash/CashLedgerRepository
OpenCashFlow.Infrastructure/Audit/AuditRepository
OpenCashFlow.Infrastructure/Persistence/EfUnitOfWork
```

The temporary `PaymentReaderAdapter`, `PaymentWriterAdapter`, `DailyPaymentWriterAdapter` and broad `IPaymentRepository` have been removed. `OpenCashFlow.Application` no longer references `OpenCashFlow.Shared`.

`PaymentService.AddPaymentAsync` no longer opens EF transactions directly. The transaction boundary for create payment lives behind `IUnitOfWork` and is implemented by `EfUnitOfWork`.

`PaymentService.UpdatePaymentAsync` and `PaymentService.DeletePaymentAsync` no longer open EF transactions directly and no longer call cash/audit persistence services directly. Their transaction boundaries also live behind `IUnitOfWork`.

`PaymentService.GetAllPaymentsAsync`, `GetPaymentByIdAsync`, report methods and calendar events now map DTOs to Application query records and map Application results back to existing public DTOs/models.

No `Infrastructure -> API` dependency is used.

## Application Lookup Use Cases: Payment Methods And Document Types

Payment method and document type CRUD now uses dedicated Application use cases:

```text
OpenCashFlow.Application/Payments/PaymentMethods/
  GetPaymentMethodsUseCase
  GetPaymentMethodDetailUseCase
  CreatePaymentMethodUseCase
  UpdatePaymentMethodUseCase
  DeletePaymentMethodUseCase

OpenCashFlow.Application/Payments/DocumentTypes/
  GetDocumentTypesUseCase
  GetDocumentTypeDetailUseCase
  CreateDocumentTypeUseCase
  UpdateDocumentTypeUseCase
  DeleteDocumentTypeUseCase
```

The lookup contracts live in `OpenCashFlow.Application/Payments/Lookups` and the ports live in `OpenCashFlow.Application/Payments/Ports`:

```text
IPaymentMethodReader
IPaymentMethodWriter
IDocumentTypeReader
IDocumentTypeWriter
```

Infrastructure implements these ports with EF readers/writers under `OpenCashFlow.Infrastructure/Payments/Lookups`. These implementations preserve the current lookup behavior:

- list/detail can include global lookup rows where `TenantID == null`;
- create/update/delete are tenant-scoped;
- delete removes unused tenant rows and soft-deletes used rows;
- public API DTOs and routes remain unchanged.

`PaymentService.PaymentMethod.cs` and `PaymentService.DocumentType.cs` now only adapt public DTOs to Application commands/queries and Application results back to public DTOs. They no longer call `IPaymentRepository` or AutoMapper for lookup CRUD.

`IPaymentRepository` has been removed. Payment method/document type lookup CRUD uses dedicated lookup ports, and payment write/daily persistence uses dedicated persistence ports.

## EF Persistence Location

EF persistence is now owned by Infrastructure:

```text
OpenCashFlow.Infrastructure/Persistence/ApplicationDbContext
OpenCashFlow.Infrastructure/Persistence/ApplicationDbContextFactory
OpenCashFlow.Infrastructure/Persistence/Migrations
```

The runtime registration uses the Infrastructure assembly as migrations assembly. The design-time command is:

```bash
dotnet ef migrations list \
  --project src/OpenCashFlow.Infrastructure/OpenCashFlow.Infrastructure.csproj \
  --startup-project src/OpenCashFlow.API/OpenCashFlow.API.csproj
```

`OpenCashFlow.Shared` has been removed from the solution and no longer contains source files.

## Company, Employee And Setup Application Slice

Setup is now modeled as Application use cases:

```text
GetSetupStatusUseCase
CompleteSetupUseCase
ISetupReader
ISetupWriter
```

Infrastructure owns the EF implementation through `SetupReader` and `SetupWriter`. This keeps password hashing, transaction handling, role creation, company creation, first admin creation and initial cash balance persistence outside the API controller.

Company read/invoice flows now use Application use cases and an Infrastructure `CompanyReader`. Employee list/detail/delete/update-profile now use Application use cases and Infrastructure `EmployeeReader`/`EmployeeWriter`.

Residual API logic:

- employee creation/update/PIN now live behind Application use cases and Infrastructure ports;
- authentication forgot/reset-token flows now live behind Application use cases and Infrastructure ports;
- company write/update/delete endpoints are not fully migrated in this slice.

## Auth Registration/Login/FastLogin Slice

Registration, login, fast-login, fast-login cookie generation, account confirmation, resend confirmation and token regeneration now use Application use cases and Infrastructure auth ports.

Decision: JWT issuing, cookie signing, password hashing, PIN verification, EF user persistence and auth audit persistence are infrastructure concerns. Application owns the orchestration and receives only neutral records. API owns public DTO/result mapping and collection of HTTP-bound audit context.

`IEmployeeRepository`, `IAuthenticationRepository` and `ICompanyRepository` were removed from API.

## Auth Audit Port Slice

`AuthenticationService` no longer depends on `ApplicationDbContext` and no longer writes directly to `Admin_AuditLog_DS`.

Auth audit events now flow through:

```text
AuthAuditEvent
IAuthAuditWriter
AuthAuditWriter
```

The API layer maps HTTP context values into `AuthAuditEvent`. Infrastructure persists the event as `Admin_AuditLog`. The helper remains best-effort: audit writer failures are logged as warnings and do not block login, fast-login, password reset or password change flows.

## Next Recommended Step

Continue reducing direct `ApplicationDbContext` use in non-auth API services, starting with `UserManagementService`, `CashService` and role administration.

## Shared Decomposition Decision

`OpenCashFlow.Shared` has been dismantled. The final ownership is documented in `Docs/architecture/shared-decomposition.md`.

`OpenCashFlow.Contracts` owns neutral public API/WebApp contracts. It must not reference EF Core, AutoMapper, MailKit, MimeKit, Infrastructure or API.

Current ownership decision:

- neutral public DTOs and `ApiResponse<T>` live in `OpenCashFlow.Contracts`;
- EF-shaped models live in `OpenCashFlow.Infrastructure/Persistence/Entities`;
- `Domain` and `Application` must remain free from Infrastructure persistence entities;
- SMTP/Slack/security implementations live in Infrastructure;
- AutoMapper profile lives in API mapping because it maps boundary DTOs and persistence-shaped models;
- Billing/Stripe models are legacy persistence compatibility only and must not re-enter core runtime.

Next cleanup order:

1. Move company/employee/setup use cases from API services to Application.
2. Isolate or remove legacy Billing/Stripe schema with an explicit migration strategy.

Current Shared removal status:

- `OpenCashFlow.Contracts` contains neutral API/WebApp contracts.
- `OpenCashFlow.Application` uses Application-owned records and does not reference `OpenCashFlow.Contracts`.
- `OpenCashFlow.Contracts` uses `OpenCashFlow.Contracts.*` namespaces.
- AutoMapper mapping moved to `OpenCashFlow.API/Mapping`.
- Email and Slack ports moved to `OpenCashFlow.Application/Abstractions`.
- SMTP/Slack implementations and options moved to `OpenCashFlow.Infrastructure`.
- Password hashing, cookie signing and secret generation moved to `OpenCashFlow.Infrastructure/Auth`.
- EF entities moved to `OpenCashFlow.Infrastructure/Persistence/Entities`.
- `OpenCashFlow.Shared` is no longer part of the solution.

## Fase 3J: Non-Auth API Services Are Persistence-Free

Decision: API services are boundary adapters, not persistence owners. User management, roles, cash, audit-log queries and health checks now depend on Application use cases/ports. Infrastructure owns EF Core access and maps EF entities to Application-owned records.

This keeps `OpenCashFlow.Application` free from `OpenCashFlow.Contracts`, EF Core, ASP.NET and Infrastructure while preserving the public JSON contracts in API/WebApp. Remaining `Infrastructure.Persistence.Entities` imports in controllers/interfaces are DTO/entity boundary residue and should be removed in later contract-cleanup slices, but they no longer represent direct `ApplicationDbContext` usage in API services.

## Fase 3K: EF Entities Do Not Cross The API Boundary

Decision: EF-shaped persistence entities must not appear in API controller/service-interface signatures. Fase 3K moved the last daily payment report shape to `OpenCashFlow.Contracts` and removed stale Infrastructure entity imports from the API boundary.

Tests may still use Infrastructure entities for database/integration setup. Infrastructure migrations and EF repositories remain the only runtime owners of persistence entities.
