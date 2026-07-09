# Cash Integrity Decision Record

Date: 2026-07-08

Status: Accepted as proof capability inside Cash Custody. Not yet implemented.

Source analysis: `Docs/analysis/cash-integrity-foundation-analysis.md`

Superseding umbrella decision: `Docs/product/CASH_CUSTODY_DECISION_RECORD.md`

## Decision Summary

Cash Integrity is a required proof capability for OpenCashFlow.

OpenCashFlow cannot credibly provide Safe-to-Pay guidance, cash forecasts, cash confidence, or owner payment decisions unless it can prove the operational cash facts behind those decisions.

The broader product/domain pillar is now Cash Custody:

> the chain of responsibility for company money from receipt to reconciliation.

The product decision is:

> Cash movements must become explicit, auditable, source-bound, employee-aware, and reconciliation-ready before forecast and Safe-to-Pay move beyond WIP/Experimental.

## Product Language

Use these terms consistently.

| Term | Meaning |
| --- | --- |
| Cash Custody | The chain of responsibility for company money from receipt to reconciliation. |
| Cash Integrity | The ability to prove the custody chain is complete, immutable, reconciled, and explainable. |
| Cash Account | A cash source controlled by the company, such as a physical cash box, bank account, or employee float. |
| Cash Source | User-facing synonym for Cash Account. |
| Cash Movement | A business event where cash is received, paid, moved, corrected, or reversed. |
| Employee Cash Handler | The employee who physically received or took the money. |
| Entered By | The user who records the movement in OpenCashFlow. |
| Approved By | The user who authorizes a movement when approval is required. |
| Expected Cash | Opening cash plus posted movements for the session/account. |
| Actual Cash | Counted cash or imported bank/account balance. |
| Discrepancy | Actual cash minus expected cash. |
| Reversal | A movement that cancels a posted movement without deleting it. |
| Correction | A reversal plus replacement movement. |

## 1. What Is A Cash Account?

Decision:

OpenCashFlow will model cash sources as Cash Accounts.

MVP includes:

- one default physical cash box per company;
- optional additional physical cash boxes;
- optional employee floats;
- optional bank accounts as cash sources, but only for balance/import/reconciliation context.

MVP excludes:

- full bank ledger replacement;
- bank transaction enrichment beyond cash decision needs;
- chart-of-accounts accounting behavior;
- interbank treasury workflows.

Rationale:

The current company-level cash balance is too coarse. A business can have a front desk cash box, a workshop cash box, employee-held cash, and a bank account. Safe-to-Pay cannot be trusted if all sources collapse into one unexplained number.

Product rule:

Every posted cash movement must affect exactly one Cash Account.

## 2. What Is A Cash Movement?

Decision:

A Cash Movement is the canonical product event for operational cash.

It must answer:

- who physically received or took the money;
- who entered it;
- who approved it, if approval is required;
- how much moved;
- whether it was inflow or outflow;
- why it moved;
- which Cash Account it affected;
- when it happened;
- which tenant owns it.

Required fields:

- tenant/company;
- cash account;
- amount;
- direction;
- reason/category;
- employee cash handler when a person physically handles the cash;
- entered-by user;
- occurred/posting timestamp.

Optional fields:

- payment reference;
- document type;
- free text note;
- approved-by user;
- external import id;
- attachment/reference evidence.

Rationale:

Payments alone are not enough. A payment may describe a business transaction, but Cash Integrity needs an explicit cash movement with handler, source, reason and audit semantics.

## 3. Who Physically Receives Or Takes Money?

Decision:

The person who physically receives or takes money is the Employee Cash Handler.

This is not automatically the same as:

- the user who enters the movement;
- the user who created a payment;
- the administrator who later reconciles the day.

Rationale:

The core proof scenario requires: Employee X received 200 EUR and Employee Z received 100 EUR. The current `UserID`/`CreatedBy` fields are not semantically clear enough to prove that.

Product rule:

