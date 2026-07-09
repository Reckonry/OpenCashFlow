# Cash Custody Decision Record

Date: 2026-07-08

Status: Accepted for product/domain direction. Not yet implemented.

Source analysis:

- `Docs/analysis/cash-custody-domain-analysis.md`
- `Docs/analysis/multi-cash-account-model.md`
- `Docs/product/CASH_INTEGRITY_DECISION_RECORD.md`

## Decision Summary

OpenCashFlow will treat Cash Custody as the umbrella product and domain pillar for operational cash.

Cash Custody means:

> The provable chain of responsibility for company money from receipt to reconciliation.

Cash Integrity remains required, but it is not the umbrella concept. It is the proof capability inside the custody model.

The product decision is:

> OpenCashFlow must prove who or what is responsible for company money, why that responsibility exists, how it changed, and whether it reconciles with reality.

This decision must be made before schema, contracts, API, or UI are expanded further. Otherwise the product risks implementing a ledger that records movements but does not answer the owner's real question: who has the money and can we prove it?

## 1. Cash Custody As Umbrella Domain

Decision:

Cash Custody is the broader domain language for OpenCashFlow operational cash.

Cash Custody covers:

- cash received from customers;
- cash held in a cash box;
- cash held by an employee;
- cash assigned as float;
- cash paid to suppliers;
- cash deposited to a bank;
- cash in POS/transit;
- cash returned to the company;
- cash reconciled at the end of a day/session;
- discrepancies that prove the chain is incomplete.

Rationale:

Small business owners do not only need to know that cash moved. They need to know who was responsible for the money at each point, why, and whether the money was later proven.

Product language:

> Cash Custody is the chain of responsibility for company money.

## 2. Cash Integrity As Proof Capability

Decision:

Cash Integrity remains a required product capability, but it is subordinate to Cash Custody.

Cash Integrity means:

- custody events are explicit;
- movements are source-bound;
- reasons are recorded;
- handlers, recorders, approvers and reconcilers are distinguishable;
- posted facts are immutable;
- corrections use reversal plus replacement;
- expected cash can be derived;
- actual cash can be compared;
- discrepancies remain visible until explained or resolved.

Rationale:

Cash Custody describes the business problem. Cash Integrity describes the quality bar required to trust the custody chain.

Product language:

> Cash Integrity proves that the custody chain is complete, reconciled, and explainable.

## 3. Cash Movement As Balance-Affecting Fact

Decision:

Cash Movement remains a valid lower-level concept.

A Cash Movement is a posted, balance-affecting fact for one Cash Account.

It answers:

- which account/source changed;
- amount;
- direction;
- reason/category;
- handler when physical cash is handled;
- recorded by;
- approved by when required;
- timestamp;
- reversal/correction links.

Rationale:

Balances and reconciliations need movement facts. The mistake would be treating movement as the whole business concept. Movement is the ledger consequence of a custody event.

Implementation implication:

Do not rename all future code to custody terminology blindly. Keep `CashMovement` where it accurately represents a balance-affecting fact. Add custody concepts where responsibility must be represented.

## 4. Cash Account As Cash Source And Custody Holder

Decision:

Cash Account is the domain object for a company-controlled cash source.

It is also a type of Custody Holder.

Examples:

- main office cash box;
- workshop cash box;
- petty cash;
- employee float;
- bank account;
- POS/transit account.

Rationale:

A cash account is not just a reporting bucket. It can be responsible for money. For example, the main cash box holds custody until money is assigned to an employee, deposited to bank, paid out, or reconciled.

MVP rule:

Every posted Cash Movement affects exactly one Cash Account.

## 5. Terminology

Use this language consistently.

| Term | Meaning |
| --- | --- |
| Cash Custody | The chain of responsibility for company money. |
| Cash Integrity | Proof that the custody chain is complete, reconciled, immutable and explainable. |
| Cash Account | Company-controlled cash source, such as a cash box, bank account, or employee float. |
| Cash Source | User-facing synonym for Cash Account. |
| Custody Holder | Person, account, institution, or boundary actor responsible for money at a point in the chain. |
| Custodian | The person or source currently responsible for money. |
| Cash Handler | The person physically receiving, carrying, counting, or handing over cash. |
| Recorded By | The authenticated user who records the event in OpenCashFlow. |
| Approved By | The user who authorizes a high-risk custody event. |
| Reconciled By | The user who verifies expected cash against actual counted/reported cash. |
| Cash Movement | Balance-affecting fact for one cash account. |
| Custody Transfer | Business event where responsibility moves from one holder/source to another. |
| Expected Cash | Cash that should exist based on opening state plus posted events. |
| Actual Cash | Counted cash or imported/reported account balance. |
| Discrepancy | Actual cash minus expected cash; evidence of an incomplete or broken custody chain. |

Naming decisions:

- use `Cash Custody` for product/domain pillar;
- keep `Cash Integrity` for proof quality;
- keep `Cash Movement` as event/ledger fact;
- use `Recorded By`, not `Entered By`, in user-facing copy;
- use `Cash Handler` for physical handling;
- use `Custody Holder` when the responsible party might be an account, employee, bank, customer, supplier, or other actor.

## 6. Transfer As Custody Transfer

Decision:

A transfer is a transfer of custody.

