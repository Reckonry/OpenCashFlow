# Legacy Billing/Stripe Removal Task List

This is the implementation backlog for removing Billing/Stripe from the core without a destructive database migration.

## Phase 1: Boundary And Flags

- [x] Remove `Features:Billing` from core configuration.
- [x] Remove subscription authorization from the core runtime.
- [x] Remove Stripe client registration from API.
- [x] Remove Billing/Stripe controllers, services, repositories and DTOs from the core runtime.
- [x] Remove Billing/Pricing/Upgrade/Checkout UI from the App.
- [x] Remove legacy Admin project from the solution.
- [ ] Add integration test: core API starts with empty Stripe configuration and no Billing feature flag.

## Phase 2: Namespace Cleanup

- [x] Remove Billing DTOs from the core contracts/runtime.
- [x] Remove `StripeSettings`.
- [x] Remove Stripe retry policies from Shared.
- [ ] Move legacy entities `Plan`, `Company_Subscription`, `Company_Renewal` out of core migrations through a migration-backed schema decision.
- [ ] Move or remove `Stripe_Webhook_Event` through a migration-backed schema decision.

## Phase 3: API Extraction

- [x] Remove Billing controllers from API.
- [x] Remove Billing repositories/services from API.
- [x] Remove Stripe webhook processor from API.
- [x] Remove disabled/no-op Billing services because no core interface remains.

## Phase 4: UI Extraction

- [x] Remove App Billing views/controllers.
- [x] Remove template pricing/upgrade/checkout partials from core layouts.
- [ ] Add UI test/manual checklist: no Billing/Pricing/Stripe menu or CTA in the App.

## Phase 5: Data Extraction

- [ ] Document current legacy Billing-owned tables and columns.
- [ ] Create module-specific migration strategy.
- [ ] Introduce `BillingDbContext` or separate migrations assembly.
- [ ] Move new Billing migrations out of core migration stream.
- [ ] Add migration path for Stripe fields currently stored on `Company`.

## Phase 6: Operational Packaging

- [ ] Add module enable/disable documentation.
- [ ] Add backup guidance for Billing data.
- [ ] Add restore procedure when Billing is disabled but historical Billing data exists.
- [ ] Add semantic version compatibility matrix.

## Known Legacy To Remove Or Move

- `OpenCashFlow.Infrastructure.Persistence.Entities.Plan`.
- `OpenCashFlow.Infrastructure.Persistence.Entities.Company_Subscription`.
- `OpenCashFlow.Infrastructure.Persistence.Entities.Company_Renewal`.
- Stripe fields on `Company`.
- `OpenCashFlow.Infrastructure.Persistence.Entities.Stripe.Stripe_Webhook_Event`.
- Legacy Billing/Stripe EF models and tables.
- Stripe fields on `Company`.