When cash is physically handled by an employee, Employee Cash Handler is required.

## 4. Who Enters It?

Decision:

Every movement records Entered By.

Entered By is the authenticated user who records the movement in OpenCashFlow.

Rationale:

The system must distinguish operational responsibility from data-entry responsibility. A manager may enter a movement on behalf of an employee; an employee may enter their own receipt; both cases must be auditable.

## 5. Who Approves It?

Decision:

Approval is optional in MVP for ordinary movement entry, but the model must support it.

MVP behavior:

- low-risk movement entry can be posted by authorized cash roles without separate approval;
- reversal, correction, end-of-day reconciliation and discrepancy resolution require stronger roles;
- future configuration may require approval above a threshold.

Rationale:

Mandatory approval for every movement could block adoption. The schema and use cases must still support approval because corrections and reconciliation are control points.

## 6. Reason And Category

Decision:

Every Cash Movement requires a reason/category.

MVP categories:

- customer payment received;
- supplier payment;
- owner withdrawal/deposit;
- employee float;
- bank deposit/withdrawal;
- cash count adjustment;
- correction;
- other.

Free-text reason is required for:

- manual adjustments;
- corrections;
- reversals;
- discrepancy explanations;
- category `other`.

Rationale:

Cash without reason is not trustworthy. A ledger delta that says only `Payment` is insufficient for owner decisions and audit review.

## 7. Immutability Policy

Decision:

Posted cash movements cannot be edited or deleted.

Corrections must use:

1. reversal of the original movement;
2. replacement movement with corrected facts;
3. audit trail linking original, reversal and replacement.

Rationale:

OpenCashFlow must preserve the historical cash story. Silent mutation breaks trust, reconciliation, audit and Safe-to-Pay confidence.

Allowed states:

- `Draft`;
- `Posted`;
- `Reversed`;
- `Corrected`.

Product rule:

If money has affected expected cash, the original record remains visible.

## 8. Daily Session

Decision:

OpenCashFlow will model a daily cash session per Cash Account.

Session lifecycle:

1. Open session.
2. Record opening balance.
3. Post movements during the day.
4. Calculate expected balance.
5. Count or import actual balance.
6. Reconcile.
7. Close as balanced or discrepant.

Required session facts:

- cash account;
- business date;
- opening expected balance;
- opening actual balance when counted;
- posted movements;
- expected closing balance;
- actual counted/imported balance;
- close status.

Close statuses:

- `Open`;
- `Balanced`;
- `Discrepant`;
- `Explained`;
- `Resolved`.

Rationale:

A current balance is not the same as a day that was opened, operated, counted and closed.

## 9. Reconciliation

Decision:

Reconciliation compares Expected Cash with Actual Cash.

Definitions:

- Expected Cash = opening balance + posted movements + corrections.
- Actual Cash = counted physical cash or imported bank/source balance.
- Discrepancy = actual cash - expected cash.

Statuses:

- `Balanced`: discrepancy is zero.
- `Discrepant`: discrepancy is non-zero and not explained.
- `Explained`: discrepancy has a documented explanation but may still represent real loss/overage.
- `Resolved`: follow-up action has closed the discrepancy.

Product rule:

A discrepancy is never hidden. It remains visible until explained or resolved.

Rationale:

Owner trust depends on showing the mismatch. The system must not convert unexplained differences into generic adjustments without preserving the discrepancy.

## 10. Roles

Decision:

Cash Integrity needs explicit permissions.

MVP role behavior:

| Action | Allowed roles |
| --- | --- |
| View own/company cash movements | CompanyAdmin, InstanceAdmin, authorized employee role |
| Enter cash movement | CompanyAdmin, InstanceAdmin, authorized cash operator |
| Reconcile daily session | CompanyAdmin, InstanceAdmin |
| Reverse/correct posted movement | CompanyAdmin, InstanceAdmin |
| Resolve discrepancy | CompanyAdmin, InstanceAdmin |
| Export cash ledger/audit | CompanyAdmin, InstanceAdmin |
| Cross-company cash access | InstanceAdmin only, with explicit company context |

