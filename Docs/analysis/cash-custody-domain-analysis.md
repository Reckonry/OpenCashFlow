# Cash Custody Domain Analysis

Branch: `analysis/cash-custody-domain`

Date: 2026-07-08

Status: analysis only. No production code, schema, API, UI, or contract changes are implemented by this document.

## Purpose

OpenCashFlow is trying to become the self-hosted cash cockpit for small businesses. The current Cash Integrity direction says the system must prove what cash moved, who handled it, why it moved, and whether reality matches the expected balance.

This document challenges that language.

The central question is:

> Is the real business concept cash moving, or responsibility for cash changing hands?

The answer matters before more code is written. If OpenCashFlow models only movement, it can become a better ledger. If it models custody, it can become the system that proves who was responsible for company money at every point in time.

## Inputs Reviewed

Primary inputs:

- `Docs/product/CASH_INTEGRITY_DECISION_RECORD.md`
- `Docs/analysis/cash-integrity-foundation-analysis.md`
- `Docs/analysis/multi-cash-account-model.md`
- current runtime cash model on `development`
- proposed Cash Integrity domain/application contracts on `feature/cash-integrity-domain-contracts`

Important repository state:

- `development` is currently at `f61525a`.
- The Cash Integrity contract branch exists locally and remotely.
- The Cash Integrity contracts are not part of local `development` at the time of this analysis, so they are treated as proposed baseline, not current runtime behavior.

## Executive Summary

Cash Movement is not wrong, but it is not the deepest domain concept.

The deeper concept is Cash Custody:

> A named party or source becomes responsible for company money until that responsibility is transferred, spent, deposited, returned, reconciled, corrected, or closed.

Recommended decision:

- keep `CashMovement` as the event/ledger-level fact;
- introduce `CashCustody` as the product pillar and domain language;
- treat `CashCustodyChain` or `CashCustodyRecord` as the conceptual aggregate around which movement, transfer, reconciliation, discrepancy, and audit make sense;
- model transfers as custody transfers, implemented by linked movement entries when persistence is designed;
- make reconciliation a proof step in a custody chain, not just a balance comparison.

Business reasoning:

Small business owners do not wake up asking, "How many movements did I record?" They ask:

- Who has the cash?
- Why do they have it?
- Should they still have it?
- Did they bring it back?
- Is the cash box short?
- Can I prove what happened?

That is custody language.

## Current Model

### Runtime Model On `development`

The current runtime cash model is company-level:

- `CashBalance`
  - one balance per company;
  - stores current balance only.

- `CashLedger`
  - records deltas against a company;
  - references payment/admin adjustment events;
  - does not identify a cash account;
  - does not identify a physical cash handler;
  - does not model responsibility transfer;
  - does not model session/reconciliation/discrepancy.

This is a ledger foundation, not a custody foundation.

### Proposed Cash Integrity Contracts

The proposed Cash Integrity branch introduces:

- `CashAccount`
- `CashMovement`
- `CashSession`
- `CashReconciliation`
- `CashDiscrepancy`
- movement commands and read/write ports

The proposed model improves the system materially:

- cash is source-bound through `CashAccountId`;
- cash has direction through `CashMovementDirection`;
- physical handling can require `EmployeeCashHandlerId`;
- entered/approved users are separate fields;
- sessions and reconciliations become explicit;
- posted movements become immutable through reversal/correction.

However, the proposed model is still primarily movement-centered. It can answer "what happened?" but it does not make "who is currently responsible?" the first-class question.

## Proposed Model

The product should evolve from:

> Cash Integrity = prove cash movements and balances.

to:

> Cash Custody = prove responsibility for company money across its lifecycle.

Cash Integrity remains a capability. Cash Custody becomes the stronger domain pillar.

Recommended conceptual model:

- Cash Account: where company-controlled cash is held or reported.
- Custody Holder: who or what currently has responsibility for the money.
- Custody Event: an auditable fact that changes or proves custody.
- Custody Transfer: an event that moves responsibility from one holder to another.
- Cash Movement: the accounting/ledger consequence of a custody event.
- Cash Session: a time-boxed proof window for one account/source.
- Cash Reconciliation: the proof that expected custody matches actual custody.
- Cash Discrepancy: evidence that the custody chain is broken or incomplete.

The model should not discard movements. It should subordinate them to custody.

## Aggregate Root

Candidate aggregate roots:

- `CashMovement`
- `CashTransfer`
- `CashCustody`
- `CashSession`
- `CashLedger`

### CashMovement

Strengths:

- simple;
- maps well to ledger rows;
- supports inflow/outflow;
- good for expected balance calculation;
- good persistence boundary for append-only records.

Weaknesses:

- too event-specific;
- does not naturally answer who currently has responsibility;
- a transfer is awkward because it requires coordination across two movements;
- a reconciliation is only indirectly related;
- can become a technical ledger concept rather than business language.

Verdict: useful entity/event, not the best aggregate root for the business problem.

### CashTransfer

Strengths:

- captures movement between two holders/sources;
- naturally links source and destination;
- fits employee float and bank deposit scenarios.

Weaknesses:

- not all cash events are transfers;
- customer receipts, supplier payments, owner deposits, adjustments and discrepancies are broader than transfer;
- transfer is a subtype of custody change, not the whole domain.

Verdict: important aggregate or command boundary for transfer operations, but not the top-level model.

### CashCustody

Strengths:

- directly represents responsibility for company money;
- covers receipt, holding, transfer, spend, return, deposit, reconciliation and discrepancy;
- matches owner concerns;
- gives strong language for audit and proof;
- naturally supports "every euro must have a current responsible holder";
- can include movement as a consequence rather than confusing movement with responsibility.

Weaknesses:

- more abstract than movement;
- must be carefully scoped to avoid over-modeling every individual banknote;
- needs clear implementation rules to avoid a complex event-sourcing system too early.

Verdict: best business aggregate/root concept. It should be introduced as the conceptual aggregate and product pillar.

### CashSession

Strengths:

- strong boundary for opening/closing/reconciliation;
- useful for daily operations;
- aligns with the Monday/evening cash routine.

Weaknesses:

- custody can cross session boundaries;
- an employee float can remain open for more than one day;
- a session proves custody for a period, but does not own the full custody lifecycle.

Verdict: session is a proof window, not the root concept.

### CashLedger

Strengths:

- append-only;
- deterministic balance calculation;
- familiar audit/accounting shape.

Weaknesses:

- technical artifact;
- weak product language;
- does not answer responsibility without additional semantics.

Verdict: ledger is an implementation/projection, not a domain aggregate root.

### Aggregate Root Decision

The real aggregate root should be:

> Cash Custody

More precisely, implementation may use records such as `CashCustodyChain`, `CashCustodyEvent`, and `CashCustodyTransfer`, but the domain root should be the custody chain for company money within a tenant/account context.

Pragmatic implementation guidance:

- Do not model every euro as a separate object in MVP.
- Model custody at the transaction/batch level.
- A custody chain starts when company money enters a controlled cash source or is assigned to a holder.
- The chain advances through custody events.
- The chain closes when the money is spent, deposited, returned, reconciled into a session, or otherwise resolved.

## Who Owns Money?

Strictly, the company owns company money. What changes is custody/responsibility.

Recommended language:

- Owner: the company/legal entity.
- Custodian: the party/source currently responsible for the money.
- Handler: the person physically receiving or carrying cash.
- Recorder: the user who entered the event.
- Approver: the user who authorized the event.
- Reconciler: the user who verified expected vs actual cash.

The system should avoid saying "employee owns the cash." The employee holds custody or responsibility.

## Complete Custody Chain

A small-business custody chain can look like this:

1. Customer pays 200 EUR.
2. Employee X receives the cash.
3. Employee X records or reports the receipt.
4. The company cash account expected balance increases.
5. Employee X deposits the cash into the main cash box.
6. A manager reconciles the main cash box.
7. The cash is later deposited into a bank account.
8. The bank confirms the deposit.
9. The chain is closed for physical custody and continues as bank custody.

Another chain:

1. Main cash box assigns 300 EUR to Employee Z as float.
2. Employee Z holds custody.
3. Employee Z spends 100 EUR on `acquisto F`.
4. Employee Z returns 200 EUR.
5. The float is reconciled.
6. The discrepancy is zero, so the chain is proven.

Broken chain:

1. Main cash box assigns 300 EUR to Employee Z.
2. Employee Z reports 100 EUR spent.
3. Employee Z returns 150 EUR.
4. Expected return is 200 EUR.
5. Actual return is 150 EUR.
6. Discrepancy is -50 EUR.
7. The custody chain remains open/discrepant until explained or resolved.

## Money Moving Or Responsibility Moving?

The product should model responsibility moving first.

Money movement is the visible surface:

- inflow;
- outflow;
- transfer;
- reversal;
- correction.

Responsibility movement is the business truth:

- who had company money before;
- who has it now;
- why responsibility changed;
- who authorized that change;
- whether responsibility was later proven by reconciliation.

This distinction is crucial.

When money moves from the main cash box to an employee float, the company still owns the money. The meaningful business event is not only "outflow from account A, inflow to account B." It is:

> responsibility moved from the main cash custodian to Employee Z.

## Chain Of Custody Questions

Every custody record should answer:

- Where did the money come from?
- Who or what had responsibility before?
- Who or what has responsibility now?
- Why did responsibility change?
- When did it happen?
- Who physically handled it?
- Who recorded it?
- Who authorized it, if approval was required?
- Which cash account/source changed?
- Was it reconciled?
- Is there a discrepancy?
- Can the chain be proven without mutating history?

If any of these are unanswerable, OpenCashFlow cannot honestly claim cash custody.

## Concept Relationships

### Cash Account

A company-controlled source or container of cash value.

Examples:

- main cash box;
- workshop cash box;
- petty cash;
- employee float;
- POS transit;
- bank account.

Relationship:

- a Cash Account can be a Custody Holder;
- a Cash Account can have sessions;
- a Cash Account has expected balances derived from custody events/movements;
- a Cash Account can be reconciled independently.

### Cash Custodian

The person or organizational actor currently responsible for money.

Examples:

- Employee X;
- Employee Z;
- manager;
- external bank as institutional custodian;
- supplier/customer only at boundary moments.

Relationship:

- a custodian can be a person or institutional holder;
- not every custodian is a user;
- not every user is a custodian.

### Cash Handler

The person physically touching or carrying cash.

Relationship:

- often an employee;
- may differ from entered-by;
- required when physical cash is handled;
- is evidence in the custody chain.

### Entered By

The authenticated user who records the event.

Relationship:

- accountable for data entry;
- may be the same as handler, but must not be assumed.

### Approved By

The user who authorizes a custody event.

Relationship:

- optional for low-risk ordinary entry in MVP;
- required for reversal, correction, reconciliation close, discrepancy resolution, and high-risk transfers.

### Reconciled By

The user who verifies expected vs actual custody.

Relationship:

- should be distinct in permission model from ordinary movement entry;
- proves or challenges custody.

### Cash Movement

The event-level ledger fact that changes expected balance for one account.

Relationship:

- generated by or attached to a custody event;
- affects exactly one Cash Account;
- should be append-only after posting;
- remains necessary for balance derivation.

### Cash Transfer

A business operation where custody moves between two holders/accounts.

Relationship:

- conceptually one custody transfer;
- operationally produces linked source/destination movement facts;
- must be atomic.

### Cash Session

A proof window for one account/source over a business date or operating period.

Relationship:

- contains opening expected/actual state;
- includes posted movements/custody events;
- closes with reconciliation.

### Cash Ledger

An append-only projection of posted custody events/movements.

Relationship:

- should support balance calculation;
- should not be the user-facing domain language;
- should never be the only evidence of custody.

### Cash Reconciliation

The act of comparing expected custody with actual counted/reported custody.

Relationship:

- proves custody when balanced;
- creates or references discrepancy when not balanced;
- is per account/session.

### Cash Discrepancy

Evidence that the custody chain does not fully explain reality.

Relationship:

- discrepancy is not just a math difference;
- it is a broken or incomplete custody proof;
- it remains visible until explained/resolved.

## Is Transfer Two Movements Or Custody Transfer?

Business answer:

> A transfer is a transfer of custody.

Implementation answer:

> A transfer should produce two linked movements.

This distinction should be explicit.

Example:

- Transfer 300 EUR from Main Cash Box to Employee Float.

Domain event:

- Custody transferred from `Main Cash Box` to `Employee Z`.

Ledger consequences:

- Main Cash Box: outflow 300.
- Employee Float: inflow 300.

Company-level consequence:

- net zero.

Risk if modeled as only two movements:

- one side can be missing;
- the business intent is lost;
- reversal is ambiguous;
- audit trail does not prove custody transfer.

Risk if modeled as only one abstract transfer:

- per-account balances become harder to derive;
- reconciliation by account becomes weaker;
- reporting needs projections anyway.

Recommended model:

- `CashCustodyTransfer` or `CashTransfer` is the business object;
- it owns/links two posted movement facts;
- both sides share a `TransferId`;
- creation is atomic;
- reversal creates an inverse linked transfer, not deletion.

## Can Every Movement Be Expressed As Custody Change?

Most cash events can be expressed as custody changes.

Examples:

- Customer payment received: custody moves from customer to company-controlled holder.
- Supplier payment: custody moves from company-controlled holder to supplier.
- Employee float assigned: custody moves from main cash box to employee.
- Employee returns cash: custody moves from employee to main cash box.
- Bank deposit: custody moves from physical cash holder to bank.
- Owner deposit: custody moves from owner to company account.
- Owner withdrawal: custody moves from company account to owner.
- Correction: custody proof is amended by reversal plus replacement.
- Reconciliation: custody is proven, disputed, explained, or resolved.

Important nuance:

Some accounting-like events do not represent physical custody transfer. Bank import corrections, fees, or settlement adjustments may update an account without a human handler. They are still custody events because responsibility or proof state changes, but they may not require `CashHandler`.

## Ubiquitous Language

Recommended product language:

| Current/technical term | Recommended domain term | Decision |
| --- | --- | --- |
| Cash Integrity | Cash Custody | Use Cash Custody as long-term pillar; keep Cash Integrity as capability. |
| CashMovement | Custody Event / Cash Movement | Keep `CashMovement` for ledger/event implementation; use Custody Event in product language when responsibility changes. |
| CashTransfer | Custody Transfer | Prefer Custody Transfer in product language; `CashTransfer` is acceptable in code if clearer for developers. |
| EmployeeCashHandler | Cash Handler | Keep employee-specific field where applicable; product term is Cash Handler. |
| EnteredBy | Recorded By | Prefer Recorded By in UI/product copy. |
| ApprovedBy | Approved By | Keep. |
| ReconciledBy | Reconciled By | Keep. |
| CashAccount | Cash Source / Cash Account | Product can say Cash Source; domain can keep Cash Account. |
| CashLedger | Custody Ledger / Cash Ledger | Ledger is a projection; not primary user-facing language. |
| Discrepancy | Custody Gap / Discrepancy | Keep Discrepancy; optionally explain as a custody gap. |

Naming decision:

- Use `Cash Custody` for positioning and product pillar.
- Use `Cash Account` in domain/implementation because it is clear and concrete.
- Use `Custody Holder` for any person/account/institution currently responsible.
- Use `Custody Event` for the generic event family.
- Use `Cash Movement` for balance-affecting posted facts.
- Use `Custody Transfer` for responsibility transfer between holders.

Avoid:

- `CashPossession`: too physical; bank and POS transit are not possession.
- `CashResponsible`: awkward noun.
- `CashHolder`: useful but less precise than Custody Holder.
- `MoneyOwner`: wrong for employees; the company owns the money.

## Lifecycle

Recommended custody lifecycle:

1. `Draft`
   - Event is being recorded.
   - No expected balance effect.

2. `Posted`
   - Custody event is accepted.
   - Expected balance/custody state changes.
   - Event becomes immutable.

3. `InCustody`
   - A holder/account is currently responsible for money.
   - This can be an implied state derived from posted events.

4. `Transferred`
   - Responsibility moved to another holder/account.
   - Original custody obligation is reduced or closed.

5. `Spent`
   - Money left company custody for a business reason.

6. `Returned`
   - Money returned from employee/external temporary custody to a company cash account.

7. `Reconciled`
   - Expected custody matched actual custody.

8. `Discrepant`
   - Expected custody did not match actual custody.
   - Chain remains open/problematic.

9. `Explained`
   - Difference has an explanation but may not be financially corrected.

10. `Resolved`
    - Difference has been corrected, accepted, or otherwise closed under policy.

11. `Archived`
    - Historical chain is closed for operational purposes and remains auditable.

Implementation note:

Do not force all these as status values on one entity in MVP. Some are event types or derived states. For example, `InCustody` may be derived from current open obligations rather than stored directly.

## Invariants

Core custody invariants:

1. Every custody event is tenant-scoped.

2. Every posted custody event has a business reason/category.

3. Every posted custody event identifies the company cash source or custody holder affected.

4. Physical cash handling requires a cash handler.

5. Recorded By is always required.

6. Approval is required for reversal, correction, reconciliation close, and discrepancy resolution.

7. Posted custody events cannot be edited or deleted.

8. Corrections use reversal plus replacement.

9. Responsibility cannot disappear silently.

10. Cash cannot become orphaned.

11. A custody transfer has exactly one source holder and one destination holder.

12. Transfer source and destination cannot be the same.

13. Transfer source and destination must be in the same tenant.

14. Same-currency transfers must net to zero at company level.

15. Every transfer side must be linked by one transfer id.

16. Reversal of a transfer reverses both sides.

17. Account/session expected balance is derived from posted movement facts.

18. Reconciliation is per cash account/source and business session.

19. Balanced reconciliation requires actual minus expected equals zero.

20. Discrepancy is a broken or incomplete custody chain and must remain visible until explained or resolved.

21. Historical custody records remain visible after account deactivation or employee status changes.

22. Tenant A cannot see or alter Tenant B custody records.

23. Unauthorized users cannot create, transfer, reverse, correct, reconcile, or resolve custody.

## Domain Model

Recommended conceptual model:

```text
Company/Tenant
  owns many Cash Accounts
  owns many Custody Holders

Cash Account
  is a Custody Holder
  has sessions
  has posted movement facts
  has expected balance projections

Custody Holder
  can be Cash Account
  can be Employee
  can be Bank
  can be Customer/Supplier boundary actor

Custody Event
  belongs to Tenant
  has reason/category
  has recorded-by
  may have approved-by
  may have physical handler
  changes custody state
  may create one or more Cash Movements

Custody Transfer
  is a Custody Event
  has source holder
  has destination holder
  has amount/currency
  creates linked source/destination movements

Cash Session
  proves one Cash Account over a business period
  calculates expected balance
  closes through reconciliation

Cash Reconciliation
  compares expected vs actual
  proves or challenges custody
  may create discrepancy

Cash Discrepancy
  marks unresolved custody gap
  requires explanation/resolution workflow
```

Potential implementation records later:

- `CashCustodyHolder`
- `CashCustodyEvent`
- `CashCustodyTransfer`
- `CashCustodyObligation`
- `CashCustodyChain`

MVP can avoid all of those names in code if that creates churn. It must still preserve the semantics.

## Migration Impact

### From Current Runtime Model

Current `CashBalance` and `CashLedger` are balance-centric.

Migration direction:

1. Create default Cash Account per company.
2. Link historical `CashLedger` entries to the default account.
3. Treat existing ledger rows as historical movement facts with limited custody semantics.
4. Mark historical rows as `LegacyImported` or equivalent in migration documentation/projections.
5. Do not claim old rows can prove handler/approver/custody if the data was never captured.

### From Proposed Cash Integrity Contracts

The proposed contracts need conceptual adjustment, but not wholesale rejection.

Recommended adjustments:

- keep `CashAccount`;
- keep `CashMovement`;
- keep `CashSession`;
- keep `CashReconciliation`;
- keep `CashDiscrepancy`;
- add transfer concept before persistence;
- add custody language to documentation and API semantics;
- add holder/source/destination vocabulary;
- clarify `EmployeeCashHandlerId` is not the same as current custodian in every case;
- consider adding `CustodyHolderType` and holder reference later;
- treat `CashMovement` as one event fact within a broader custody chain.

## Compatibility Impact

Public product language should evolve carefully:

- Do not abruptly remove Cash Integrity references.
- Introduce Cash Custody as the stronger umbrella.
- Explain Cash Integrity as the measurable capability of the Cash Custody model.

Suggested wording:

> Cash Custody is the chain of responsibility for company money. Cash Integrity is the proof that the chain is complete, reconciled, and explainable.

API compatibility:

- existing payment/cash endpoints should not be broken by analysis;
- future endpoints can use `/cash/accounts`, `/cash/movements`, `/cash/transfers`, `/cash/reconciliations`;
- product docs can describe these as custody workflows even if endpoint names stay cash-oriented.

Data compatibility:

- old company-level balance should be migrated into default account;
- old ledger rows should remain auditable but labeled as limited legacy evidence;
- no silent rewriting of history.

## Product Impact

### Cash Integrity

Cash Integrity becomes the proof layer of Cash Custody.

It answers:

- Does expected match actual?
- Can every discrepancy be explained?
- Are movements immutable and auditable?

### Safe-to-Pay

Safe-to-Pay becomes stronger if based on custody:

- money in employee float is not equally safe as money in bank;
- unreconciled cash should reduce confidence;
- open discrepancies should affect safe cash;
- custody uncertainty becomes an explicit risk signal.

### Forecast

Forecast should separate:

- expected future cash events;
- actual custody state;
- confidence level based on reconciliation quality.

Without custody, forecast can look precise but be operationally untrusted.

### Cash Visibility

Cash visibility improves from "balance by account" to:

- balance by account;
- cash currently held by employees;
- unreconciled sessions;
- open discrepancies;
- transfers in transit;
- custody obligations due back.

### Weekly Review

The weekly review should show:

- cash sources;
- unreconciled custody;
- employee-held cash;
- open discrepancies;
- upcoming commitments;
- safe cash confidence.

### Owner Dashboard

The dashboard should not only show "cash balance." It should show:

- cash available;
- cash committed;
- cash held by employees;
- cash in transit;
- cash not yet reconciled;
- discrepancies requiring action.

## Risks

### Over-Modeling Risk

Custody can become too complex if the system tries to track every euro as an object.

Mitigation:

- track custody at transaction/batch level;
- keep MVP focused on operational cash proof;
- avoid accounting-grade subledger ambitions.

### Naming Churn Risk

Renaming all code from Cash Movement to Cash Custody too early would create churn without product value.

Mitigation:

- change product language first;
- add custody concepts only where they clarify behavior;
- keep stable technical names where they remain accurate.

### ERP Drift Risk

