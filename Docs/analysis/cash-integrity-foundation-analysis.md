# Cash Integrity Foundation Analysis

Branch: `analysis/cash-integrity-foundation`

Date: 2026-07-08

## Purpose

OpenCashFlow is positioning itself as a cash cockpit. That positioning is not credible unless the system can prove the cash facts behind every cash decision.

The minimum proof chain is:

- who received or took money;
- how much money moved;
- why it moved;
- who entered it;
- who authorized it, if authorization is required;
- which cash source/account was affected;
- what the expected cash balance should be;
- whether counted or bank-reported cash matches the expected balance;
- where any difference comes from.

This analysis intentionally does not implement production code. It evaluates the current model and defines the foundation required before forecast and Safe-to-Pay features can be treated as reliable.

## Current State

### Payment Flow

Payment create/update/delete is now orchestrated in `OpenCashFlow.Application`.

Observed behavior:

- `CreatePaymentOrchestrator` validates payment input, checks idempotency by `RequestId`, validates payment method/document type, creates the payment, updates daily payment aggregation, applies cash ledger entries for cash-like methods, and writes payment audit.
- `UpdatePaymentOrchestrator` updates payment state, reverses/reapplies daily aggregation, updates/voids/reapplies cash ledger effects when the payment method or amount changes, and writes payment audit.
- `DeletePaymentOrchestrator` soft-deletes the payment, removes daily aggregation effect, voids cash ledger effect for cash-like methods, and writes payment audit.
- Cash-like detection is based on a system payment method id (`00000000-0000-0000-0000-000000000002`) or payment method names `Cash` / `Contanti`.
- Payment persistence stores `TenantID`, `Amount`, `EntryType`, `PaymentMethodID`, `DocumentTypeID`, `Description`, `UserID`, `CreatedBy`, `EditedBy`, soft-delete fields and timestamps.

What this gives today:

- a payment can identify tenant, amount, direction, method, document type, description and user;
- cash-like payments produce ledger deltas;
- create/update/delete are transaction-scoped through `IUnitOfWork`;
- audit exists for payment create/update/delete.

What it does not give today:

- a distinct employee/cash handler separate from the actor who entered the payment;
- an approver;
- an explicit cash source/account;
- a posted/immutable cash movement concept;
- a daily cash session;
- actual cash count;
- reconciliation status;
- discrepancy explanation;
- reversal-first correction policy.

### Cash Ledger

Current persistence entities:

- `CashBalance`
  - key: `CompanyId`;
  - fields: `Balance`, `LastUpdatedUtc`, `RowVersion`;
  - represents one current balance per company.

- `CashLedger`
  - fields: `CompanyId`, `RefType`, `RefId`, `OriginalPaymentId`, `Delta`, `Reason`, `CreatedBy`, `CreatedAtUtc`;
  - stores ledger deltas for payment events, voids, reapplications, refunds and manual adjustments.

Current infrastructure behavior:

- cash balance is incrementally updated when a ledger delta is inserted;
- duplicate ledger effects are suppressed by `(CompanyId, RefType, RefId)` checks;
- cash balance rebuild recalculates `CashBalance.Balance` from sum of `CashLedgers.Delta`;
- `CashWriter` uses a local transaction when there is no ambient EF transaction;
- `CashWriter` retries once on `DbUpdateConcurrencyException`;
- `AdminAdjustAsync` requires a non-empty reason;
- payment-driven ledger entries usually do not store a reason.

What this gives today:

- a deterministic company-level running cash balance;
- a ledger of deltas from payment and adjustment events;
- basic idempotence for ref-based ledger writes;
- a repair path through rebuild.

What it does not give today:

- multiple cash accounts/sources;
- opening balance by day/source;
- closing balance by day/source;
- counted actual balance;
- reconciliation discrepancy;
- required reason for every movement;
- employee receiver/taker;
- approval metadata;
- immutable posting state;
- explicit reversal/correction entries;
- durable link from ledger entry to business reason except for manual adjustments.

### Daily Payment Aggregation

Current `Payment_DailyPayments` stores:

- `TenantID`;
- `CashDate`;
- `Total`.

