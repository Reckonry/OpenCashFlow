# Module Manifest

The module manifest is the contract between product, API, UI and operations. In the first implementation it is represented by `Shared.Modules.OpenCashFlowModuleManifest` and evaluated by `Shared.Modules.OpenCashFlowModuleRegistry`.

## Fields

| Field | Required | Description |
| --- | --- | --- |
| `Id` | Yes | Stable technical id, for example `Core.CashFlow` or `Reports`. |
| `DisplayName` | Yes | Human-readable name. |
| `Version` | Yes | Semantic module version. |
| `Type` | Yes | `Core`, `Official`, `Community`, or `Enterprise`. |
| `Lifecycle` | Yes | `Planned`, `Preview`, `Stable`, or `Legacy`. |
| `Required` | Yes | Required modules cannot be disabled. |
| `EnabledByDefault` | Yes | Default state on clean install. |
| `FeatureFlag` | No | Configuration path controlling the module, for example `Modules:Reports:Enabled`. |
| `Dependencies` | Yes | Required module ids. |
| `Permissions` | Yes | Module-scoped permission keys. |
| `MenuItems` | Yes | Menu entries contributed by the module. |
| `Description` | Yes | Operational description. |

## Example

```json
{
  "id": "Reports",
  "displayName": "Reports",
  "version": "0.1.0",
  "type": "Official",
  "lifecycle": "Preview",
  "required": false,
  "enabledByDefault": false,
  "featureFlag": "Modules:Reports:Enabled",
  "dependencies": ["Core.CashFlow"],
  "permissions": ["reports.read", "reports.export"],
  "menuItems": [
    {
      "area": "App",
      "label": "Reports",
      "path": "/Reports",
      "requiredRole": "CompanyAdmin",
      "order": 200
    }
  ],
  "description": "Optional advanced reports and exports."
}
```

## Configuration Mapping

Future module flags:

```json
{
  "Modules": {
    "Reports": { "Enabled": false }
  }
}
```

`Core.CashFlow` does not need a feature flag because it is required.

## Menu Item Contract

Menu items are declarative:

- `Area`: `App` or future areas.
- `Label`: display label or localization key.
- `Path`: local path.
- `RequiredRole`: optional role gate.
- `Order`: deterministic sort key.

UI should filter menu items by:

1. Module enabled state.
2. Current area.
3. Current user role/permissions.
4. Order.

The current registry exposes enabled state and missing dependencies. API/App still need thin adapters that resolve feature flags from their configuration providers.

## Permission Contract

Permissions are strings with module prefixes:

```text
payments.read
payments.write
cash.read
cash.adjust
reports.export
```

Roles can map to permission bundles, but business logic should eventually check permissions rather than hardcoded role strings.

## Versioning

Modules use semantic versioning:

- Patch: compatible bug fix.
- Minor: backward-compatible features.
- Major: breaking data/API behavior.

Each module release must declare compatible core versions.

## Migration Metadata

Future module manifests should include migration metadata:

```json
{
  "migrations": {
    "context": "ReportsDbContext",
    "assembly": "OpenCashFlow.Modules.Reports",
    "autoApply": false
  }
}
```

The first implementation documents this but keeps EF migrations monolithic until optional module data is physically split.
