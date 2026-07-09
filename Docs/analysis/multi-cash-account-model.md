# Multi-Cash-Account Model Analysis

Branch: `analysis/multi-cash-account-model`

Date: 2026-07-08

## Scope

This is an analysis and design document only. It does not implement schema, migrations, API endpoints, WebApp screens, or production code.

Important local context:

- `development` was pulled and was up to date with `origin/development`.
- The requested prerequisite commit from `feature/cash-integrity-domain-contracts` was not an ancestor of local `development` at the time of this analysis.
- The current Cash Integrity domain/application contracts were inspected from `feature/cash-integrity-domain-contracts` as the intended next baseline.

## Goal

OpenCashFlow must support more than one cash source per company.

Examples:

- main office cash box;
- workshop cash box;
- employee float;
- bank account;
- POS/transit account;
- petty cash;
- cash temporarily held by an employee.

The product must prove:

- where money came from;
- where money went;
- who handled it;
- which cash account/source changed;
- expected balance per cash account;
- actual counted balance per cash account;
- discrepancies per cash account;
- transfers between cash accounts.

## Current State

### Production State On `development`

The current runtime model is still company-level:

- `CashBalance`
  - key: `CompanyId`;
  - fields: `Balance`, `LastUpdatedUtc`, `RowVersion`;
  - one cash balance per company.

- `CashLedger`
  - fields: `CompanyId`, `RefType`, `RefId`, `OriginalPaymentId`, `Delta`, `Reason`, `CreatedBy`, `CreatedAtUtc`;
  - no `CashAccountId`;
  - no transfer group;
  - no session or reconciliation link.

`CashWriter` applies deltas to the company balance and inserts ledger entries. `CashReader` reads the company balance and company ledger. Current payment cash handling updates this single company-level balance.

### Intended Cash Integrity Contracts

The domain/application contract branch introduces:

- `CashAccount`;
- `CashMovement`;
- `CashSession`;
- `CashReconciliation`;
- `CashDiscrepancy`;
- `CashAccountType`;
- `CashMovementDirection`;
- `CashMovementStatus`;
- `CashSessionStatus`;
- `CashReconciliationStatus`;
- Application commands/results/ports under `Application/Cash/Integrity`.

These contracts already point in the right direction:

- `CashAccount` has `TenantId`, `Name`, `Type`, `Currency`, `IsDefault`, `IsActive`;
- `CashMovement` affects exactly one `CashAccountId`;
- `CashSession` is per `CashAccountId` and business date;
- `CashReconciliation` is per session/account;
- `CashDiscrepancy` is linked to a reconciliation.

The main missing concept is transfer: a business object that coordinates two opposite movements across two cash accounts.

## Decisions

## 1. Can One Company Have Many Cash Accounts?

Decision: yes.

One tenant/company can have many Cash Accounts.

Rationale:

A real small business may hold money in several operational places. A company-level balance hides operational risk. Safe-to-Pay needs source-level visibility because cash in a bank account, physical cash box, POS transit account, and employee float do not have the same availability or confidence.

## 2. Is There Exactly One Default Cash Account?

Decision: yes, exactly one active default account per company and currency.

MVP simplification:

- one company currency, initially `EUR`;
- therefore one active default account per company.

Later multi-currency:

- one active default account per company/currency.

Enforcement:

- domain can express `IsDefault`;
- application/infrastructure must enforce uniqueness because it requires querying existing accounts;
- database should add a filtered unique index for active default account per tenant/currency when schema is implemented.

## 3. Which Cash Account Types Are Supported?

Decision:

Supported product types:

- `PhysicalCash`;
- `Bank`;
- `EmployeeFloat`;
- `POS/transit`;
- `PettyCash`;
- `Other`.

Current contract adjustment needed:

- `CashAccountType` currently includes `PhysicalCash`, `Bank`, `EmployeeFloat`, `Other`;
- add `PosTransit` and `PettyCash` before persistence implementation, or map both to `Other` in MVP and document the limitation.