It is updated from payment create/update/delete. This is a payment reporting aggregate, not a cash integrity or reconciliation model.

Limitations:

- uses `double` while the ledger uses `decimal`;
- represents daily net payment total, not expected cash by source;
- does not store opening balance, closing count, discrepancy or close status;
- cannot explain differences.

### Cash Admin API And WebApp

Current API endpoint area:

- `GET /v1/admin/cash/current`;
- `GET /v1/admin/cash/ledger`;
- `POST /v1/admin/cash/adjust`;
- `POST /v1/admin/cash/rebuild`;
- `GET /v1/admin/cash/ledger/export`.

Authorization:

- `CashAdminController` is restricted to `CompanyAdmin,InstanceAdmin`;
- non-instance users are constrained to the `TenantID` claim;
- `InstanceAdmin` must provide `companyId`.

Current WebApp view:

- displays current cash balance;
- lists ledger rows;
- allows administrators to add a positive/negative adjustment with reason;
- attempts a non-blocking user-id to employee-name lookup for display.

What is missing in the UI:

- employee cash movement entry;
- daily opening balance;
- daily closing count;
- reconciliation screen;
- discrepancy workflow;
- approval workflow;
- movement reversal/correction screen;
- per-source cash view.

### Audit

Current audit model:

- `Admin_AuditLog` stores generic event type, resource, resource id, action, user id, username, changes JSON, IP/user-agent, timestamp, severity, additional info and tenant id.

Current payment audit:

- payment created;
- payment updated;
- payment deleted.

Current cash admin audit:

- manual adjustment;
- rebuild;
- export.

Limitations:

- cash ledger entries are not audited as first-class cash movement events;
- audit does not encode cash source, receiver/taker, approver or reconciliation context;
- audit is not yet the canonical integrity trail for cash lifecycle events.

## Core Scenario Evaluation

Scenario:

- Employee X receives 200 EUR for `rinnovo K`.
- Employee Z receives 100 EUR for `acquisto F`.
- Expected end-of-day cash movement is 300 EUR.
- Actual cash count is compared with expected cash.

Current system can partially prove:

- two cash-like payments can be recorded with amounts 200 and 100;
- each payment can store a description such as `rinnovo K` or `acquisto F`;
- each payment has a tenant and `UserID`;
- cash-like payment creation can write cash ledger deltas;
- the company cash balance can become 300 if both are inflows and no other movements exist;
- payment create audit can be written.

Current system cannot fully prove:

- that Employee X physically received exactly 200;
- that Employee Z physically received exactly 100;
- whether `UserID` means receiver, operator, creator or owner of the payment;
- which cash box, bank account, till or source was affected;
- who approved the cash movement;
- that the movement is immutable after posting;
- opening cash for the day;
- expected cash for the day by source;
- counted actual cash;
- balanced vs discrepant reconciliation;
- why a discrepancy exists;
- whether discrepancy remains open or has been resolved.

Verdict: the current system is a useful payment/cash-ledger foundation, but it cannot yet prove the full cash integrity scenario.

## Gap Analysis

### Already Implemented

- Tenant-scoped payments.
- Payment create/update/delete use cases in Application.
- Transaction boundary through `IUnitOfWork` for payment create/update/delete.
- Cash-like payment ledger effect.
- Company-level current cash balance.
- Ledger append for payment/void/reapply/update/adjustment.
- Cash balance rebuild from ledger deltas.
- Admin cash adjustment with required reason.
- Cash admin API authorization for `CompanyAdmin` and `InstanceAdmin`.
- Tenant claim isolation in cash admin endpoints.
- Generic audit log.
- Payment create/update/delete audit.
- Cash adjustment/rebuild/export audit at API level.
- Tests for payment orchestrator cash ledger application/void/update paths.
- Domain tests for simple cash balance arithmetic.

### Partially Implemented

