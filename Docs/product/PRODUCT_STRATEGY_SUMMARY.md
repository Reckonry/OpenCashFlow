# Product Strategy Summary

Date: 2026-07-08

This summary consolidates the product positioning sprint.

## Documents Created

- `Docs/product/PRODUCT_MANIFESTO.md`
- `Docs/product/POSITIONING.md`
- `Docs/product/WEEKLY_CASH_RITUAL.md`
- `Docs/product/PRODUCT_PRINCIPLES.md`
- `Docs/product/COMPETITOR_POSITIONING.md`
- `Docs/product/PRODUCT_ROADMAP.md`
- `Docs/product/FEATURE_FILTER.md`
- `Docs/product/OWNER_DASHBOARD.md`
- `Docs/product/FIRST_DEMO.md`
- `Docs/product/PRODUCT_DECISIONS.md`
- `Docs/product/PRODUCT_STRATEGY_SUMMARY.md`
- `Docs/product/CASH_CUSTODY_DECISION_RECORD.md`

## Core Strategic Decision

OpenCashFlow should not become another ERP.

OpenCashFlow should become:

> the self-hosted cash cockpit for small businesses.

The product should focus on weekly operational cash decisions:

- what cash is real;
- what cash is expected;
- what cash is committed;
- what is late;
- what can break;
- what decisions must be made this week.

Cash Custody is now the product/domain pillar underneath trusted cash:

> OpenCashFlow should prove who or what is responsible for company money from receipt to reconciliation.

Cash Integrity remains the proof capability that makes the custody chain complete, immutable, reconciled, and explainable.

## Key Strategic Decisions

1. Position around cash operations, not generic business management.
2. Treat accounting systems and ERPs as integration sources, not enemies to replace.
3. Build the weekly cash ritual as the primary habit.
4. Make the owner dashboard the primary product surface.
5. Target workshops, small manufacturers, distributors, accounting firms, and owner-led companies first.
6. Reject features that create ERP drift.
7. Treat Cash Custody as a gate before trusted Safe-to-Pay recommendations.
8. Prioritize forecast calendar, receivables/payables lite, bank import, risk alerts, and scenarios.
9. Monetize through support, onboarding, migration, hosted services, certified builds, training, and official modules.
10. Build importers before deep integrations.
11. Prove one niche before expanding.

## Biggest Positioning Improvements

Before:

> Open-source, self-hosted cash-flow management system.

After:

> Self-hosted cash control for small businesses.

Better:

> The cash cockpit for owner-led companies.

Sharpest:

> Know what cash is safe, what is committed, what is at risk, and what to do this week.

## Contradictions Found

No hard contradictions were introduced.

Existing README messaging is aligned with the new strategy because it already says OpenCashFlow is:

- self-hosted;
- cash-flow focused;
- not an accounting suite;
- not a full ERP;
- not SaaS/Stripe dependent.

Minor tension:

- README currently includes "business management" style language through scope and modules. Future copy should emphasize
  cash operations first and treat modules as supporting capabilities.

## Recommended README Updates

Update the opening paragraph from broad cash-flow management language to:

> OpenCashFlow is a self-hosted cash cockpit for small businesses. It helps owner-led teams see current cash, expected
> cash, committed cash, overdue payments, projected risk, and the decisions they need to make this week.

Add a "Product Focus" section:

```text
OpenCashFlow is not an ERP and not accounting software. It is the cash decision layer between your bank, accounting
system, invoices, supplier commitments, and weekly payment decisions.
```

Add a "Best Fit" section:

```text
OpenCashFlow is best for small companies, workshops, light manufacturers, distributors, consultants, and accounting firms
that still plan cash in spreadsheets.
```

Add a "Not For" section:

```text
OpenCashFlow is not for companies looking for certified accounting, payroll, full ERP/MRP, ecommerce, POS, or tax filing.
```

## Recommended Website Homepage Copy

Headline:

> Self-hosted cash control for small businesses.

Subheadline:

> See what cash is real, what cash is committed, what cash is at risk, and what decisions you need to make this week.

Primary call to action:

> Try the demo

Secondary call to action:

> Read the manifesto

Section: The Problem

> Your bank balance is not your cash position. Customer promises, supplier payments, payroll, taxes, loans, and purchases
> all compete for the same money. Spreadsheets hide the risk until it is too late.

Section: The Product

> OpenCashFlow brings bank movements, expected receivables, supplier commitments, taxes, payroll, and scenarios into one
> self-hosted cash cockpit.

Section: Weekly Ritual

> Every week, review cash reality, expected receipts, upcoming commitments, alerts, scenarios, and payment decisions.

Section: Not Another ERP

> Keep your accounting system. Keep your invoices. Keep your ERP if you have one. Use OpenCashFlow to understand cash
> risk and decisions before the bank surprises you.

## Recommended First Demo Copy

> Northside Workshop has 84,000 in the bank. It looks safe. But payroll, VAT, a supplier bill, and one late customer turn
> next week into a cash crunch. OpenCashFlow shows the risk, tests the scenarios, and helps the owner decide what to pay,
> what to delay, and who to call.

## Product North Star

The north-star metric should be:

> weekly cash reviews completed.

Supporting metrics:

- active companies running weekly review;
- receivables/payables imported;
- scenarios run;
- cash alerts resolved;
- accountant summaries shared;
- decisions recorded.

Avoid vanity metrics:

- modules enabled;
- dashboard views;
- raw transactions entered;
- generic user count.

## Final Product Direction

Build fewer features.

Make one habit unavoidable:

> Every Monday, the owner opens OpenCashFlow before making payment decisions.