Recommendation:

Add explicit `PosTransit` and `PettyCash` enum values before schema generation. They are not ERP drift; they are cash source semantics.

## 4. Can Cash Accounts Be Deactivated?

Decision: yes.

Deactivation means:

- account no longer accepts ordinary new movements;
- account remains visible historically;
- account remains available in reports, audit, reconciliations and old movements;
- account may still receive system correction/reversal movements needed to preserve integrity.

## 5. Can Cash Accounts Be Deleted?

Decision: no physical delete after creation if any movement/session/reconciliation exists.

MVP behavior:

- account with no history may be deleted only if product needs it, but deletion is not required for MVP;
- account with history can only be deactivated.

Recommendation:

Avoid delete endpoints in the first implementation. Use deactivate only.

## 6. Can Cash Accounts Have Opening Balances?

Decision: yes, but opening balance belongs to the first Cash Session or initialization movement, not only to the account row.

Recommended model:

- `CashAccount` stores identity/configuration;
- opening cash for a day is recorded in `CashSession`;
- initial migration/import creates an opening session or initialization movement for the default account.

Rationale:

Opening balances are historical facts. If stored only as mutable account properties, they are hard to audit.

## 7. Can Cash Accounts Have Currencies?

Decision: yes, Cash Account has currency.

MVP:

- enforce single tenant/company currency, likely `EUR`;
- transfers are allowed only between accounts with the same currency.

Excluded from MVP:

- FX conversion;
- multi-currency Safe-to-Pay;
- exchange gains/losses;
- transfer with currency conversion.

## 8. Can Cash Accounts Be Reconciled Independently?

Decision: yes.

Reconciliation is per:

- tenant;
- cash account;
- business date/session.

A discrepancy in `Main Cash Box` must not make `Workshop Cash Box` discrepant.

## 9. Can A Daily Session Exist Per Cash Account?

Decision: yes.

There can be one active/open session per tenant, cash account and business date.

This allows:

- main cash box reconciled daily;
- employee float reconciled when returned;
- bank/POS source reconciled when imported or settled;
- independent discrepancy lifecycle.

## 10. Can One User Access Only Specific Cash Accounts?

Decision:

Not in MVP, but the model should not block it.

MVP:

- tenant-level role permissions decide access;
- `CompanyAdmin` / `InstanceAdmin` can manage/reconcile;
- authorized cash operators can enter movements if introduced.

Later:

- per-account access policy for employee floats, branch cash boxes or sensitive bank accounts.

## Transfer Model

Transfers are critical because they move value between cash sources without changing company-level cash.

Examples:

- move 300 EUR from main cash box to employee float;
- move 1,000 EUR from physical cash to bank deposit;
- move 200 EUR from employee float back to main cash box.

## Transfer Decision

Decision:

A transfer is one business object that creates two linked cash movements.

Implementation concept:

- `CashTransfer`
  - `CashTransferId`;
  - tenant;
  - source cash account;
  - destination cash account;
  - amount;
  - currency;
  - reason/category;
  - entered by;
  - employee cash handler, when physical custody is involved;
  - occurred/posting timestamps;
  - status;
  - reversal/correction group.

Posting a transfer creates:

- source movement:
  - account = source;
  - direction = outflow;
  - amount = transfer amount;
  - `TransferId = CashTransferId`;

- destination movement:
  - account = destination;
  - direction = inflow;
  - amount = transfer amount;
  - `TransferId = CashTransferId`.

The pair must be committed atomically.

## Why One Transfer Object Plus Two Movements?

Rejected option: model transfer as one movement with source and destination fields.

Reason rejected:

- every movement invariant says one movement affects exactly one cash account;
- account balances and sessions are easier when each movement belongs to exactly one account;
- reconciliation per account needs source and destination to appear independently.