- Actor tracking: `UserID`, `CreatedBy` and ledger `CreatedBy` exist, but semantics are ambiguous.
- Reason tracking: payment description and adjustment reason exist, but reason is not mandatory for every cash movement.
- Ledger determinism: deltas are deterministic for payment flows, but there is no explicit posting/correction policy.
- Daily totals: `Payment_DailyPayments` exists, but it is not a reconciliation or cash-day model.
- Audit trail: payment and cash admin actions are audited, but cash movements are not modeled as auditable lifecycle events.
- Authorization: admin cash endpoints are role-protected, but there is no domain-level permission model for cashier vs reconciler vs approver.

### Missing

- `CashAccount` / cash source.
- `CashMovement` aggregate.
- `CashSession` or `CashDay`.
- Opening cash balance per day/source.
- Actual counted balance.
- End-of-day reconciliation.
- Reconciliation discrepancy.
- Discrepancy explanation workflow.
- Approval metadata.
- Cash receiver/taker field distinct from creator/actor.
- Movement status (`Draft`, `Posted`, `Reversed`, `Corrected`).
- Reversal/correction model.
- Cash movement audit as first-class use case.
- API contracts for movement entry, expected balance, close day, reconcile, explain discrepancy, reverse/correct.
- WebApp screens for the daily cash workflow.
- Heavy tenant/authorization/reconciliation tests.

### Risky Or Ambiguous

- Payment uses `double` in persistence while Application records and cash ledger use `decimal`.
- Cash-like method detection depends on fixed id/name aliases rather than a durable method classification.
- `CashLedger.RefType` is a string with implicit values.
- `CashLedger.Reason` is nullable.
- `CashLedger.CreatedBy` is a string rather than a typed user reference.
- A payment can be updated or soft-deleted after creating cash effects; ledger compensates, but the product has not defined whether posted cash movement editing should be allowed.
- `Payment_DailyPayments.Total` can drift semantically from actual cash because it is a reporting aggregate, not a reconciliation source.
- Cash balance is company-wide, which is insufficient for multiple cash boxes, registers, banks or employee-held cash.
- Manual adjustment can explain a delta but does not create a structured discrepancy lifecycle.

### Overcomplicated

- Existing payment update/delete cash reapplication logic is necessary for current behavior, but it is compensating for the absence of a first-class immutable cash movement model.
- Daily payment aggregation and cash ledger are separate but similarly named concepts; this can confuse future forecast/reconciliation work.
- Cash admin UI mixes operational adjustment and ledger inspection, but not daily workflow.

### Needs Product Decision

- Is `Payment.UserID` the employee who received/took money, the creator, or the payment owner?
- Must every cash movement be approved?
- Which roles can create, approve, reverse and reconcile?
- Can cash be negative?
- Should posted movements be immutable?
- Are corrections always reversal-based?
- How many cash accounts/sources are needed in MVP: one default cash box, multiple cash boxes, bank accounts, employee wallets?
- Does bank account reconciliation belong in MVP, or only physical cash?
- Should cash ledger include bank-like movements, or should bank reconciliation be separate?
- Is end-of-day reconciliation mandatory before forecasting is considered reliable?

## Cash Integrity Invariants

These invariants should become explicit product and domain rules.

1. Every cash movement is tenant-scoped.
2. Every cash movement affects exactly one cash account/source.
3. Every cash movement has a signed monetary delta using decimal precision.
4. Every cash movement has a direction or type derived from the delta.
5. Every cash movement has a required business reason/category.
6. Every cash movement identifies the actor who entered it.
7. Every cash movement identifies the employee/cash handler who received or took the money when that differs from the actor.
8. Every posted movement is immutable.
9. Changes to posted movements happen through reversal/correction entries.
10. Every reversal references the original movement.
11. Every correction preserves the original movement, the reversing movement and the corrected movement.
12. Every movement has a timestamp generated by the system.
13. Every movement has an audit event.
14. Every reconciliation has an opening expected balance.
15. Every reconciliation has a calculated expected balance.
16. Every reconciliation records actual counted balance or imported bank balance.
17. Every discrepancy remains visible until explained or resolved.
18. Discrepancy explanations are audit logged.
19. A daily cash session cannot be silently overwritten.
20. Tenant isolation must be enforced in read, write, correction and reconciliation paths.
21. Unauthorized roles cannot create, reverse, approve or reconcile movements.
22. Forecast and Safe-to-Pay must not treat unreconciled cash as high-confidence cash.

