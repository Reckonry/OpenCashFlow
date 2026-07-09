# Feature Filter

OpenCashFlow should reject good ideas that weaken the product identity.

The product is:

> the self-hosted cash cockpit for small businesses.

Every proposed feature must pass this filter.

## The Six Questions

## 1. Does It Improve Cash Visibility?

Accept if the feature helps users see:

- current cash;
- expected cash;
- committed cash;
- overdue cash;
- safe cash;
- cash runway;
- cash risk.

Reject if it only adds generic business data.

## 2. Does It Improve Decisions?

Accept if the feature helps users decide:

- what to pay;
- what to delay;
- what to chase;
- what to reserve;
- what to buy;
- what not to buy.

Reject if it creates reports without decisions.

## 3. Does It Reduce Uncertainty?

Accept if the feature makes assumptions visible:

- payment confidence;
- expected date;
- source;
- scenario;
- owner;
- status.

Reject if it adds numbers nobody can trust or explain.

## 4. Does It Help The Weekly Workflow?

Accept if the feature makes the weekly cash ritual faster, clearer, or more complete.

Reject if it is useful only once during setup or belongs in administration.

## 5. Does It Integrate With Existing Systems?

Accept if the feature helps OpenCashFlow work with:

- bank exports;
- spreadsheets;
- accounting systems;
- invoicing tools;
- ERPs;
- accountant workflows.

Reject if it tries to replace those systems without a compelling cash reason.

## 6. Does It Strengthen Cash Custody And Integrity?

Accept if the feature makes responsibility for company money more provable:

- source/origin;
- custodian or destination;
- cash source;
- employee cash handler;
- reason/category;
- actor;
- audit trail;
- expected vs actual cash;
- discrepancy status.

Reject or redesign if it creates cash numbers without a custody chain that can be reconciled and explained.

## ERP Drift Test

Ask:

> Are we building this because cash decisions require it, or because ERPs usually have it?

If the answer is "because ERPs usually have it", reject.

## Default Rejection List

Reject from core unless a later product decision overrides:

- full CRM;
- full inventory;
- full MRP;
- payroll;
- ecommerce;
- POS;
- HR suite;
- tax compliance engine;
- certified accounting;
- project management;
- help desk;
- marketing automation.

## Acceptable Core Areas

Accept in core when scoped tightly:

- payments;
- cash ledger;
- cash forecast;
- receivables/payables lite;
- commitments;
- scenarios;
- alerts;
- owner dashboard;
- accountant summary;
- imports/exports;
- audit.

## Feature Scoring

Score each proposal 0-2:

| Question | 0 | 1 | 2 |
| --- | --- | --- | --- |
| Cash visibility | No effect | Indirect | Direct |
| Decisions | No decision | Informs decision | Drives action |
| Uncertainty | Adds ambiguity | Neutral | Clarifies assumptions |
| Weekly workflow | No role | Occasional | Weekly habit |
| Integration | Isolated | Export/import later | Connects existing system |
| Cash custody/integrity | Weakens proof | Neutral | Strengthens proof |

Decision:

- 10-12: strong candidate;
- 7-9: consider if small;
- 0-4: reject or move outside core.

## Product Rule

When in doubt, choose the feature that helps an owner decide what to do this week.

Cash features must pass one extra rule:

> If the custody chain cannot be reconciled, do not use the number as trusted cash.