Rejected option: model transfer as two unrelated movements.

Reason rejected:

- audit cannot prove the two sides belong together;
- reversal could accidentally reverse only one side;
- users cannot understand transfer lifecycle.

Accepted option:

- one transfer aggregate;
- two linked movements;
- one transaction boundary.

## Transfer Invariants

- transfer belongs to exactly one tenant;
- source and destination accounts belong to the same tenant;
- source and destination cannot be the same account;
- source and destination must use the same currency in MVP;
- amount must be greater than zero;
- transfer creates exactly two posted movements;
- source side is an outflow;
- destination side is an inflow;
- both sides share the same `CashTransferId`;
- company-level net effect is zero;
- both sides are posted atomically;
- neither side can be edited directly after posting;
- transfer reversal reverses both sides;
- transfer correction preserves original transfer, reversal pair and replacement pair.

## Transfer Reversal

Decision:

Transfer reversal creates a linked reversal pair, not a physical delete.

For original transfer:

- source A -> destination B, amount 300.

Reversal creates:

- B outflow 300;
- A inflow 300;
- both linked to original transfer/reversal group.

Rationale:

This restores both account expected balances and preserves audit.

## Transfer Reconciliation

Decision:

Each side reconciles with its own cash account/session.

If only one side is counted/reconciled:

- only that account/session can become balanced/discrepant;
- the other account remains unreconciled;
- transfer is visible in both accounts;
- Safe-to-Pay confidence should treat unreconciled side as lower confidence.

Example:

- 1,000 EUR moved from cash box to bank deposit.
- Cash box is counted and balanced.
- Bank account has not imported deposit yet.

Result:

- cash box session can close balanced;
- bank account session remains expected/unconfirmed until bank import/count confirms;
- no silent global balance assumption.

## Multi-Account Invariants

1. Every Cash Account is tenant-scoped.
2. A company can have many Cash Accounts.
3. A company has exactly one active default Cash Account per currency.
4. Cash Account names are unique per tenant among active accounts.
5. Cash Account currency is immutable after first movement.
6. Cash Account type can be changed only while no movements exist, or only through an explicit admin action with audit.
7. Inactive Cash Accounts cannot receive new ordinary movements.
8. Historical movements remain visible after account deactivation.
9. Every Cash Movement affects exactly one Cash Account.
10. Every Cash Movement account belongs to the same tenant as the movement.
11. Cash account balance is derived from posted movements, not mutable user input.
12. A Cash Session is per tenant/account/business date.
13. Reconciliation is per tenant/account/session.
14. Discrepancy in account A does not affect account B.
15. Transfer affects exactly two Cash Accounts.
16. Transfer source and destination cannot be equal.
17. Transfer must net to zero at company level.
18. Transfer source and destination movements are linked and atomic.
19. Transfer reversal reverses both sides.
20. Cross-tenant transfer is forbidden.
21. Unauthorized role cannot create account, transfer or reconciliation.

## MVP Scope

Confirmed MVP:

- multiple active cash accounts per company;
- exactly one default account per company/currency;
- physical cash box account type;
- employee float account type;
- petty cash account type;
- POS/transit account type if enum is adjusted before persistence;
- bank account as manual/import-ready source, not automated connector;
- manual transfers between accounts;
- per-account expected balance;
- per-account daily session;
- per-account reconciliation;
- per-account discrepancy visibility;
- tenant-level authorization;
- no account-specific permissions initially.

Excluded from MVP:

- multi-currency transfers;
- FX rates and conversion;
- bank automation;
- POS integration;
- full treasury workflows;
- per-account user permissions;
- delete account with history;
- partial transfer reversal;
- cross-company transfers;
- certified accounting reconciliation.

## Required Data Model

## CashAccount

Recommended fields:

- `CashAccountId`;
- `TenantId`;
- `Name`;
- `Type`;
- `Currency`;
- `IsDefault`;
- `IsActive`;
- `CreatedByUserId`;
- `CreatedAtUtc`;
- `DeactivatedByUserId`;
- `DeactivatedAtUtc`;

Indexes:

- unique active account name per tenant;
- unique active default per tenant/currency;
- tenant/type for filtering.

## CashMovement

Recommended fields:

- `CashMovementId`;
- `TenantId`;
- `CashAccountId`;
- `CashTransferId`;
- `Amount`;
- `Currency`;
- `Direction`;
- `ReasonCategory`;
- `ReasonText`;
- `EmployeeCashHandlerUserId`;
- `EnteredByUserId`;
- `ApprovedByUserId`;
- `OccurredAtUtc`;
- `PostedAtUtc`;
- `Status`;
- `PhysicalCashHandled`;
- `OriginalMovementId`;
- `CorrectionGroupId`;

Indexes:

- tenant/account/posting date;
- tenant/transfer id;
- original movement id;
- correction group id.

## CashTransfer

Recommended fields:

- `CashTransferId`;
- `TenantId`;
- `SourceCashAccountId`;
- `DestinationCashAccountId`;
- `Amount`;
- `Currency`;
- `ReasonCategory`;
- `ReasonText`;
- `EnteredByUserId`;
- `EmployeeCashHandlerUserId`;
- `OccurredAtUtc`;
- `PostedAtUtc`;
- `Status`;
- `OriginalTransferId`;
- `CorrectionGroupId`.

## CashSession

Recommended fields:

- `CashSessionId`;
- `TenantId`;
- `CashAccountId`;
- `BusinessDate`;
- `OpeningExpectedBalance`;
- `OpeningActualBalance`;
- `Currency`;
- `OpenedByUserId`;
- `OpenedAtUtc`;
- `ClosedByUserId`;
- `ClosedAtUtc`;
- `Status`.

Index:

- unique session per tenant/account/business date.

## CashReconciliation

Recommended fields:

- `CashReconciliationId`;
- `TenantId`;
- `CashSessionId`;
- `CashAccountId`;
- `ExpectedBalance`;
- `ActualBalance`;
- `Discrepancy`;
- `Currency`;
- `Status`;
- `ReconciledByUserId`;
- `ReconciledAtUtc`.

## CashDiscrepancy

Recommended fields:

- `CashDiscrepancyId`;
- `TenantId`;
- `CashReconciliationId`;
- `Amount`;
- `Currency`;
- `Category`;
- `Explanation`;
- `CreatedByUserId`;
- `CreatedAtUtc`;
- `ResolvedByUserId`;
- `ResolvedAtUtc`.

## API Proposal

Use `/v1/cash` for the new product API. Keep existing `/v1/admin/cash` as legacy/admin compatibility until migrated.

## Cash Accounts

- `GET /v1/cash/accounts`
- `GET /v1/cash/accounts/{id}`
- `POST /v1/cash/accounts`
- `PATCH /v1/cash/accounts/{id}`
- `PATCH /v1/cash/accounts/{id}/deactivate`
- `GET /v1/cash/accounts/{id}/balance`

Contract behavior:

- create rejects duplicate active names per tenant;
- create can set default only if it becomes the only default;
- setting a new default unsets previous default in same transaction;
- deactivate rejects default account unless another default is selected.

## Cash Movements

- `POST /v1/cash/accounts/{id}/movements`
- `GET /v1/cash/accounts/{id}/movements`
- `GET /v1/cash/movements/{movementId}`
- `POST /v1/cash/movements/{movementId}/reverse`
- `POST /v1/cash/movements/{movementId}/correct`

Contract behavior:

- movement writes only to the selected account;
- inactive account rejects ordinary movement;
- posted movement cannot be edited/deleted.

## Transfers

- `POST /v1/cash/transfers`
- `GET /v1/cash/transfers/{transferId}`
- `POST /v1/cash/transfers/{transferId}/reverse`
- `POST /v1/cash/transfers/{transferId}/correct`