## Minimum Product Workflow

### Morning

1. A company admin or authorized cashier opens the daily cash session.
2. The system shows yesterday's closing balance as today's opening expected balance.
3. If no previous closing exists, the user records an opening cash count with reason.
4. The opening balance is locked for the session.

### During The Day

1. Employee receives or takes money.
2. User records:
   - employee/cash handler;
   - amount;
   - direction;
   - reason/category;
   - optional payment/document reference;
   - cash account/source.
3. System validates authorization and tenant.
4. Movement is posted.
5. Ledger entry is appended.
6. Expected cash balance updates.
7. Audit event is written.

### Evening

1. Authorized user starts reconciliation for the cash account/source.
2. System calculates expected balance:
   - opening balance;
   - plus posted inflows;
   - minus posted outflows;
   - plus/minus corrections.
3. User enters actual counted cash or imported bank balance.
4. System calculates discrepancy.
5. If discrepancy is zero, session can close as balanced.
6. If discrepancy is non-zero, discrepancy remains open until explained.
7. Explanation is required before marking as explained/resolved.
8. Close/reconcile action is audit logged.

## Proposed Model

### CashAccount

Represents a source of cash/bank value.

Suggested fields:

- `CashAccountId`;
- `TenantId`;
- `Name`;
- `Type` (`PhysicalCash`, `Bank`, `EmployeeFloat`, `Other`);
- `Currency`;
- `IsDefault`;
- `IsActive`;
- `CreatedBy`;
- `CreatedAtUtc`;
- `ClosedAtUtc`.

MVP can start with one default physical cash account per company, but the model should not block multiple accounts later.

### CashMovement

Canonical business movement.

Suggested fields:

- `CashMovementId`;
- `TenantId`;
- `CashAccountId`;
- `Amount`;
- `Direction`;
- `ReasonCode`;
- `ReasonText`;
- `EmployeeId` / `CashHandlerUserId`;
- `EnteredByUserId`;
- `ApprovedByUserId`;
- `PaymentId`;
- `DocumentTypeId`;
- `OccurredAtUtc`;
- `PostedAtUtc`;
- `Status` (`Draft`, `Posted`, `Reversed`, `Corrected`);
- `OriginalMovementId`;
- `CorrectionGroupId`;
- `IdempotencyKey`.

### CashLedgerEntry

Append-only accounting effect of a posted movement.

Suggested fields:

- `CashLedgerEntryId`;
- `TenantId`;
- `CashAccountId`;
- `CashMovementId`;
- `Delta`;
- `BalanceAfter`;
- `CreatedAtUtc`.

`CashLedger` should stop being the only cash business model. It should become the append-only effect log of posted movements.

### CashSession / CashDay

Represents a daily operating period per cash account.

Suggested fields:

- `CashSessionId`;
- `TenantId`;
- `CashAccountId`;
- `BusinessDate`;
- `OpeningExpectedBalance`;
- `OpeningActualBalance`;
- `OpenedByUserId`;
- `OpenedAtUtc`;
- `ClosedByUserId`;
- `ClosedAtUtc`;
- `Status` (`Open`, `Reconciled`, `ClosedWithDiscrepancy`).

### Reconciliation

Represents expected vs actual comparison.

Suggested fields:

- `ReconciliationId`;
- `TenantId`;
- `CashSessionId`;
- `CashAccountId`;
- `ExpectedBalance`;
- `ActualBalance`;
- `Discrepancy`;
- `Status` (`Balanced`, `Discrepant`, `Explained`, `Resolved`);
- `ReconciledByUserId`;
- `ReconciledAtUtc`.

### ReconciliationDiscrepancy

Structured discrepancy explanation.

Suggested fields:

- `DiscrepancyId`;
- `ReconciliationId`;
- `Amount`;
- `Explanation`;
- `Category`;
- `CreatedByUserId`;
- `CreatedAtUtc`;
- `ResolvedByUserId`;
- `ResolvedAtUtc`.

### AuditEvent

