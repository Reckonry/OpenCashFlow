# Lean Product Roadmap

Date: 2026-07-08

Role: Product Guardian.

Goal:

> Remove anything that does not make OpenCashFlow a better Cash Cockpit.

This roadmap is intentionally smaller than `PRODUCT_ROADMAP.md`.

It cuts feature ambition in favor of one habit:

> Every week, the owner opens OpenCashFlow to answer: what can we safely pay?

## Product Guardrail

Keep only work that directly strengthens at least one of these:

- Safe-to-Pay Radar;
- weekly cash decisions;
- cash visibility;
- uncertainty reduction;
- trust in the forecast;
- cash custody and cash integrity;
- import of existing cash facts;
- owner action this week.

Move to Later if useful but not essential to the first habit.

Delete if it drifts toward ERP, marketplace, broad business management, or premature go-to-market machinery.

## The Lean Roadmap

## Now

Goal:

Make the Cash Cockpit identity impossible to misunderstand.

Keep:

1. Adopt "self-hosted cash cockpit" across product messaging.
2. Define Safe-to-Pay Radar as the core feature.
3. Define the weekly cash ritual as the primary workflow.
4. Define owner dashboard around safe cash, committed cash, expected cash, risk, and weekly actions.
5. Keep the feature filter explicit.
6. Define Cash Custody as the required foundation for trusted cash, with Cash Integrity as the proof capability.

Cut from Now:

- broad demo story work unless it directly demonstrates Safe-to-Pay Radar.

Why:

The product does not need many stories. It needs one unavoidable story: "Can I safely pay this?"

## Next Month

Goal:

Make one owner understand the product in one realistic cash decision.

Keep:

1. Cash Custody foundation:
   - cash account/source;
   - custodian/holder;
   - employee cash handler;
   - immutable movement;
   - custody transfer;
   - reversal/correction;
   - daily reconciliation;
   - discrepancy visibility.
2. Safe-to-Pay Radar concept and prototype, clearly marked WIP until Cash Custody and Cash Integrity are implemented.
3. Workshop demo dataset only if it supports the Radar:
   - late customer;
   - supplier payment;
   - payroll reserve;
   - VAT/tax reserve;
   - optional machine purchase;
   - projected cash shortfall.
4. Owner dashboard concept reduced to:
   - bank cash;
   - safe cash;
   - committed cash;
   - expected cash;
   - projected low point;
   - recommended weekly actions.
5. Manual import templates for the minimum facts:
   - bank balance/movements;
   - expected receivables;
   - supplier commitments;
   - reserves.
6. Interview 10 target users specifically about payment approval anxiety and end-of-day cash mismatch anxiety.

Move to Later:

- full homepage copy system;
- broad interviews across four segments.

Delete:

- generic product-marketing polish that does not test Safe-to-Pay Radar.

Why:

The next month must validate the painful decision, not the brand system. Safe-to-Pay cannot be trusted until the cash
custody chain is auditable and reconcilable.

## Next Quarter

Goal:

Make Safe-to-Pay Radar useful in a real weekly review.

Keep:

1. Payment Decision Queue:
   - safe to pay;
   - risky;
   - unsafe;
   - split recommended;
   - delay recommended.
2. Safe Cash model:
   - current cash;
   - protected reserves;
   - committed cash;
   - expected cash;
   - risky expected cash.
3. Receivables/payables lite, only as inputs to cash decisions.
4. Cash confidence levels:
   - confirmed;
   - expected;
   - risky;
   - hypothetical.
5. Basic scenarios:
   - customer pays late;
   - supplier split payment;
   - payment delayed;
   - tax/payroll reserve protected.
6. Cash Collision Timeline, only inside the Radar.
7. Weekly Action Summary:
   - pay;
   - delay;
   - split;
   - chase;
   - reserve.

Move to Later:

- full cash forecast calendar;
- overdue aging as a standalone module;
- accountant/advisor summary export;
- CSV reconciliation queue.

Delete:

- any receivables/payables screen that looks like accounting software;
- any scenario builder that becomes financial modeling;
- any alert feed not tied to a payment decision.

Why:

The quarter should deliver a working answer to "what can we safely pay?", not a finance suite.

## Next 6 Months

Goal:

Make the weekly habit repeatable and trusted.

Keep:

1. Bank CSV import and match queue.
2. Recurring commitments:
   - payroll reserve;
   - tax/VAT reserve;
   - rent;
   - loans;
   - recurring supplier obligations.
