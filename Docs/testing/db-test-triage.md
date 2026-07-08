# Database Test Triage

Date: 2026-07-08
Branch: `test/db-legacy-test-triage`

## Summary

The database-level tests under `tests/OpenCashFlow.Test/Tests/db/**/*.cs` are not part of the default test suite.

`tests/OpenCashFlow.Test/OpenCashFlow.Test.csproj` excludes them with:

```xml
<Compile Remove="Tests/db/**/*.cs" />
```

That means `dotnet test OpenCashFlow.sln --configuration Release --no-build` reports zero skipped tests because these files are not compiled or discovered. This is different from ordinary xUnit skipped tests. The files still contain test drafts, skipped facts, direct `ApplicationDbContext` usage, and assumptions about seed data/database constraints.

This document classifies them so they are no longer silent debt.

Follow-up foundation work has started in `tests/OpenCashFlow.Database.Tests`. That project runs real PostgreSQL
Testcontainers tests for a small set of high-value persistence behaviors. The old `Tests/db/**/*.cs` files remain
excluded and should not be re-enabled in bulk.

## Inventory

| File | Declared Tests | `Skip =` Attributes | Area | Current Status |
| --- | ---: | ---: | --- | --- |
| `tests/OpenCashFlow.Test/Tests/db/Company_Tests.cs` | 8 | 7 | Company persistence | Excluded from compilation |
| `tests/OpenCashFlow.Test/Tests/db/Employee_Tests.cs` | 12 | 12 | Employee/Company_Staff persistence | Excluded from compilation |
| `tests/OpenCashFlow.Test/Tests/db/Lookup_Tests.cs` | 30 | 28 | Payment methods, document types, lookup persistence | Excluded from compilation |
| `tests/OpenCashFlow.Test/Tests/db/Payment_Tests.cs` | 21 | 15 | Payment persistence | Excluded from compilation |
| `tests/OpenCashFlow.Test/Tests/db/Registration_Tests.cs` | 0 | 0 | Registration TODO notes | Excluded from compilation |
| **Total** | **71** | **62** | Company, employee, lookup, payment, registration notes | **Not discovered by test runner** |

## Covered Areas

The excluded files cover these intended database-level concerns:

- Company required columns, optional columns, contract date chronology, FK delete restrictions, soft delete, `DateIns`, and `DateEdit`.
- Employee/Company_Staff required FK fields, missing FK targets, duplicate user/company assignments, multi-company assignment rules, delete restrictions, `CreatedBy`, and UTC timestamps.
- Payment method and document type required fields, duplicate names, global lookup semantics, tenant FK behavior, visibility, soft delete, and delete restrictions when referenced by payments.
- Payment required fields, FK references to payment method/document type/company/user, negative amount, optional fields, audit timestamps, update, soft delete, query filtering, and related-data loading.
- Registration file contains TODO notes only and no executable tests.

## Existing Coverage Overlap

### Company

Current API/Application tests provide meaningful coverage for many public contract behaviors:

- create company success;
- required `CompanyName` validation;
- duplicate `CompanyName` validation;
- duplicate TIN validation;
- list/detail authorization;
- tenant isolation for detail/update;
- update validation;
- soft delete;
- active-relation delete failure;
- active-state/name/TIN/revenue filtering;
- self-hosted expired-contract behavior;
- `MaxUsers` employee-create enforcement.

Remaining database-specific value:

- direct persistence defaults for optional columns;
- date chronology constraints if they are meant to be database-enforced;
- `DateIns`/`DateEdit` persistence behavior;
- hard FK delete behavior beneath the API soft-delete contract.

Recommended action: convert only the remaining persistence-specific checks into a dedicated DB integration suite. Do not duplicate Company API contract tests.

### Payments

Current API/Application tests provide meaningful coverage for:

- create/update/delete payment flows;
- authentication and role enforcement;
- tenant isolation and cross-company lookup rejection;
- negative amount validation;
- locked field mutation rejection;
- list/detail/filter behavior;
- cash ledger and daily payment orchestration at Application level.

Remaining database-specific value:

- raw FK constraint behavior for invalid payment method, document type, tenant and user IDs;
- EF persistence of optional fields and timestamps;
- direct query filter behavior if required below repository level.

Recommended action: convert FK/timestamp persistence checks into a dedicated DB integration suite. Treat API-level behavior as already covered and avoid duplicating it.

### Payment Methods And Document Types

Current Application tests cover use-case validation and reader/writer interaction for payment methods and document types. The API lookup test file exists, but many compiled `Lookup_Tests.cs` facts contain only `// todo: implement test`, so they do not provide meaningful behavioral coverage.

Remaining valuable checks:

- required fields at database level;
- duplicate name constraints, if the schema is intended to enforce them;
- tenant-scoped versus global lookup semantics;
- soft delete persistence;
- FK delete restrictions when lookups are referenced by payments;
- visibility query semantics, if implemented below the API layer.

Recommended action: prioritize this group after Company/Payment persistence because overlap is weaker.

