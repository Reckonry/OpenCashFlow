# Billing Module Boundary

OpenCashFlow core is a self-hosted community application. The core domain is company, workspace, users, roles, payments,
cash flow, dashboards, and audit.

Billing, plans, subscriptions, Stripe checkout, Stripe customer portal, and SaaS subscription enforcement are legacy SaaS
concerns. Runtime controllers, services, DTOs, UI routes, feature flags and Stripe client registration have been removed
from the community core. Some EF models and tables remain temporarily to avoid destructive model and migration changes.

## Current Boundary

- `Plan` and `Company_Subscription` remain in the data model for compatibility.
- `Company_Renewal`, `Stripe_Webhook_Event`, and Stripe fields on `Company` remain schema history.
- `SubscriptionAuthorizationMiddleware`, Billing API endpoints, disabled Billing services, App Billing routes and Stripe
  services are no longer part of the core runtime.
- The legacy `src/OpenCashFlow.Admin` project has been removed from the active repository source tree.

## Future Move

The next clean step is a migration-backed schema decision: export, archive, or remove the legacy Billing-owned tables and
columns. Any future Billing implementation should live in a separate optional module/package with its own configuration,
UI menu entries, Stripe settings, migrations, and documentation.
