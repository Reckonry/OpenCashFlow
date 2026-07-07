# Legacy Billing/Stripe Archive

Billing/Stripe is no longer part of the OpenCashFlow community core runtime. This document records the removed SaaS-era surface and the schema artifacts that remain temporarily for migration compatibility.

## Product Positioning

Billing is not installed in the core.

If it is ever reintroduced, it must live outside the community core as a separate optional module or external service. It may support:

- Manual customer billing for managed hosting or support contracts.
- Stripe Checkout or customer portal.
- Subscription lifecycle tracking.
- Invoices and renewal history.
- Webhook processing.

It must not:

- Block use of `Core.CashFlow`.
- Be required for setup.
- Be required for companies, users, payments, cash ledger or dashboard.
- Reintroduce a SaaS-only product model.

## Current State

The runtime controllers, services, DTOs, webhooks, feature flags and App views were removed from the community core.

Remaining items are schema/model history only:

- `Plan`.
- `Plan_Feature`.
- `Plan_Price`.
- `Company_Subscription`.
- `Company_Renewal`.
- `Stripe_Webhook_Event`.
- Stripe fields on `Company`.

These should not be referenced by daily core workflows.

## Owned Concepts

An external Billing module would own:

- `Plan`.
- `Company_Subscription`.
- `Company_Renewal`.
- Billing DTOs.
- Stripe DTOs/settings.
- Stripe webhook event model.
- Billing controllers and views.
- Subscription authorization middleware if retained.

## Core Concepts To Keep Separate

Core owns:

- `Company` as an operational workspace, not a SaaS customer.
- Users and roles.
- Payments.
- Payment methods.
- Document types.
- Cash ledger.
- Audit.
- Core dashboard.

Any current Stripe fields on `Company` should be treated as legacy Billing data until migrated.

## Module Boundary

Target external namespace layout:

```text
src/OpenCashFlow.Modules.Billing/
  BillingModule.cs
  BillingDbContext.cs
  Controllers/
  Services/
  Repositories/
  DTOs/
  Models/
  Migrations/
  Views/
```

No Billing service is registered by the core runtime.

## Data Migration Plan

Phase 1:

- Keep current schema to avoid a destructive migration.
- Mark tables/columns as legacy Billing-owned in documentation.
- Do not add new Billing concepts to core migrations.

Phase 2:

- Add `BillingDbContext`.
- Move Billing migrations into module migration folder/assembly.
- Keep compatibility migration for existing installs.

Phase 3:

- Move legacy Stripe fields from `Company` into a Billing-owned profile table.
- Keep read fallback during one release.
- Provide migration script.

## UI Rules

Core UI rules:

- No Billing menu item.
- No plan/subscription/pricing CTA in core UI.
- No Stripe portal button.
- No subscription-expired path in normal navigation.

## Security Rules

- Stripe secret keys must not exist in committed core configuration.
- Webhooks must not be exposed by the core runtime.
- Any future Billing action must be tenant-scoped and audited.

## Acceptance For Extraction

- Core boots without `Features:Billing`.
- No API/App route in daily workflow requires Billing.
- Stripe is never called by the core runtime.
- Legacy Billing schema is documented before any physical removal.
