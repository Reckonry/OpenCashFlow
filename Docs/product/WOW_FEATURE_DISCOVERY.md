# Wow Feature Discovery

Date: 2026-07-08

Role: Product Guardian.

Mission:

> Find one feature that makes a small business owner say: "I need this."

Constraints:

- must fit the Cash Cockpit vision;
- must not become ERP;
- must not become accounting;
- must not become CRM;
- must not require hundreds of screens;
- must create weekly habit;
- must create emotional value;
- must solve an expensive problem.

Product filter applied:

- `PRODUCT_MANIFESTO.md`
- `PRODUCT_PRINCIPLES.md`
- `FEATURE_FILTER.md`
- `PRODUCT_DECISIONS.md`

## The Standard

A "wow" feature for OpenCashFlow is not a clever widget.

It is a feature that answers a painful owner question:

> Can I safely pay this, or will it create a cash problem later?

The feature must create relief.

It must turn vague anxiety into a clear action.

## Candidate Features

## 1. Safe-to-Pay Radar

Problem solved:

Owners approve payments from bank balance, memory, and fear. They need to know whether paying a supplier today will
endanger payroll, taxes, rent, or another commitment later.

Who benefits:

Owners, office managers, accountants, workshop managers, distributors, consultants managing contractors.

Why competitors do not solve it well:

ERPs and accounting systems can show payables and reports, but they usually do not frame the decision as "safe to pay
this week?" with expected cash, reserves, late customers, and scenario impact in one owner-facing view.

Development complexity:

Medium. Requires commitments, expected receivables, reserves, forecast logic, and action workflow. Does not require full
accounting or ERP.

Business value:

Very high. Avoiding one bad payment decision can protect payroll, supplier relationships, and owner confidence.

Adoption impact:

Very high. The pain is immediate and easy to demonstrate.

Frequency of use:

Weekly, often daily in cash-tight businesses.

Competitive advantage:

Strong. It is a decision feature, not a recordkeeping feature.

## 2. Cash Collision Calendar

Problem solved:

Cash crises happen when inflows and outflows collide on the same dates.

Who benefits:

Small manufacturers, workshops, distributors, accountants.

Why competitors do not solve it well:

Accounting calendars exist, but they rarely combine confidence-weighted receivables, supplier commitments, payroll,
taxes, and forecast low points in a simple operating view.

Development complexity:

Medium.

Business value:

Very high.

Adoption impact:

High.

Frequency of use:

Weekly.

Competitive advantage:

Strong, especially if visual and scenario-based.

## 3. Late Customer Risk Meter

Problem solved:

One late customer can create a cash shortfall, but owners often see the lateness before they see the downstream damage.

Who benefits:

Businesses with concentrated receivables.

Why competitors do not solve it well:

Invoicing systems show overdue invoices; they do not always show what the delay will break.

Development complexity:

Low-medium.

Business value:

High.

Adoption impact:

High for service, manufacturing, and distribution businesses.

Frequency of use:

Weekly.

Competitive advantage:

Good, but narrower than Safe-to-Pay Radar.

## 4. Payroll Protection Reserve

Problem solved:

Owners accidentally treat payroll cash as spendable cash.

Who benefits:

Any company with employees or contractors.

Why competitors do not solve it well:

Payroll systems calculate payroll; accounting systems record it. Neither necessarily protects payroll cash inside weekly
payment decisions.

Development complexity:

Low-medium.

Business value:

High.

Adoption impact:

High emotional value.

Frequency of use:

Weekly or per payroll cycle.

Competitive advantage:

Good as a component, not strong enough as the flagship.

## 5. Tax/VAT Cash Reserve

Problem solved:

Businesses spend cash that should be reserved for tax.

Who benefits:

Small companies, consultants, accounting firms.

Why competitors do not solve it well:

Accounting systems can calculate liabilities, but owners need an operational reserve visible in cash decisions.

Development complexity:

Medium because tax differs by country. MVP can use manual reserve rules.

Business value:

High.

Adoption impact:

Medium-high.

Frequency of use:

Weekly/monthly.

Competitive advantage:

Good if implemented as cash reserve, not tax engine.

## 6. Customer Promise Tracker

Problem solved:

Owners track payment promises in email, memory, and notes.

Who benefits:

Anyone with receivables.