Implementation can map these to current roles first, then introduce narrower permissions later.

Rationale:

The current role split is enough for admin-only cash operations, but movement entry and reconciliation need clearer separation before broader employee workflows.

## 11. Forecast And Safe-To-Pay Status

Decision:

Forecast and Safe-to-Pay are WIP/Experimental until Cash Custody exists and Cash Integrity can prove it.

They may exist as prototypes, concept screens, or planning documents, but must not be presented as reliable payment recommendations until:

- Cash Accounts exist;
- Cash Movements exist;
- custody transfer and holder semantics exist where responsibility changes;
- posted movements are immutable;
- daily sessions exist;
- reconciliation exists;
- discrepancies are visible;
- tenant isolation and authorization tests pass;
- cash confidence can distinguish reconciled from unreconciled cash.

Product rule:

Unreconciled custody must reduce forecast confidence.

## 12. MVP Scope

Included now:

- default physical cash account per company;
- cash movement model;
- employee cash handler;
- required reason/category;
- immutable posting;
- reversal/correction;
- daily session;
- expected vs actual reconciliation;
- discrepancy explanation;
- audit trail;
- tenant isolation;
- role enforcement;
- WebApp screens for daily movement entry and reconciliation.

Explicitly excluded from MVP:

- full accounting ledger;
- certified bank reconciliation;
- payroll;
- POS;
- inventory/MRP;
- tax compliance;
- automated bank connectors;
- multi-step approval workflows beyond basic admin control;
- advanced anomaly detection;
- cash forecasting marketed as stable.

## 13. Required Test Scenarios

### Scenario A - Balanced Cash Day

Given:

- opening cash is 0;
- Employee X receives 200 EUR for `rinnovo K`;
- Employee Z receives 100 EUR for `acquisto F`;
- actual counted cash is 300.

Expected:

- X is linked to the 200 movement;
- Z is linked to the 100 movement;
- both reasons are recorded;
- expected cash is 300;
- actual cash is 300;
- reconciliation status is `Balanced`;
- audit trail exists.

### Scenario B - Discrepancy

Given:

- same movements as Scenario A;
- actual counted cash is 250.

Expected:

- expected cash is 300;
- discrepancy is -50;
- status is `Discrepant`;
- discrepancy remains visible until explained or resolved.

### Scenario C - Reversal And Correction

Given:

- a posted movement is wrong.

Expected:

- direct edit is rejected;
- original movement remains visible;
- reversal is created;
- replacement movement is created;
- audit links all records.

### Scenario D - Tenant Isolation

Given:

- a user from Tenant A attempts to read or alter Tenant B cash movements.

Expected:

- access is rejected;
- no data leaks;
- no ledger entry changes.

### Scenario E - Unauthorized Role

Given:

- a user without cash permission attempts movement entry, reconciliation, correction or export.

Expected:

- request is rejected;
- no movement, session, reconciliation or audit mutation occurs except optional security audit.

### Scenario F - Posted Movement Deletion

Given:

- a posted movement exists.

Expected:

- physical delete is forbidden;
- soft delete is forbidden for posted movement;
- reversal is the correction path.

## 14. Implementation Gate

No implementation branch should add forecast/Safe-to-Pay production behavior until it can state which Cash Custody chain
and Cash Integrity guarantees it relies on.

Recommended implementation sequence:

1. domain/application contracts and invariants;
2. persistence model and migration;
3. cash movement use cases;
4. session/reconciliation use cases;
5. API contracts;
6. WebApp workflow;
7. test coverage;
8. forecast confidence integration.

## Final Decision

OpenCashFlow will not treat cash as an anonymous balance.

It will treat cash as a custody chain proven by accountable movements:

> source + custodian + cash handler + reason + actor + posting + audit + reconciliation.

That chain is the foundation for Safe-to-Pay.