3. What changed since last week:
   - new commitments;
   - changed due dates;
   - changed confidence;
   - forecast low point changed.
4. Customer payment promise tracker, only as forecast confidence input.
5. Decision log:
   - paid;
   - delayed;
   - split;
   - chased;
   - reserved.
6. One accountant/share summary, limited to the weekly cash review.

Move to Later:

- accountant/advisor workspace;
- multi-client dashboard;
- importers from Odoo, ERPNext, Dolibarr, Akaunting, Invoice Ninja;
- workshop cash pack;
- public hosted demo;
- case studies.

Delete:

- adoption-loop features that do not improve the weekly owner workflow;
- broad partner tooling before the owner habit is proven.

Why:

If owners do not return weekly, accountant workspace and case studies are premature.

## Next Year

Goal:

Turn a proven weekly cash ritual into a product people can adopt confidently.

Keep:

1. Stable Safe-to-Pay Radar.
2. Reliable import/export around the Radar.
3. Stable owner dashboard.
4. Cash review history.
5. Narrow accountant handoff:
   - weekly summary;
   - notes;
   - export.
6. Comparison content only if it explains the focused category:
   - OpenCashFlow vs spreadsheets;
   - OpenCashFlow vs ERP for cash decisions;
   - OpenCashFlow vs accounting reports.

Move to Later:

- productized onboarding packages;
- support subscription offer;
- certified self-hosted builds;
- managed hosting beta;
- official forecasting module;
- official accountant module;
- official workshop/manufacturing module;
- localization.

Delete:

- "become the default open-source cash cockpit" as a roadmap goal.

Why:

Market position is an outcome. The roadmap should describe product work. Do not build commercialization machinery before
the core weekly habit is proven.

## Next 3 Years

Goal:

Expand only after Safe-to-Pay Radar becomes a trusted category-defining workflow.

Keep:

1. Regional bank connector packs, if they reduce manual cash uncertainty.
2. Country-specific tax calendar packs, if limited to cash reserve dates and not tax compliance.
3. Partner templates for weekly cash review.
4. API ecosystem for importing cash facts from existing systems.

Move to Later:

- managed hosting;
- certified releases and upgrade assurance;
- partner program;
- official training and certification;
- anonymized benchmarking;
- marketplace.

Delete:

- broad marketplace as a strategic goal until the core workflow has real pull;
- community templates for industries unless they directly support Safe-to-Pay Radar.

Why:

OpenCashFlow should earn expansion by owning one habit first.

## Kept Roadmap Items

These survive because they directly strengthen the Cash Cockpit.

| Item | Why It Stays |
| --- | --- |
| Self-hosted cash cockpit positioning | Clarifies product identity. |
| Safe-to-Pay Radar | Core "I need this" feature. |
| Weekly cash ritual | Creates habit. |
| Owner dashboard | Main decision surface. |
| Feature filter | Prevents ERP drift. |
| Cash Custody | Proves who or what is responsible for company money before forecast recommendations. |
| Cash Integrity | Proves the custody chain is complete, immutable, reconciled, and explainable. |
| Workshop demo dataset | Demonstrates one painful cash decision. |
| Manual imports for bank/receivables/commitments/reserves | Feeds the Radar without integrations. |
| Payment Decision Queue | Turns cash visibility into action. |
| Safe Cash model | Separates bank balance from spendable cash. |
| Receivables/payables lite | Necessary inputs, not accounting replacement. |
| Cash confidence levels | Reduces false certainty. |
| Basic scenarios | Lets owners test decisions. |
| Cash Collision Timeline | Shows why a payment is risky. |
| Weekly Action Summary | Ends with decisions, not reports. |
| Bank CSV import | Reduces manual work and improves trust. |
| Recurring commitments | Protects payroll, tax, rent, loans. |
| What changed since last week | Builds trust and weekly habit. |
| Payment promise tracker | Improves forecast confidence. |
| Decision log | Records why cash decisions were made. |
| Narrow accountant summary | Supports weekly cash conversation. |
| Cash review history | Makes weekly ritual cumulative. |
| Focused comparison content | Explains category without feature bloat. |
| Bank connector packs | Later, only to reduce cash uncertainty. |
| Tax calendar packs | Later, only as reserve-date helpers. |
| Cash fact import API | Helps integrate without replacing systems. |

## Moved To Later

These may be useful, but not before Safe-to-Pay Radar proves the weekly habit.