Contract behavior:

- source and destination required;
- source and destination cannot match;
- both accounts must be active;
- both accounts must belong to tenant;
- both accounts must use same currency in MVP;
- source outflow and destination inflow are atomic.

## Sessions And Reconciliation

- `GET /v1/cash/accounts/{id}/sessions/current`
- `POST /v1/cash/accounts/{id}/sessions/open`
- `GET /v1/cash/accounts/{id}/sessions/{sessionId}`
- `GET /v1/cash/accounts/{id}/sessions/{sessionId}/expected-balance`
- `POST /v1/cash/accounts/{id}/sessions/{sessionId}/reconcile`
- `GET /v1/cash/accounts/{id}/sessions/{sessionId}/reconciliation`
- `POST /v1/cash/reconciliations/{reconciliationId}/discrepancies`
- `PATCH /v1/cash/reconciliations/{reconciliationId}/discrepancies/{discrepancyId}/resolve`

## UI Proposal

## Cash Accounts List

Shows:

- account name;
- type;
- currency;
- current expected balance;
- last reconciliation status;
- default marker;
- active/inactive marker.

Actions:

- create account;
- edit account;
- set default;
- deactivate.

## Create/Edit Cash Account

Fields:

- name;
- type;
- currency;
- default flag;
- active flag.

Warnings:

- currency cannot change after first movement;
- deactivation preserves history.

## Account Balances Overview

Shows all accounts:

- main office cash box;
- workshop cash box;
- employee floats;
- bank/manual accounts;
- POS/transit;
- petty cash.

Also shows:

- company total;
- unreconciled account warnings;
- accounts with open discrepancies.

## Movement Entry

Fields:

- cash account selector;
- employee cash handler;
- amount;
- inflow/outflow;
- reason/category;
- note;
- optional payment/document link.

Default behavior:

- preselect default cash account;
- require explicit account if multiple accounts exist and user changed context.

## Transfer Between Cash Accounts

Fields:

- source account;
- destination account;
- amount;
- reason/category;
- employee handler if physical cash custody changes;
- note.

UI must preview:

- source decreases by amount;
- destination increases by amount;
- company total net effect is zero.

## Per-Account Daily Reconciliation

Shows:

- account name;
- opening expected;
- posted movements;
- expected closing;
- actual counted/imported amount;
- discrepancy.

Status:

- balanced;
- discrepant;
- explained;
- resolved.

## Account Discrepancy View

Shows:

- account;
- date/session;
- expected;
- actual;
- discrepancy amount;
- explanation status;
- linked movements/transfers.

## Employee Float Summary

Shows:

- employee;
- assigned float account;
- current expected balance;
- open sessions;
- unresolved discrepancies;
- movements handled by employee.

## Test Matrix

## Domain Tests

- company can have many Cash Accounts;
- default account uniqueness is represented and enforced at service/repository level;
- account requires name, tenant, currency and type;
- account currency normalizes to ISO code;
- inactive account cannot receive ordinary movement;
- movement affects exactly one account;
- transfer source and destination cannot match;
- transfer amount must be positive;
- transfer creates source outflow and destination inflow;
- transfer net effect is zero;
- transfer reversal restores both accounts;
- session expected balance includes only posted movements for that account;
- reconciliation discrepancy is per account.

## Application Tests

- create multiple cash accounts for same company;
- duplicate account names rejected per company;
- exactly one active default account;
- setting new default unsets old default;
- default account cannot be deactivated without replacement;
- inactive account rejects new movement;
- movement updates only selected account;
- transfer source decreases and destination increases;
- transfer nets to zero at company level;
- transfer cannot use same source/destination;
- transfer cannot cross tenant;
- transfer cannot cross currency in MVP;
- transfer reversal restores both accounts;
- reconciliation is independent per account;
- discrepancy in account A does not affect account B;
- tenant A cannot see tenant B accounts/movements/transfers;
- unauthorized role cannot create account, transfer, movement or reconciliation.