Why competitors do not solve it well:

CRMs track interactions; invoicing tools track due dates. Neither turns payment promises into forecast confidence.

Development complexity:

Low.

Business value:

High.

Adoption impact:

Medium-high.

Frequency of use:

Weekly.

Competitive advantage:

Moderate. Needs connection to cash forecast to become powerful.

## 7. Cash Confidence Score

Problem solved:

Expected cash is not equal to reliable cash.

Who benefits:

Owners and accountants.

Why competitors do not solve it well:

Many systems show due amounts but do not distinguish confirmed, expected, risky, or hypothetical cash.

Development complexity:

Low-medium.

Business value:

High.

Adoption impact:

Medium.

Frequency of use:

Weekly.

Competitive advantage:

Strong as a supporting model.

## 8. Weekly Cash Review

Problem solved:

Owners lack a repeatable cash meeting workflow.

Who benefits:

Owner-led businesses and accounting firms.

Why competitors do not solve it well:

Most tools provide screens. They do not prescribe a weekly operating ritual.

Development complexity:

Medium.

Business value:

High.

Adoption impact:

High if it becomes habit.

Frequency of use:

Weekly.

Competitive advantage:

Strong, but it is a workflow wrapper. Needs a sharp core decision feature.

## 9. Supplier Pressure Board

Problem solved:

Owners need to know which suppliers can be paid, delayed, split, or negotiated with.

Who benefits:

Manufacturers, workshops, distributors.

Why competitors do not solve it well:

Payables lists do not usually combine supplier relationship risk with safe cash and forecast impact.

Development complexity:

Medium.

Business value:

High.

Adoption impact:

Medium-high.

Frequency of use:

Weekly.

Competitive advantage:

Good for manufacturing/distribution.

## 10. Scenario Snapshots

Problem solved:

Owners need to test "what if customer pays late?" or "what if we delay purchase?" without building a spreadsheet.

Who benefits:

Owners, CFO consultants, accountants.

Why competitors do not solve it well:

ERPs can report; spreadsheets can model; few lightweight self-hosted tools make scenarios part of weekly cash decisions.

Development complexity:

Medium.

Business value:

High.

Adoption impact:

High.

Frequency of use:

Weekly/monthly.

Competitive advantage:

Strong as part of Safe-to-Pay Radar.

## 11. Cash Runway Forecast

Problem solved:

Owners need to know how many days of cash they have under expected and worst-case assumptions.

Who benefits:

All cash-sensitive companies.

Why competitors do not solve it well:

Accounting reports are backward-looking; runway is operational and forward-looking.

Development complexity:

Medium.

Business value:

High.

Adoption impact:

High.

Frequency of use:

Weekly.

Competitive advantage:

Moderate by itself; strong inside dashboard.

## 12. Accountant Cash Summary

Problem solved:

Accountants see books; owners need cash advice. The summary bridges the conversation.

Who benefits:

Accounting firms, owners.

Why competitors do not solve it well:

Accounting software is accountant-centered; ERP is operations-centered. This is advisor conversation-centered.

Development complexity:

Low-medium.

Business value:

Medium-high.

Adoption impact:

High through accounting firms.

Frequency of use:

Weekly/monthly.

Competitive advantage:

Good distribution advantage.

## 13. Bank CSV Match Queue

Problem solved:

Manual entry kills trust and adoption.

Who benefits:

All users.

Why competitors do not solve it well:

Many competitors have bank sync, but self-hosted/local setups often struggle with practical import workflows.

Development complexity:

Medium.

Business value:

High.

Adoption impact:

High.

Frequency of use:

Weekly.

Competitive advantage:

Necessary but not emotionally differentiated.

## 14. "What Changed Since Last Week?"

Problem solved:

Owners need to know why the cash forecast changed.

Who benefits:

Owners and advisors.

Why competitors do not solve it well:

Most systems show current state, not changes in assumptions.

Development complexity:

Medium.

Business value:

Medium-high.

Adoption impact:

Medium.

Frequency of use:

Weekly.

Competitive advantage:

Strong for trust, not enough as the flagship.

## 15. Purchase Delay Advisor

Problem solved:

Owners need to decide whether to buy equipment/materials now or wait.

Who benefits:

Workshops, manufacturers, distributors.

Why competitors do not solve it well:

Inventory/ERP systems manage purchases; they rarely tell a small owner the cash safety impact in plain language.

Development complexity:

Medium.

Business value:

High.

Adoption impact:

Medium-high in target niche.

Frequency of use:

Occasional/weekly.

Competitive advantage:

Good niche advantage.

## 16. Minimum Cash Buffer Guard

Problem solved:

Owners need a line they should not cross.

Who benefits:

All small businesses.

Why competitors do not solve it well:

Bank balance shows absolute cash; it does not separate cash that must remain protected.

Development complexity:

Low.

Business value:

High.

Adoption impact:

Medium.

Frequency of use:

Weekly/daily.

Competitive advantage:

Useful but simple; best as part of Safe-to-Pay Radar.

## 17. Critical Week Finder

Problem solved:

Owners need to know which future week is the most dangerous.

Who benefits:

Cash-constrained businesses.

Why competitors do not solve it well:

Reports show ranges; they often do not highlight the week that will hurt.

Development complexity:

Medium.

Business value:

High.

Adoption impact:

High.

Frequency of use:

Weekly.

Competitive advantage:

Strong as part of forecast UX.

## 18. Cash Decision Log

Problem solved:

Owners forget why they delayed or approved payments.

Who benefits:

Owners, managers, accountants.

Why competitors do not solve it well:

Accounting records transactions, not decision rationale.

Development complexity:

Low-medium.

Business value:

Medium.

Adoption impact:

Medium.

Frequency of use:

Weekly.

Competitive advantage:

Good trust feature, not a wow feature alone.

## 19. Customer Concentration Warning

Problem solved:

One customer can dominate cash risk.

Who benefits:

Small B2B companies with a few large customers.

Why competitors do not solve it well:

CRM/accounting tools may show revenue concentration; they often do not show cash timing concentration.

Development complexity:

Medium.

Business value:

High.

Adoption impact:

Medium.

Frequency of use:

Monthly/weekly.

Competitive advantage:

Good analytic feature, not universal enough.

## 20. Cash Stress Test

Problem solved:

Owners need to know whether the business survives common shocks.

Who benefits:

Owners, advisors, lenders.

Why competitors do not solve it well:

Small-business tools rarely offer simple operational stress tests.

Development complexity:

Medium-high.

Business value:

High.

Adoption impact:

Medium-high.

Frequency of use:

Monthly/quarterly.

Competitive advantage:

Strong for advisors, but less weekly than Safe-to-Pay Radar.

## 21. Payment Split Planner

Problem solved:

Owners often need to split supplier payments without losing sight of cash impact.

Who benefits:

Workshops, distributors, companies with supplier pressure.

Why competitors do not solve it well:

Payables tools track payment terms; they do not usually model negotiation options against forecast risk.

Development complexity:

Medium.

Business value:

High in cash-tight businesses.

Adoption impact:

Medium.

Frequency of use:

Weekly.

Competitive advantage:

Useful inside Safe-to-Pay Radar.

## 22. Cash Forecast Confidence Timeline

Problem solved:

Forecasts feel fake when assumptions are hidden.

Who benefits:

Owners and advisors.

Why competitors do not solve it well:

Forecasts are often presented as precise. Small businesses need confidence-aware forecasts.

Development complexity:

Medium.

Business value:

Medium-high.

Adoption impact:

Medium.

Frequency of use:

Weekly.

Competitive advantage:

Good trust layer.

## Ranking