Current `Admin_AuditLog` can be reused initially, but cash integrity should introduce semantic audit writes for:

- movement created;
- movement posted;
- movement reversed;
- movement corrected;
- session opened;
- session reconciled;
- discrepancy recorded;
- discrepancy resolved;
- export.

## Proposed Application Use Cases

Cash movement:

- `CreateCashMovementUseCase`;
- `PostCashMovementUseCase`;
- `ReverseCashMovementUseCase`;
- `CorrectCashMovementUseCase`;
- `GetCashMovementDetailUseCase`;
- `GetCashMovementsByDayUseCase`.

Cash account:

- `GetCashAccountsUseCase`;
- `CreateCashAccountUseCase`;
- `DeactivateCashAccountUseCase`.

Cash session:

- `OpenCashSessionUseCase`;
- `GetCashSessionUseCase`;
- `GetDailyExpectedCashUseCase`;
- `CloseCashSessionUseCase`.

Reconciliation:

- `StartCashReconciliationUseCase`;
- `RecordActualCashCountUseCase`;
- `ExplainCashDiscrepancyUseCase`;
- `ResolveCashDiscrepancyUseCase`;
- `GetCashReconciliationStatusUseCase`.

Audit:

- `ICashAuditWriter`;
- cash event records in Application.

## Proposed API Contracts

Initial API should be explicit and deterministic.

### Cash Accounts

- `GET /v1/cash/accounts`
- `POST /v1/cash/accounts`
- `PATCH /v1/cash/accounts/{cashAccountId}/deactivate`

### Cash Movements

- `POST /v1/cash/movements`
- `GET /v1/cash/movements?date=&cashAccountId=&employeeId=`
- `GET /v1/cash/movements/{cashMovementId}`
- `POST /v1/cash/movements/{cashMovementId}/reverse`
- `POST /v1/cash/movements/{cashMovementId}/correct`

### Daily Cash

- `POST /v1/cash/sessions/open`
- `GET /v1/cash/sessions/current`
- `GET /v1/cash/sessions/by-date?date=&cashAccountId=`
- `GET /v1/cash/sessions/{cashSessionId}/expected-balance`
- `POST /v1/cash/sessions/{cashSessionId}/close`

### Reconciliation

- `POST /v1/cash/sessions/{cashSessionId}/reconcile`
- `GET /v1/cash/sessions/{cashSessionId}/reconciliation`
- `POST /v1/cash/reconciliations/{reconciliationId}/discrepancies`
- `PATCH /v1/cash/reconciliations/{reconciliationId}/discrepancies/{discrepancyId}/resolve`

### Audit

- `GET /v1/cash/movements/{cashMovementId}/audit`
- `GET /v1/cash/sessions/{cashSessionId}/audit`

## Proposed WebApp Screens

### Employee Cash Movement Entry

Purpose: quick entry during operations.

Fields:

- employee/cash handler;
- cash account/source;
- amount;
- direction;
- reason/category;
- free-text note;
- optional payment/document reference.

### Daily Cash Movements

Purpose: review all movements for a day.

Shows:

- opening expected balance;
- movement list;
- employee summaries;
- cash account/source filters;
- current expected balance.

### End-Of-Day Reconciliation

Purpose: close the day.

Shows:

- expected cash;
- actual counted cash input;
- calculated discrepancy;
- close as balanced or record discrepancy.

### Discrepancy View

Purpose: keep differences visible.

Shows:

- discrepancy amount;
- source/day;
- linked movements;
- explanation status;
- explanation history;
- resolution status.

### Employee Money Received Summary

Purpose: prove employee-level handling.

Shows:

- employee;
- amounts received/taken by day;
- reasons;
- linked cash movements;
- reconciled/unreconciled status.

### Audit/History View

Purpose: prove lifecycle.

Shows:

- movement creation/posting/reversal/correction;
- session open/close;
- reconciliation events;
- discrepancy explanation/resolution.

## Test Plan

### Domain Unit Tests