## API Tests

- `GET /cash/accounts` returns only tenant accounts;
- `POST /cash/accounts` creates account;
- duplicate name returns `409 Conflict`;
- invalid type/currency returns `400 Bad Request`;
- deactivate account preserves read access;
- movement into inactive account returns `409 Conflict`;
- transfer source equal destination returns `400 Bad Request`;
- transfer cross-tenant account returns `403` or `404` consistently;
- reconciliation for account A does not alter account B;
- unauthorized role returns `403`.

## Database Integration Tests

- unique account name per tenant;
- unique active default per tenant/currency;
- movement FK requires account;
- transfer FK requires source and destination accounts;
- session unique per tenant/account/date;
- reconciliation FK requires session/account;
- historical movements remain queryable after account deactivation;
- rollback of transfer transaction leaves no one-sided movement.

## Migration Strategy From Current Single CashBalance

Current state:

- one `CashBalance` row per company;
- `CashLedger` entries have only `CompanyId`, no `CashAccountId`;
- existing UI/API display company-level current balance.

## Migration Decision

Decision:

Create one default Cash Account per company and migrate existing cash data into that account.

## Default Account Creation

For each company:

- create default account named `Default cash account` or localized equivalent;
- type = `PhysicalCash`;
- currency = tenant/company currency, default `EUR` if no currency exists;
- `IsDefault = true`;
- `IsActive = true`.

## Existing CashBalance

Recommended approach:

- preserve `CashBalance` during transition as compatibility cache/view;
- initialize default account expected opening from existing `CashBalance.Balance`;
- mark that opening as a migration/import event or first session opening.

Rejected approach:

- drop `CashBalance` immediately.

Reason rejected:

- current payment/cash UI and tests depend on company-level balance;
- immediate removal increases migration risk.

## Existing CashLedger Entries

Recommended phased approach:

1. Add `CashAccountId` to ledger/movement model in future migration.
2. Backfill every existing ledger row with the tenant's default Cash Account.
3. Keep old ledger entries visible as legacy imported movements.
4. Later, convert old `CashLedger` to a compatibility projection of `CashMovement`/ledger entries.

If a new `CashMovement` table is introduced:

- create one imported movement per legacy ledger row where feasible;
- preserve `RefType`, `RefId`, `OriginalPaymentId`, `Reason`, `CreatedBy`, `CreatedAtUtc`;
- map missing reason to category `LegacyImport` and reason text from ref fields;
- do not invent employee cash handlers for historical data.

## CashBalance Future

Options:

1. Keep `CashBalance` as company-level cache.
2. Replace with `CashAccountBalance` per account.
3. Derive balances on read from posted movements and use caches only for performance.

Recommendation:

- MVP implementation should introduce `CashAccountBalance` cache per account, or derive per-account balance if volume is low;
- keep old `CashBalance` temporarily as sum of active account balances for compatibility;
- plan deprecation once UI/API migrates to account-aware cash views.

## Rollback Plan

Use additive migration first:

- add Cash Accounts;
- add account-aware tables/columns;
- backfill default accounts;
- keep existing `CashBalance` and `CashLedger`;
- do not delete legacy columns/tables in the first release.

Rollback:

- disable new account-aware UI/API;
- continue reading existing `CashBalance` and `CashLedger`;
- account data can remain unused;
- no irreversible data loss.

Do not ship destructive cleanup until:

- account-aware flows are stable;
- backup/restore drill covers new tables;
- migration has been tested on realistic data.

## Current Cash Integrity Contracts: Required Adjustments

The current intended contracts are a good foundation but need small adjustments before schema/API implementation:

1. Add `CashTransfer` domain model.
2. Add transfer commands:
   - `CreateCashTransferCommand`;
   - `ReverseCashTransferCommand`;
   - `CorrectCashTransferCommand`.
3. Add transfer result model and ports:
   - `ICashTransferReader`;
   - `ICashTransferWriter`.
4. Add transfer linkage to `CashMovementResult`, e.g. `CashTransferId`.
5. Add `PosTransit` and `PettyCash` to `CashAccountType`, or explicitly defer them.
6. Decide whether `CashAccount` needs `CreatedBy`, timestamps and deactivation metadata in domain result contracts.
7. Add account default management command, because exactly-one-default requires orchestration.
8. Add account balance result per account.
9. Add currency policy: single currency MVP, reject cross-currency transfer.

## Rejected Options

## One Global Company Cash Balance

Rejected because it cannot prove source-level cash integrity.

## One Movement With Source And Destination

Rejected because it violates the clean invariant that a movement affects exactly one account and makes per-account reconciliation harder.

## Two Unlinked Movements For Transfer

Rejected because it breaks auditability and can leave one-sided reversals.

## Physical Delete For Cash Accounts

Rejected because account history must remain readable.

## Multi-Currency Transfers In MVP

Rejected because FX conversion and accounting implications are outside the Cash Cockpit MVP.

## Account-Level Permissions In MVP

Deferred. Useful later, but tenant-level roles are enough to implement source-level Cash Integrity first.

## Implementation Sequence

## PR 1 - Contract Adjustment

Add:

- `CashTransfer`;
- transfer enums/status if needed;
- transfer commands/results/ports;
- account balance result;
- missing account types;
- default-account command/result.

No EF schema.

## PR 2 - Domain/Application Use Cases

Add use cases with fake-port tests:

- create/deactivate/set default account;
- create movement;
- create transfer;
- reverse transfer;
- open session;
- reconcile per account.

No EF schema.

## PR 3 - Persistence Schema

Add EF entities/migration:

- `CashAccounts`;
- `CashMovements`;
- `CashTransfers`;
- `CashSessions`;
- `CashReconciliations`;
- `CashDiscrepancies`;
- optional `CashAccountBalances`.

Backfill default accounts.

## PR 4 - Infrastructure Implementations

Implement repositories/readers/writers.

Preserve existing payment behavior by routing legacy payment cash effects into the default account.

## PR 5 - API Contracts

Add `/v1/cash` account-aware endpoints.

Keep `/v1/admin/cash` compatibility until WebApp moves.

## PR 6 - WebApp UX

Add:

- account list;
- transfer form;
- movement form with account selector;
- per-account balance/reconciliation.

## PR 7 - Migration Cutover

Move payment cash effects from company-level ledger to account-aware movement/ledger.

Deprecate old company-level balance only after compatibility period.

## Risks

- Migration risk from one company balance to per-account balances.
- Payment cash effects currently lack account selection; default-account fallback is necessary.
- Transfers must be atomic or they can corrupt expected balances.
- Cross-account reconciliation can confuse users if transfer timing differs between physical and bank sources.
- Employee float accounts can become pseudo-HR if scope is not constrained.
- Bank accounts can become full bank reconciliation/accounting if product boundaries are not enforced.
- Default account uniqueness needs infrastructure/database enforcement, not only domain records.
- Historical data cannot reliably reconstruct employee cash handlers.

## Final Recommendation

Model Cash Accounts as first-class company-owned cash sources.

Keep the invariant:

> one Cash Movement affects exactly one Cash Account.

Model transfers as:

> one CashTransfer business object that atomically creates two linked Cash Movements: source outflow and destination inflow.

Migration should be additive and conservative:

> create one default Cash Account per company, map existing `CashBalance` and `CashLedger` history to that default account, and keep legacy `CashBalance` as a temporary compatibility cache.

This keeps OpenCashFlow aligned with Cash Integrity without turning it into accounting software or ERP treasury management.