| Rank | Candidate | Why |
| ---: | --- | --- |
| 1 | Safe-to-Pay Radar | Directly answers the owner's most urgent cash question and creates weekly/daily habit. |
| 2 | Cash Collision Calendar | Strong visual way to reveal future cash crises. |
| 3 | Scenario Snapshots | Converts anxiety into decisions. |
| 4 | Late Customer Risk Meter | Expensive, emotional, easy to understand. |
| 5 | Weekly Cash Review | Important habit wrapper, but needs a core decision engine. |
| 6 | Cash Runway Forecast | High value, but common enough to need stronger framing. |
| 7 | Supplier Pressure Board | Strong in workshops/distribution, less universal. |
| 8 | Payroll Protection Reserve | Emotional and valuable, but narrower. |
| 9 | Critical Week Finder | Excellent insight, best as part of the radar. |
| 10 | Bank CSV Match Queue | Necessary adoption feature, not the emotional hook. |
| 11 | Tax/VAT Cash Reserve | Valuable but country-sensitive. |
| 12 | Cash Confidence Score | Strong model, not a standalone feature. |
| 13 | Customer Promise Tracker | Useful input, not the whole product. |
| 14 | Payment Split Planner | Strong supporting action. |
| 15 | Cash Stress Test | Powerful for advisors, less weekly. |
| 16 | Accountant Cash Summary | Distribution feature, not the owner wow moment. |
| 17 | Purchase Delay Advisor | Valuable niche feature. |
| 18 | Minimum Cash Buffer Guard | Simple and useful, not enough alone. |
| 19 | What Changed Since Last Week? | Trust feature, not first-order pain. |
| 20 | Customer Concentration Warning | Important but less frequent. |
| 21 | Cash Decision Log | Supports accountability, not acquisition. |
| 22 | Cash Forecast Confidence Timeline | Good sophistication, not immediate pain. |

## Chosen Feature

## Safe-to-Pay Radar

Single strongest feature:

> Safe-to-Pay Radar tells a business owner what can be paid this week without breaking payroll, taxes, supplier
> commitments, reserves, or future cash runway.

This is the feature most likely to make a small business owner say:

> I need this.

## Why This Wins

It is not a report.

It is not another dashboard.

It answers a stressful decision:

> Can I pay this?

The owner does not need more data. The owner needs confidence.

Safe-to-Pay Radar creates that confidence by combining:

- current cash;
- committed cash;
- protected reserves;
- expected receivables;
- overdue receivables;
- due payables;
- confidence levels;
- scenario outcomes;
- recommended actions.

## Product Guardian Evaluation

Does it strengthen the Cash Cockpit vision?

Yes. It is the clearest expression of cash cockpit: cash position plus decision.

Does it improve weekly cash decisions?

Yes. It becomes the weekly payment approval surface.

Does it improve cash visibility?

Yes. It separates bank cash, safe cash, committed cash, protected cash, and risky expected cash.

Does it reduce uncertainty?

Yes. It exposes assumptions, confidence levels, and future collisions.

Does it move OpenCashFlow toward ERP?

No, if scoped correctly. It does not require inventory, CRM, accounting, payroll calculation, or purchasing modules. It
only models cash commitments and decision outcomes.

## UX Design

## Primary Screen: Safe-to-Pay Radar

Purpose:

The owner opens this screen before approving payments.

Top-level question:

> What can we safely pay this week?

Primary sections:

1. Safe Cash Summary.
2. Payment Decision Queue.
3. Cash Collision Timeline.
4. Risk Explanation.
5. Scenario Drawer.
6. Weekly Action Summary.

## UX Principle

The screen must never ask the owner to interpret raw finance.

It should say:

- safe;
- risky;
- unsafe;
- why;
- what to do next.

## Workflow

## 1. Open Weekly Review

The owner starts the Monday review.

OpenCashFlow shows:

- bank cash;
- safe cash;
- committed cash;
- protected reserves;
- expected cash;
- risky expected cash.

## 2. Review Payment Queue

The owner sees payments due this week:

- supplier;
- amount;
- due date;
- status;
- impact if paid;
- recommendation.

Recommendations:

- Safe to pay.
- Pay only if receivable lands.
- Split recommended.
- Delay recommended.
- Unsafe: breaks reserve.

## 3. Inspect A Payment

Click a payment.

Show:

- current cash impact;
- forecast after payment;
- affected future commitments;
- protected reserve impact;
- late receivables dependency;
- scenarios.

## 4. Run Scenario

Scenario options:

- customer pays 7 days late;
- customer pays 15 days late;
- split supplier payment;
- delay payment;
- reserve tax first;
- delay purchase.

## 5. Decide

Possible decisions:

- approve;
- delay;
- split;
- hold pending customer;
- mark as critical;
- negotiate;
- reject.

Each decision updates the forecast and decision log.

## 6. Share Summary

The owner exports or shares:

- payments approved;
- payments delayed;
- cash risk;
- customer follow-ups;
- accountant questions.

## Conceptual Data Model

This is product data, not implementation.

## CashAccount

Represents a bank or cash source.

Fields:

- `CashAccountId`
- `Name`
- `CurrentBalance`
- `LastUpdatedAt`
- `Source`