- Cash movement requires tenant.
- Cash movement requires cash account.
- Cash movement requires amount not zero.
- Cash movement requires reason/category.
- Cash movement requires actor.
- Cash movement requires cash handler when movement type needs it.
- Posted movement cannot be edited.
- Reversal references original movement.
- Correction preserves original and creates reversal/new movement.
- Expected balance equals opening balance plus posted deltas.
- Discrepancy equals actual minus expected.
- Balanced reconciliation requires discrepancy zero.
- Discrepant reconciliation cannot be hidden without explanation.

### Application Use Case Tests

- Create cash movement validates tenant, account, user, employee and reason.
- Post movement appends ledger and writes audit in transaction.
- Reverse movement creates reversal and audit.
- Correct movement creates reversal and replacement movement.
- Open session derives opening balance from prior close.
- Reconcile session calculates expected balance.
- Reconcile balanced closes as balanced.
- Reconcile discrepant leaves discrepancy open.
- Explain discrepancy records explanation and audit.
- Unauthorized role cannot create/reconcile/reverse.
- Cross-tenant access is rejected.

### API Tests

- `POST /cash/movements` returns 201 for valid movement.
- Missing amount/reason/account returns 400.
- Cross-tenant account returns 403/404 consistently.
- Unauthorized role returns 403.
- Posted movement update endpoint does not exist or returns deterministic rejection.
- Reversal endpoint creates reversal, not physical delete.
- Reconciliation balanced returns expected status.
- Reconciliation discrepant returns discrepancy detail.
- Discrepancy explanation is visible on subsequent reads.

### Database Integration Tests

- Movement requires existing tenant/company.
- Movement requires existing cash account.
- Movement requires existing actor/user.
- Movement requires existing employee/cash handler if provided.
- Ledger entry requires movement.
- Reconciliation requires session.
- Discrepancy requires reconciliation.
- Unique session per tenant/account/business date.
- Reversal references original movement.
- Cash ledger balance after concurrent movement posting remains deterministic.

### Tenant Isolation Tests

- Tenant A cannot list Tenant B cash accounts.
- Tenant A cannot create movement in Tenant B account.
- Tenant A cannot reverse Tenant B movement.
- Tenant A cannot reconcile Tenant B session.
- Tenant A cannot read Tenant B audit trail.

### Authorization Tests

- Employee/cashier can create movement only if product allows it.
- Employee/cashier cannot reconcile.
- CompanyAdmin can reconcile.
- InstanceAdmin requires explicit company context.
- Unauthorized roles cannot export cash ledger.

### Immutability And Reversal Tests

- Posted movement edit is rejected.
- Posted movement delete is rejected.
- Correction creates reversal and replacement.
- Audit includes original/reversal/replacement ids.
- Original movement remains visible.

### End-Of-Day Reconciliation Tests

- Session opening from prior close.
- Expected balance from opening + movements.
- Actual count zero with expected non-zero creates discrepancy.
- Balanced close prevents further movement unless next session opens or explicit late adjustment policy is used.

### Audit Trail Tests

- Movement created audit.
- Movement posted audit.
- Movement reversed audit.
- Movement corrected audit.
- Reconciliation audit.
- Discrepancy explanation audit.
- Export audit.

## Concrete Test Scenarios

### Scenario A - Balanced Day

Given:

- opening cash: 0;
- employee X receives 200 for `rinnovo K`;
- employee Z receives 100 for `acquisto F`.

Expected:

- two posted movements exist;
- X is linked to the 200 movement;
- Z is linked to the 100 movement;
- reasons are stored;
- expected cash is 300;
- actual cash count is 300;
- reconciliation status is `Balanced`;
- audit trail includes movement and reconciliation events.

### Scenario B - Discrepancy

Given:

- same movements as Scenario A;
- actual cash count is 250.

Expected:

- expected cash is 300;
- actual cash is 250;
- discrepancy is -50;
- reconciliation status is `Discrepant`;
- discrepancy remains visible until explained;
- forecast/Safe-to-Pay confidence is degraded.

### Scenario C - Posted Movement Edit

Given:

- a posted movement exists.

Expected:

- direct edit is rejected;
- or system creates reversal/correction entries;
- original movement remains visible.