### Employees / Company_Staff

Application tests cover some employee use-case behavior, and registration API tests contain real assertions for registration/user/staff/company creation. However, `tests/OpenCashFlow.Test/Tests/API/Empolyee_Tests.cs` contains placeholder facts with `// todo: implement test`, so it does not provide meaningful API coverage for the scenarios it names.

Remaining valuable checks:

- `Company_Staff` FK constraints;
- duplicate `(UserID, TenantID)` rules;
- whether a user may belong to multiple companies in the current self-hosted model;
- employee delete restrictions when referenced by payments;
- audit/timestamp persistence.

Recommended action: decide the intended user-to-company cardinality first. Then convert constraints into DB integration tests and replace placeholder API Employee tests with real API tests.

### Registration

The excluded DB registration file contains TODO notes only. Current API registration tests contain real assertions around successful registration, validation failures, duplicate behavior, roles, approval flags, timestamps and email sending.

Recommended action: archive or delete the excluded DB registration TODO file after confirming every note maps to an existing API/Application test or a tracked backlog item.

## Recommended Migration Path

Do not re-enable `Tests/db/**/*.cs` in the default test project as-is. The files are not ready:

- they depend directly on `ApplicationDbContext`;
- they assume seeded data through `.First()` calls;
- several assertions describe missing desired behavior rather than current enforced behavior;
- several tests use placeholder wording such as `TBF`, `WIP`, and `TO BE FIXED`;
- some scenarios duplicate newer API/Application tests;
- some scenarios need explicit product decisions before they can be asserted safely.

Recommended phases:

1. Create a dedicated database integration test project, for example `tests/OpenCashFlow.Infrastructure.Tests` or `tests/OpenCashFlow.Database.Tests`.
2. Use Testcontainers PostgreSQL consistently, like the existing high-value API integration tests.
3. Move only persistence-specific tests, not API contract duplicates.
4. Replace seed assumptions with per-test data builders.
5. Remove all `Skip = ...` attributes from migrated tests.
6. Archive or delete the old excluded files only after migrated coverage exists.
7. Add a CI job or optional workflow that can run the DB integration suite explicitly.

## Integration Test Foundation Added

`tests/OpenCashFlow.Database.Tests` now provides the first real database integration-test foundation.

Current migrated coverage:

- Company soft-delete flags persist to PostgreSQL.
- Company optional nullable fields persist as `NULL`.
- Payment insert fails for an unknown `PaymentMethodID`.
- Payment insert fails for an unknown `DocumentTypeID`.
- Payment insert fails for an unknown `UserID`.
- Deleting a referenced payment method cascades to the related payment with the current EF schema.

The cascade lookup test is intentionally named as current-schema behavior. It is not a recommendation that lookup
deletes should cascade in the product. If the intended product rule is "restrict lookup delete while referenced", that
requires an explicit schema/model change and migration in a later slice.

The new tests use deterministic per-test data builders and avoid `.First()` seed assumptions. They do not carry any
`Skip` attributes.

## Group Decisions

| Group | Decision | Reason |
| --- | --- | --- |
| Company DB tests | Partially converted into DB integration tests | Soft-delete and nullable-field persistence are covered. Date chronology and hard company delete rules remain product/schema decisions. |
| Payment DB tests | Partially converted into DB integration tests | FK behavior for payment method, document type, and user is covered. Tenant FK is not asserted because the current EF model does not define a Company FK on `Payment.TenantID`. |
| Lookup DB tests | Partially converted through payment lookup FK behavior | Referenced payment-method delete currently cascades; restrict-delete behavior would require a deliberate schema change. |
| Employee DB tests | Convert after product decisions | User/company cardinality and delete semantics need explicit decisions before assertions are safe. |
| Registration DB TODO file | Archive or delete after mapping TODOs | It contains no executable tests; current API registration tests already cover much of the valuable behavior. |

## Immediate Change Made

The test project exclusion was made explicit:

```xml
<!--
  Historical database-level tests require a dedicated PostgreSQL integration-test pass.
  They are intentionally excluded from the default suite and triaged in Docs/testing/db-test-triage.md.
-->
<Compile Remove="Tests/db/**/*.cs" />
<None Include="Tests/db/**/*.cs" />
```

This keeps the default suite stable while making the excluded files visible as non-compiled test artifacts.

## Remaining Risk

The default test result can still say `0 skipped` while these historical DB test drafts remain outside compilation. The risk is now documented, but not eliminated.

The most important remaining coverage gap is not "make skipped count non-zero"; it is continuing to convert valuable
persistence behaviors into real Testcontainers-backed integration tests with deterministic setup and assertions.

Remaining migration candidates:

- Company hard-delete behavior once product/schema rules are explicit.
- Company `DateIns`/`DateEdit` persistence behavior.
- Payment timestamp persistence.
- Document-type referenced-delete behavior.
- Employee/company-staff cardinality, FK behavior, and delete semantics after a product decision.
- Archival or deletion of the registration DB TODO file after mapping its notes to current tests/backlog.