## CashReserve

Represents protected cash.

Examples:

- payroll reserve;
- tax reserve;
- rent reserve;
- minimum buffer.

Fields:

- `CashReserveId`
- `Name`
- `Amount`
- `Priority`
- `ProtectedUntil`
- `Reason`

## ExpectedCashIn

Represents money expected to arrive.

Fields:

- `ExpectedCashInId`
- `Counterparty`
- `Amount`
- `ExpectedDate`
- `OriginalDueDate`
- `Confidence`
- `Status`
- `Source`
- `Notes`

## CashCommitment

Represents money expected or required to leave.

Fields:

- `CashCommitmentId`
- `Payee`
- `Amount`
- `DueDate`
- `Category`
- `Criticality`
- `Deferrable`
- `Status`
- `Source`
- `Notes`

## PaymentDecision

Represents an owner decision.

Fields:

- `PaymentDecisionId`
- `CashCommitmentId`
- `Decision`
- `DecisionDate`
- `DecisionBy`
- `Reason`
- `ExpectedImpact`

Decision values:

- approve;
- delay;
- split;
- hold;
- negotiate;
- reject.

## ForecastPoint

Represents projected balance on a date.

Fields:

- `Date`
- `ExpectedBalance`
- `ConservativeBalance`
- `WorstCaseBalance`
- `SafeCash`
- `CommittedCash`

## CashCollision

Represents a future risk event.

Fields:

- `CashCollisionId`
- `Date`
- `Severity`
- `Trigger`
- `AffectedReserve`
- `ProjectedShortfall`
- `Explanation`
- `SuggestedActions`

## Scenario

Represents a temporary what-if assumption.

Fields:

- `ScenarioId`
- `Name`
- `Assumptions`
- `ProjectedLowPoint`
- `RunwayDays`
- `AffectedCommitments`

## WeeklyCashReview

Represents the weekly ritual.

Fields:

- `WeeklyCashReviewId`
- `WeekStartDate`
- `StartedBy`
- `CashAtStart`
- `SafeCashAtStart`
- `ActionsTaken`
- `Summary`
- `CompletedAt`

## Screenshots (Concept)

These are wireframe concepts, not implementation screens.

## Screen 1: Safe-to-Pay Overview

```text
+--------------------------------------------------------------+
| Safe-to-Pay Radar                         Week of Aug 5       |
+--------------------------------------------------------------+
| Bank Cash     Safe Cash     Committed     Expected     Risk   |
| 84,000        18,300        61,500        79,900       HIGH   |
+--------------------------------------------------------------+
| This week: You can safely approve 3 of 7 payments.            |
| Warning: ACME late payment can break payroll reserve Aug 12.  |
+--------------------------------------------------------------+
| Payment Queue                                                |
| Supplier           Due        Amount     Recommendation       |
| SteelCo            Aug 7      27,500     Split recommended    |
| Payroll            Aug 9      38,000     Protected            |
| Rent               Aug 10      6,000     Safe to pay          |
| Machine purchase   Aug 11     32,000     Delay recommended    |
+--------------------------------------------------------------+
```

## Screen 2: Payment Decision Detail

```text
+--------------------------------------------------------------+
| SteelCo - 27,500 due Aug 7                                   |
+--------------------------------------------------------------+
| If paid in full:                                             |
| - Safe cash drops to -9,200                                  |
| - Payroll reserve is breached                                |
| - Lowest projected balance: Aug 12                           |
+--------------------------------------------------------------+
| Recommended action: Split payment                            |
| Pay 12,000 now, 15,500 after ACME receipt                    |
+--------------------------------------------------------------+
| [Approve split] [Delay] [Run scenario] [Mark critical]        |
+--------------------------------------------------------------+
```

## Screen 3: Cash Collision Timeline

```text
Aug 5       Aug 7       Aug 9       Aug 12       Aug 18
84,000      56,500      18,500      -6,800       45,200
            SteelCo     Payroll     ACME late    ACME paid
                        reserve     collision
                        protected
```

## Screen 4: Scenario Drawer

