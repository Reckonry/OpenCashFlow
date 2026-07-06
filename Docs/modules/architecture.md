# OpenCashFlow Modular Architecture

OpenCashFlow is a self-hosted community product first. The modular architecture must keep the community core installable and useful without SaaS billing, Stripe, marketplace services, a separate legacy Admin application, or external managed services.

The target model is Odoo-like in principle, but intentionally simpler in the first iteration: modules are statically shipped, registered through manifests, and enabled through feature/module flags. Dynamic plugin loading, remote marketplaces, runtime assembly loading, and tenant-specific code downloads are out of scope for the first modular release.

## Product Editions

### Community Core

The community core is always installed and cannot be disabled.

Initial core module:

- `Core.CashFlow`

Responsibilities:

- Companies/workspaces.
- Users and roles.
- Payments in and out.
- Payment methods and document types.
- Cash ledger and balances.
- Dashboard base KPIs.
- Audit log.
- Basic CSV exports.
- Setup wizard and self-hosted configuration.

### Optional Official Modules

Official modules are maintained by the OpenCashFlow project but must not be required to boot or operate the core.

Candidates:

- `ItalianEInvoicing`
- `Forecasting`
- `CRM`
- `Inventory`
- `HR`
- `Reports`

### Commercial Strategy

Commercialization should happen outside artificial core restrictions:

- Managed hosting.
- Professional setup and migrations.
- Support contracts.
- Training.
- Advanced optional modules.
- Integration packages.
- Marketplace listing and curation.

The AGPL community core must remain useful without paid unlocks.

## Module Model

Each module has a manifest with:

- Stable `Id`.
- Human display name.
- Semantic `Version`.
- Type: `Core`, `Official`, `Community`, `Enterprise`.
- Lifecycle: `Planned`, `Preview`, `Stable`, `Legacy`.
- Required flag.
- Enabled-by-default flag.
- Feature flag path.
- Dependencies.
- Permissions.
- Menu items.
- Description.

The first implementation lives in `Shared.Modules.OpenCashFlowModuleCatalog` and `Shared.Modules.OpenCashFlowModuleRegistry`. It is intentionally static. This creates a central product map before physical project extraction.

## Runtime Registry

The registry should expose three concepts:

- Available modules: all known manifests from the catalog.
- Installed modules: modules whose code/migrations are present.
- Enabled modules: installed modules enabled by configuration for the instance.

First iteration:

- `Core.CashFlow` is required and always enabled.
- Billing/Stripe is not installed in the core runtime. Historical schema artifacts are kept only for migration compatibility.
- Planned modules are visible in documentation/catalog only, not in UI.
- `OpenCashFlowModuleRegistry` evaluates module state from the static catalog and a feature-flag resolver supplied by API/App.

Later iteration:

- Add database table `InstanceModules` with `ModuleId`, `InstalledVersion`, `Enabled`, `InstalledAtUtc`, `EnabledAtUtc`, `ConfiguredBy`.
- Add an Instance Admin screen to enable/disable modules.
- Validate dependencies before enabling a module.
- Emit audit events for module install/enable/disable/configuration.

## Dependency Rules

Dependencies are explicit and one-way.

Examples:

- `Reports` depends on `Core.CashFlow`.
- `ItalianEInvoicing` depends on `Core.CashFlow` and may later integrate with `Reports`.
- `Inventory` may depend on `CRM` only if customer/supplier models are moved there.

Core must not depend on optional modules.

## Service Registration

First iteration keeps static DI registration:

- Core services are always registered.
- Optional module services are introduced only when a module exists as a separate runtime boundary.
- No Stripe/Billing service is registered by the community core.

Target pattern:

```csharp
if (moduleRegistry.IsEnabled("Reports"))
{
    services.AddReportsModule(configuration);
}
```

## Menu And UI

Menu entries must come from enabled modules.

First iteration:

- Core menus are App-only.
- Removed Billing/Pricing/Upgrade/Checkout menu entries must not reappear in core layouts.
- Module manifests declare intended menu entries for future modules.

Target pattern:

- App menu components read enabled module menu entries.
- Each menu item declares area, path, label, required role and display order.
- Disabled modules produce no menu entries.
- No dead links to disabled features.

## Permissions

Permissions should be module-scoped:

- `payments.read`
- `payments.write`
- `cash.read`
- `cash.adjust`
- `reports.export`

Roles map to permission bundles:

- `InstanceAdmin`: instance/module administration.
- `CompanyAdmin`: company operations and user management.
- `Employee`: daily operational access.

## Modular Migrations

Core migrations are always applied.

Optional module migrations should apply only when the module is installed/enabled. Because EF Core migrations are currently monolithic, first iteration must not physically split migrations. Instead:

1. Document tables currently owned by optional modules.
2. Stop adding optional module tables to core migrations.
3. Move optional module migrations into module-specific assemblies or folders in a later release.
4. Provide install scripts per module.

Target approaches:

- Separate DbContext per module.
- Separate migrations assembly per module.
- `dotnet ef database update --context BillingDbContext` only when Billing is installed.

Uninstall must be explicit:

- Disable module first.
- Keep data by default.
- Purge data only with an administrative purge command.

## Packaging

Recommended packaging model:

- Monorepo for community core and official modules while the project is young.
- Separate repositories only when module ownership or release cadence diverges.
- Semantic versioning for core and each official module.
- Release notes must state compatible core version ranges.

Example compatibility:

```text
Reports 0.1.x requires OpenCashFlow Core >= 1.0.0
```

## Roadmap

### Phase A: Static Registry

- Add module manifest model.
- Add built-in module catalog.
- Document ownership and dependencies.
- Keep current feature flags.

### Phase B: Legacy SaaS Data Boundary

- Keep historical `Plan`, subscription and Stripe tables only as legacy schema until a migration removes or exports them.
- Do not expose Billing/Stripe controllers, services, menus, DTOs or feature flags in the core runtime.
- Document any remaining legacy table before changing schema.

### Phase C: UI Menu Registry

- Generate App menus from enabled module manifests.
- Remove template pricing/upgrade partials from core layouts.
- Add an InstanceAdmin modules page inside the main App only if module administration is needed.

### Phase D: Modular Data

- Introduce module installation table.
- Split optional module migrations.
- Add install/disable/purge workflows.

### Phase E: Advanced Modules

- Reports.
- Italian e-invoicing.
- Forecasting.
- CRM/Inventory/HR depending on user demand.