### Scenario D - Cross-Tenant Access

Given:

- Tenant A employee attempts to see or alter Tenant B movements.

Expected:

- read/write/reverse/reconcile actions are rejected;
- no data leaks in response shape.

### Scenario E - Unauthorized Role

Given:

- user without cash permission attempts movement creation or reconciliation.

Expected:

- action returns 403;
- no ledger entry is written;
- optional security audit is written.

### Scenario F - Delete Posted Movement

Given:

- posted movement exists.

Expected:

- physical delete is forbidden;
- soft delete is also forbidden for posted cash effect;
- reversal is the only correction path.

### Scenario G - Correction Audit

Given:

- movement 200 was posted but should be 180.

Expected:

- original 200 remains;
- reversal -200 is created;
- corrected +180 is created;
- expected balance changes by -20;
- audit links all three records.

## Forecast And Safe-To-Pay Status

Forecast and Safe-to-Pay should remain WIP/Experimental until Cash Integrity is implemented.

Reason:

- current cash balance can be calculated, but not reconciled;
- expected cash cannot be tied to actual counted/bank balance;
- discrepancies cannot be tracked or explained;
- cash confidence cannot be scored;
- user/employee/cash source semantics are ambiguous.

Safe-to-Pay can be prototyped as a product concept, but it should not present cash recommendations as reliable until:

1. cash movement model exists;
2. daily reconciliation exists;
3. discrepancy status feeds forecast confidence;
4. tenant/authorization/immutability tests are green.

## Release Blockers For Cash Integrity

These are blockers before cash forecasting can be marketed as trustworthy:

1. No cash account/source model.
2. No cash movement aggregate.
3. No explicit employee cash handler field.
4. No immutable posted movement policy.
5. No reversal/correction model.
6. No daily cash session.
7. No actual-vs-expected reconciliation.
8. No discrepancy workflow.
9. No cash movement audit lifecycle.
10. No heavy tenant/authorization/reconciliation test suite.

## Recommended Implementation PR Sequence

### PR 1 - Cash Integrity Product Decision Record

Document decisions:

- cash account scope for MVP;
- movement roles;
- authorization roles;
- immutable/reversal policy;
- reconciliation requirements.

No schema changes.

### PR 2 - Domain Model And Application Contracts

Add neutral domain/application records:

- `CashAccount`;
- `CashMovement`;
- `CashSession`;
- `CashReconciliation`;
- `CashDiscrepancy`;
- reversal/correction concepts.

Add domain tests for invariants.

### PR 3 - Persistence Schema And Repositories

Add EF entities and migrations for:

- cash accounts;
- cash movements;
- sessions;
- reconciliations;
- discrepancies.

Add database integration tests.

### PR 4 - Movement Use Cases

Implement:

- create/post movement;
- list by day;
- reverse/correct movement;
- audit writer.

Add application and API tests.

### PR 5 - Daily Session And Reconciliation Use Cases

Implement:

- open session;
- expected balance;
- reconcile actual count;
- discrepancy explanation/resolution.

Add application/API/database tests.

### PR 6 - WebApp Daily Cash Workflow

Add screens:

- movement entry;
- daily movement list;
- end-of-day reconciliation;
- discrepancy view.

Keep current cash ledger admin page as an admin/diagnostic view until replaced.

### PR 7 - Forecast Confidence Gate

Connect reconciliation status to future forecast/Safe-to-Pay confidence:

- unreconciled cash lowers confidence;
- open discrepancy creates warning;
- balanced sessions can be used as high-confidence actuals.

## Final Assessment

The current system can partially support the sample scenario as payments and ledger entries, but it cannot prove the scenario at cash-integrity level.

It can show that:

- two cash-like payments were entered;
- their amounts affected company cash balance;
- payment audit exists.

It cannot prove that:

- specific employees physically received the money;
- a specific cash source was affected;
- the day opened and closed correctly;
- actual cash matched expected cash;
- discrepancies were explained.

Therefore Cash Integrity should be treated as a foundational product track, and forecast/Safe-to-Pay features should remain WIP/Experimental until the cash movement, reconciliation and audit model is complete.