```text
+--------------------------------------------------------------+
| Scenario: ACME pays 15 days late                             |
+--------------------------------------------------------------+
| Expected low point: -6,800                                   |
| Safe cash breach: Yes                                        |
| Payroll at risk: Yes                                         |
| Supplier at risk: SteelCo                                    |
+--------------------------------------------------------------+
| Fix options                                                  |
| 1. Split SteelCo payment: low point becomes 9,400             |
| 2. Delay machine purchase: low point becomes 18,300           |
| 3. Collect 10,000 deposit: low point becomes 3,200            |
+--------------------------------------------------------------+
```

## MVP

## MVP Goal

Answer:

> What can we safely pay this week?

## MVP Scope

Inputs:

- current cash balance;
- manual expected cash-in;
- manual cash commitments;
- manual reserves;
- confidence level;
- due dates.

Core calculations:

- safe cash;
- committed cash;
- projected daily balance;
- reserve breach;
- payment impact;
- basic recommendation.

Screens:

1. Safe-to-Pay Overview.
2. Payment Decision Detail.
3. Simple Scenario Drawer.
4. Weekly Summary.

Recommendations:

- safe to pay;
- risky;
- unsafe;
- split/delay suggested.

Out of scope:

- bank sync;
- full accounting;
- payroll calculation;
- tax filing;
- inventory;
- CRM;
- automated supplier negotiation;
- AI recommendations.

## MVP Success Criteria

The MVP works if an owner can:

1. enter cash, receivables, commitments, and reserves;
2. open the Radar on Monday;
3. see which payments are safe;
4. understand why one payment is risky;
5. run one scenario;
6. decide what to pay or delay.

## Version 2

Goal:

Reduce manual work and increase trust.

Add:

- bank CSV import;
- invoice/payables CSV import;
- match expected payments to bank movements;
- recurring commitments;
- decision log;
- accountant summary export;
- "what changed since last week";
- customer payment promise tracker.

Do not add:

- full invoicing;
- full accounting;
- supplier portal;
- inventory.

## Version 3

Goal:

Make Safe-to-Pay Radar the operating system for weekly cash decisions.

Add:

- import templates for Odoo, ERPNext, Dolibarr, Akaunting, Invoice Ninja, spreadsheets;
- advisor/accountant multi-client view;
- cash risk alerts;
- workshop/manufacturing cash pack;
- regional bank connector packs where commercially practical;
- certified cash review reports;
- scenario library.

Do not add:

- ERP module marketplace;
- payroll;
- tax compliance engine;
- broad CRM.

## Why This Feature Creates Emotional Value

Small business owners do not lose sleep because they lack another report.

They lose sleep because they are unsure whether they can pay people, suppliers, taxes, and still survive the next
customer delay.

Safe-to-Pay Radar gives them a concrete answer.

It replaces:

- "I think we are okay"

with:

- "We can pay rent and payroll. We should split SteelCo. Do not buy the machine until ACME pays."

That is emotional value.

## Why This Solves An Expensive Problem

Bad cash decisions create expensive consequences:

- payroll stress;
- supplier holds;
- late fees;
- emergency borrowing;
- missed discounts;
- owner panic;
- damaged trust;
- bad purchases;
- tax surprises.

If Safe-to-Pay Radar prevents even one avoidable cash crunch, it can justify the product.

## Why Competitors Will Not Easily Own This

Accounting software is ledger-first.

ERP is process-first.

Invoicing software is billing-first.

Personal finance software is budget-first.

OpenCashFlow can be decision-first.

Safe-to-Pay Radar is not a module bolted onto an ERP. It is the product's central job.

## Could This Make OpenCashFlow Famous?

Yes.

If OpenCashFlow became famous, Safe-to-Pay Radar could be the reason.

The phrase is concrete:

> It tells me what I can safely pay.

That is stronger than:

- cash-flow dashboard;
- financial report;
- budget view;
- payment list;
- forecast chart.

The feature is memorable because it maps directly to owner anxiety.

## If Not This, What Would Be Better?

The only stronger alternative would be an even sharper version:

> Pay This Week

A screen that simply says:

- Pay these.
- Delay these.
- Call these customers.
- Reserve this cash.
- Do not buy this yet.

That may become the final UX name.

But the underlying feature remains Safe-to-Pay Radar.

## Final Decision

Build:

> Safe-to-Pay Radar

Position it as:

> The weekly answer to "what can we safely pay?"

Reject any feature that distracts from this until the Radar is useful, trusted, and habit-forming.