Custody could invite broad inventory/procurement/accounting workflows.

Mitigation:

- model responsibility for money only;
- reject stock, payroll, CRM, full accounts payable, and full bank accounting workflows.

### Migration Evidence Risk

Historical cash ledger rows lack handler/source/approver data.

Mitigation:

- preserve them as legacy ledger evidence;
- do not backfill fake custody facts;
- start full custody proof from migration date.

### User Friction Risk

Custody workflows can become too bureaucratic for small businesses.

Mitigation:

- ordinary entry remains fast;
- stronger proof is required at transfer, correction, reconciliation and discrepancy resolution;
- UI emphasizes "who has it and why" rather than compliance jargon.

## Recommended Implementation Order

Do not start with database tables.

Recommended sequence:

1. Product language update
   - update decision record and product principles to introduce Cash Custody as umbrella language.

2. Contract adjustment analysis
   - add transfer/custody holder concepts to the planned Cash Integrity contracts before persistence.

3. Domain contracts
   - add `CashTransfer` or `CashCustodyTransfer`;
   - add holder/source/destination semantics;
   - clarify handler vs custodian vs recorded-by.

4. Use cases
   - create custody movement;
   - transfer custody;
   - reverse/correct custody event;
   - open/close/reconcile account session;
   - explain discrepancy.

5. Persistence schema
   - cash accounts;
   - movements;
   - transfer links;
   - sessions;
   - reconciliations;
   - discrepancies;
   - audit.

6. API contracts
   - account list/detail;
   - movement entry;
   - transfer;
   - reconciliation;
   - discrepancy.

7. WebApp workflows
   - owner cash overview;
   - employee custody entry;
   - transfer between accounts/holders;
   - daily reconciliation;
   - discrepancy action queue.

8. Migration
   - create default account per company;
   - link legacy ledger to default account;
   - preserve legacy limitations.

9. Safe-to-Pay integration
   - treat unreconciled custody and discrepancies as confidence reducers.

## Should Cash Integrity Become Cash Custody?

Decision:

Yes, at the product/domain level.

Cash Integrity should evolve into Cash Custody as the broader product pillar.

Precise relationship:

- Cash Custody is the business domain: who is responsible for company money and how responsibility changes.
- Cash Integrity is the proof quality: whether the custody chain is complete, immutable, reconciled, and explainable.

Why Cash Custody is stronger:

- it speaks to business risk, not just data correctness;
- it captures human responsibility;
- it explains employee floats, cash boxes, bank deposits, transfers and discrepancies in one language;
- it supports emotional value for owners: "I know where the money is and who is responsible";
- it creates a sharper identity than generic cash ledger/reporting.

Why not replace all technical names immediately:

- `CashMovement` remains useful for balance-affecting facts;
- `CashAccount` remains clear;
- `CashSession` and `CashReconciliation` remain clear;
- a full rename would add churn before behavior exists.

Recommendation:

Adopt Cash Custody in product documents and future design. Keep Cash Movement as a lower-level concept inside the custody model.

## If OpenCashFlow Disappeared Tomorrow

Would "Cash Custody" be a stronger long-term identity than "Cash Integrity"?

Yes.

Business reasoning:

Cash Integrity sounds like an internal quality promise. It says the data is correct. That matters, but it does not immediately describe the owner's pain.

Cash Custody describes the lived problem:

- employees receive money;
- cash moves through boxes, floats, banks and suppliers;
- owners worry about missing cash;
- managers need to know who is responsible;
- discrepancies create stress and conflict;
- weekly cash decisions depend on trust.

If OpenCashFlow became known for one thing, "it proves who has the money and why" is more memorable than "it has accurate cash movements." The first is a business outcome. The second is a system property.

The strongest long-term identity is:

> OpenCashFlow is the cash cockpit that proves custody of company money from receipt to reconciliation.

That identity is narrower than ERP, stronger than generic cash tracking, and directly aligned with the weekly cash ritual.