It should be implemented as two linked Cash Movements:

- source movement: outflow from the source account/holder;
- destination movement: inflow to the destination account/holder.

Both sides must share a transfer id and be created atomically.

Examples:

- main cash box to employee float;
- employee float back to main cash box;
- physical cash box to bank deposit;
- POS/transit settlement to bank.

Rules:

- source and destination cannot be the same;
- source and destination must belong to the same tenant;
- same-currency transfers net to zero at company level;
- reversal of a transfer reverses both sides;
- one side must never exist without the other.

Rationale:

One abstract transfer alone is not enough for per-account balance and reconciliation. Two unrelated movements are not enough to prove business intent. The correct model is one custody transfer with two linked movement facts.

## 7. Multi-Cash-Account MVP Scope

Decision:

The MVP supports multiple cash accounts per company.

Included:

- one active default cash account per company;
- physical cash boxes;
- employee float;
- bank account as cash source/context;
- POS/transit account;
- petty cash;
- deactivate account;
- per-account expected balance;
- per-account daily/session reconciliation;
- transfer between accounts;
- discrepancy per account/session.

Excluded from MVP:

- multi-currency transfers;
- FX conversion;
- bank automation;
- per-account permission rules;
- full accounting bank ledger replacement;
- treasury workflows.

Rationale:

Multiple cash sources are required to prove custody. Multi-currency, automation, and permission-per-account can wait until the core custody chain is proven.

## 8. Chain Of Custody Invariant

Decision:

Every euro tracked by OpenCashFlow must have a provable custody chain from the moment it enters the system's operational scope.

Minimum invariant:

Every custody event must have:

- tenant/company;
- source or origin;
- current custodian or destination;
- cash account/source;
- amount and currency;
- reason/category;
- recorded-by actor;
- cash handler when physical cash is handled;
- approval where policy requires it;
- audit trail;
- reconciliation status or path to reconciliation.

Operational rule:

Cash must never become orphaned. Responsibility cannot disappear through edit, delete, import, or partial transfer.

Discrepancy rule:

A discrepancy is a broken or incomplete custody chain. It must remain visible until explained or resolved.

## 9. Historical Legacy Ledger Limitation

Decision:

Historical ledger data must not be upgraded into fake custody data.

Current legacy cash data can prove:

- company-level ledger deltas;
- payment/admin adjustment references;
- created-by for some ledger rows;
- balance reconstruction.

It cannot fully prove:

- exact physical cash handler;
- exact custodian chain;
- approval;
- source/account before migration;
- reconciliation status;
- discrepancy lifecycle.

Migration rule:

When historical data is migrated, assign it to a default Cash Account only as legacy evidence. Do not invent handler, approver, or reconciliation facts that were never captured.

Product rule:

Full Cash Custody proof begins from the implementation/migration date forward.

## 10. Forecast And Safe-To-Pay Gate

Decision:

Forecast and Safe-to-Pay must treat unreconciled custody as lower confidence.

Rules:

- reconciled cash can be high-confidence;
- unreconciled cash can be visible but lower-confidence;
- open discrepancies reduce safe cash confidence;
- employee-held cash is not equivalent to bank-confirmed cash;
- transfers in transit must reduce confidence until both sides are reconciled or settled;
- forecast/Safe-to-Pay must remain WIP/Experimental until custody events, transfers, reconciliation and discrepancy status exist.

Rationale:

Safe-to-Pay is only valuable if users trust it. A recommendation built on unreconciled or unexplained cash is dangerous.

Product language:

> Safe-to-Pay can only be trusted when the custody chain is trusted.

## MVP Acceptance Criteria

The Cash Custody MVP is acceptable only when OpenCashFlow can prove this scenario:

1. Employee X receives 200 EUR for `rinnovo K`.
2. Employee Z receives 100 EUR for `acquisto F`.
3. Both events identify source/account, handler, recorded-by, reason and tenant.
4. Expected cash for the account/session is 300 EUR higher.
5. Actual cash of 300 EUR reconciles as balanced.
6. Actual cash of 250 EUR creates a visible -50 EUR discrepancy.
7. A posted event cannot be edited.
8. Correction uses reversal plus replacement.
9. Another tenant cannot see or alter the chain.
10. Unauthorized roles cannot enter, transfer, reverse, correct, reconcile or resolve custody.

## Implementation Implications

Do not implement this decision by renaming everything.

Recommended technical implications:

- add custody/transfer concepts before persistence schema;
- keep movement as a balance-affecting fact;
- introduce transfer id/grouping before account balances are persisted;
- ensure sessions/reconciliations are per cash account;
- ensure legacy ledger migration is explicit about limited evidence;
- connect reconciliation/discrepancy state to future forecast confidence.

Recommended next implementation branch:

`feature/cash-custody-transfer-contracts`

Purpose:

- adjust domain/application contracts to include custody transfer and holder semantics;
- avoid schema/API/UI changes until the terminology and invariants are stable.

## Final Decision

Cash Custody is the product/domain pillar.

Cash Integrity is the proof capability.

Cash Movement is the balance-affecting fact.

Cash Account is the cash source and possible custody holder.

Transfer is custody transfer implemented by linked movements.

Safe-to-Pay remains gated until custody is reconciled, explainable and confidence-aware.