| Item | Why Later |
| --- | --- |
| Full homepage copy system | Useful, but messaging should follow validated feature pull. |
| Broad interviews across multiple segments | Too diffuse; start with payment anxiety in one niche. |
| Full cash forecast calendar | Useful, but can distract from payment decisions. |
| Overdue aging as standalone module | Risks becoming accounting/invoicing. Keep only as Radar input. |
| Accountant/advisor summary export | Valuable, but only after owner workflow is useful. |
| CSV reconciliation queue | Important, but not first decision value. |
| Accountant/advisor workspace | Distribution feature; premature before weekly owner habit. |
| Multi-client dashboard | Could become advisor SaaS before core is proven. |
| Importers from Odoo/ERPNext/Dolibarr/Akaunting/Invoice Ninja | Useful after manual import proves demand. |
| Workshop cash pack | Good niche packaging, but depends on Radar. |
| Public hosted demo | Useful after demo story stabilizes. |
| Case studies | Need real usage first. |
| Productized onboarding packages | Commercialization after habit proof. |
| Support subscription | Commercialization after stable adoption. |
| Certified self-hosted builds | Trust layer, not product wedge. |
| Managed hosting beta | Operations business; later. |
| Official forecasting module | Risk of module sprawl; keep forecast inside Radar first. |
| Official accountant module | Later if accountant summary proves pull. |
| Official workshop/manufacturing module | Later if workshop demo converts. |
| Localization | Later after core workflow is stable. |
| Partner program | Later after repeatable adoption. |
| Official training/certification | Later after product maturity. |
| Anonymized benchmarking | Interesting, not essential to weekly cash decisions. |
| Marketplace | Much later; marketplace amplifies demand, it does not create it. |

## Deleted Items

These should be removed from the active product roadmap.

| Deleted Item | Why It Is Deleted |
| --- | --- |
| Generic product-marketing polish | Does not directly improve cash decisions. |
| Broad demo stories | One sharp Safe-to-Pay story is better than many demos. |
| Receivables/payables as standalone accounting-like screens | Drifts toward accounting software. |
| Scenario builder as financial modeling | Risks becoming complex planning software. |
| Generic alert feed | Alerts must be tied to payment decisions or they become noise. |
| Adoption-loop features before weekly habit | Growth before retention is waste. |
| Broad partner tooling before owner workflow | Optimizes distribution before product pull. |
| "Become the default open-source cash cockpit" as roadmap item | Outcome, not a roadmap item. |
| Community templates for industries not tied to Radar | Risks generic template marketplace. |
| Broad marketplace as strategic goal | Creates ERP/platform drift before category pull exists. |

## Roadmap Cut Summary

Original product roadmap direction contained roughly:

- positioning work;
- demo work;
- dashboard work;
- imports;
- interviews;
- forecast/calendar;
- receivables/payables;
- confidence levels;
- scenarios;
- accountant export;
- reconciliation;
- advisor workspace;
- multi-client dashboard;
- system importers;
- alerts;
- workshop pack;
- hosted demo;
- case studies;
- onboarding;
- support;
- certified builds;
- managed hosting;
- official modules;
- localization;
- comparison pages;
- bank connectors;
- tax packs;
- benchmarking;
- API ecosystem;
- templates;
- training;
- marketplace.

Lean roadmap keeps the core cash-decision system and moves or deletes most commercialization, marketplace, module, and
partner expansion work.

Estimated cut:

- Kept now/active: about 40%.
- Moved to Later: about 40%.
- Deleted: about 20%.

Active focus reduced by more than 50%.

## The New Roadmap In One Page

## Phase 1: Prove The Pain

Build the product around one question:

> What can we safely pay this week?

Deliver:

- Safe-to-Pay Radar concept;
- workshop cash decision demo;
- manual cash inputs;
- safe cash model;
- owner dashboard.

## Phase 2: Make It Useful Weekly

Deliver:

- payment decision queue;
- receivables/payables lite as inputs;
- reserves;
- confidence levels;
- scenarios;
- cash collision timeline;
- weekly actions.

## Phase 3: Make It Trusted

Deliver:

- bank CSV import;
- recurring commitments;
- what changed since last week;
- payment promises;
- decision log;
- accountant summary.

## Phase 4: Expand Carefully

Only after weekly use is proven:

- selected importers;
- narrow accountant workflow;
- workshop packaging;
- bank connectors;
- tax calendar packs.

## Final Product Guardian Decision

Build fewer things.

Build Safe-to-Pay Radar first.

Reject anything that does not help an owner decide what to pay this week.
