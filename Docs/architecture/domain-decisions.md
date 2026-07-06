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

Current payment, company and cash models live in `OpenCashFlow.Shared` and are EF-shaped:

- They carry persistence attributes and navigation properties.
- They are used by the current `ApplicationDbContext`.
- They are tied to the current migrations and database schema.
- Some models still contain legacy SaaS/Billing fields kept for migration compatibility.

Moving those models directly into `OpenCashFlow.Domain` would drag EF Core and schema history into the pure domain layer. Instead, the first step creates small domain primitives that represent stable business concepts without persistence concerns.

## Temporary Shared Types

These remain in `OpenCashFlow.Shared` for now:

- EF Core models under `Shared.Models`.
- `ApplicationDbContext` and migrations under `Shared.Data`.
- DTOs under `Shared.DTOs`.
- AutoMapper profile under `Shared.Mappings`.
- Email/Slack service implementations under `Shared.Services`.
- Legacy SaaS/Billing schema models such as `Plan`, `Company_Subscription`, `Company_Renewal` and `Stripe_Webhook_Event`.

This is intentional. The runtime still uses the existing EF models and DTOs while Domain grows safely in parallel.

## Mapping Strategy For Later Phases

The next extraction phases should map between persistence models and domain primitives at the Application/Infrastructure boundary.

Recommended direction:

```text
Shared EF model / Infrastructure persistence model
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

- Runtime services still use the old `Shared` models.
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

`IPaymentRepository` now lives in `OpenCashFlow.Application/Payments/Repositories` and the EF implementation lives in `OpenCashFlow.Infrastructure/Payments`. The repository contract still uses `OpenCashFlow.Shared` DTOs and EF-shaped models temporarily because API/WebApp contracts and `PaymentService` have not yet been split from those types.

Infrastructure adapters under `OpenCashFlow.Infrastructure/ApplicationAdapters` now delegate persistence to dedicated Infrastructure repositories:

```text
OpenCashFlow.Infrastructure/Payments/PaymentRepository
OpenCashFlow.Infrastructure/Cash/CashLedgerRepository
OpenCashFlow.Infrastructure/Audit/AuditRepository
OpenCashFlow.Infrastructure/Persistence/EfUnitOfWork
```

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

`IPaymentRepository` has been reduced and no longer contains payment method/document type CRUD. It still references `OpenCashFlow.Shared` EF-shaped payment and daily-payment models for the write/daily flows that have not yet been split into dedicated persistence ports.

## Next Recommended Step

Replace the remaining broad `IPaymentRepository` with smaller Application ports for payment write/read-for-update and daily-payment persistence:

```text
IPaymentPersistenceWriter
IPaymentPersistenceReader
IDailyPaymentRepository
```

After that, move EF models, `ApplicationDbContext` and migrations from `Shared` to `Infrastructure/Persistence` in a dedicated migration phase.
