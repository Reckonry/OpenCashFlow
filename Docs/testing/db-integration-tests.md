# Database Integration Tests

OpenCashFlow has a dedicated database integration-test project:

```text
tests/OpenCashFlow.Database.Tests
```

The project uses PostgreSQL Testcontainers and the real `ApplicationDbContext` migrations from
`OpenCashFlow.Infrastructure`. It is intended for persistence behavior that cannot be trusted with EF InMemory tests,
API-only tests, or pure Application tests.

## Scope

Use this project for:

- database foreign-key behavior;
- delete/cascade/restrict behavior;
- persistence defaults and nullable column behavior;
- migration-backed schema behavior;
- tenant-related persistence constraints when those constraints are actually modeled in EF/schema.

Do not use it to duplicate API contract tests or Application use-case tests.

## Current Coverage

The initial foundation covers:

- Company soft-delete flag persistence;
- Company nullable optional fields;
- Payment foreign-key failures for unknown payment method, document type, and user;
- current-schema cascade behavior when deleting a referenced payment method.

The cascade test documents observed behavior. It does not decide whether cascade is the desired product rule.

## Running

Docker must be available because Testcontainers starts PostgreSQL containers.

```bash
dotnet test tests/OpenCashFlow.Database.Tests/OpenCashFlow.Database.Tests.csproj --configuration Release
```

The default solution test command also includes this project:

```bash
dotnet test OpenCashFlow.sln --configuration Release --no-build
```

## Test Data Rules

Database integration tests should:

- create deterministic data per test;
- avoid `.First()` seed assumptions;
- use unique names/emails/IDs;
- avoid `Skip`;
- assert current schema behavior, not desired behavior without an implemented schema/model rule;
- document any surprising current behavior in `Docs/testing/db-test-triage.md`.

## Historical DB Tests

The old files under:

```text
tests/OpenCashFlow.Test/Tests/db/**/*.cs
```

remain excluded from compilation. They are historical drafts and should not be re-enabled in bulk. Valuable scenarios
should be migrated one at a time into `OpenCashFlow.Database.Tests` after confirming the intended product/schema rule.
