# Skipped Tests Backlog

Date: 2026-07-07

Source command:

```bash
dotnet test OpenCashFlow.sln --configuration Release --no-build --logger "console;verbosity=normal"
```

Current result: 225 passed, 30 skipped, 0 failed.

This backlog intentionally keeps the skipped tests visible instead of deleting them. The skipped tests below cover real product gaps in company CRUD, role enforcement, tenant isolation, duplicate validation, payment cross-company validation, and locked field updates. They should not be re-enabled until the behavior exists and fixtures prove it without weakening assertions.

## Summary

| Area | Count | Priority | Notes |
| --- | ---: | --- | --- |
| Company CRUD endpoints and validation | 11 | P1 | Create, update, delete, duplicate validation, required fields, and delete semantics are not implemented or not wired to the route contract. |
| Company authorization and tenant isolation | 6 | P0 | Role policy and cross-company access checks are security-sensitive release blockers. |
| Company list filtering and state exposure | 5 | P1/P2 | Filters and expired contract state need product/API decisions before tests can be made authoritative. |
| Company user limit enforcement | 1 | P1 | `MaxUsers` behavior must be either enforced or removed from the product contract. |
| Payment authorization and role enforcement | 2 | P0 | Payment write/access authorization cannot remain skipped for a public release. |
| Payment tenant/cross-company validation | 3 | P0 | Cross-company payment and lookup validation are tenant isolation release blockers. |
| Payment locked field enforcement | 1 | P0 | User ownership fields must be immutable or the API contract must explicitly reject changes. |

## Backlog Items

| ID | Test | File | Skip reason | Area | Risk | Action | Priority |
| --- | --- | --- | --- | --- | --- | --- | --- |
| ST-001 | POST /v1/company should create company | `tests/OpenCashFlow.Test/Tests/API/Company_Tests.cs:28` | Endpoint not implemented yet | Company CRUD | Company creation contract is unverified. | Implement the create company endpoint/use case or remove the public route from the contract, then re-enable the test. | P1 |
| ST-002 | POST /v1/company by unauthorized user should fail | `tests/OpenCashFlow.Test/Tests/API/Company_Tests.cs:53` | Role/Policy not enforced yet | Company authorization | Unauthorized users may create companies if endpoint is enabled without policy. | Add role/policy enforcement and re-enable the test. | P0 |
| ST-003 | POST /v1/company missing required CompanyName should fail | `tests/OpenCashFlow.Test/Tests/API/Company_Tests.cs:71` | Endpoint not implemented yet | Company validation | Required field validation is not proven. | Implement create validation and re-enable the test. | P1 |
| ST-004 | POST /v1/company with duplicate CompanyName should fail | `tests/OpenCashFlow.Test/Tests/API/Company_Tests.cs:89` | Endpoint not implemented yet | Duplicate validation | Duplicate company names may create ambiguous tenant data. | Define duplicate-name policy, enforce it, and re-enable the test. | P1 |
| ST-005 | POST /v1/company with duplicate TIN should fail | `tests/OpenCashFlow.Test/Tests/API/Company_Tests.cs:107` | Endpoint not implemented yet | Duplicate validation | Duplicate fiscal identifiers can corrupt company identity and reporting. | Define tenant/global TIN uniqueness, enforce it, and re-enable the test. | P1 |
| ST-006 | GET /v1/company/view/{id} should fail when accessing other company | `tests/OpenCashFlow.Test/Tests/API/Company_Tests.cs:187` | Access control not implemented: endpoint returns current company ignoring route id | Tenant isolation | Cross-company data access may be hidden by route-id ignoring behavior. | Decide route semantics, enforce tenant ownership or reject route id, then re-enable the test. | P0 |
| ST-007 | GET /v1/company/view/{id} with non-existent id should fail | `tests/OpenCashFlow.Test/Tests/API/Company_Tests.cs:206` | Endpoint ignores route id and returns current company | Company detail routing | Invalid ids may return misleading data. | Make route id authoritative or remove it from the API contract, then re-enable the test. | P1 |
| ST-008 | PUT /v1/company/{id} should update company | `tests/OpenCashFlow.Test/Tests/API/Company_Tests.cs:227` | Endpoint not implemented yet | Company CRUD | Company update contract is unverified. | Implement update endpoint/use case and re-enable the test. | P1 |
| ST-009 | PUT /v1/company/{id} by unauthorized user should fail | `tests/OpenCashFlow.Test/Tests/API/Company_Tests.cs:246` | Role/Policy not enforced yet | Company authorization | Unauthorized users may update company profile data. | Add role/policy enforcement and re-enable the test. | P0 |
| ST-010 | PUT /v1/company/{id} should fail when updating other company | `tests/OpenCashFlow.Test/Tests/API/Company_Tests.cs:265` | Access control not implemented yet | Tenant isolation | Cross-company mutation risk. | Enforce tenant ownership in update use case and re-enable the test. | P0 |
| ST-011 | PUT /v1/company/{id} changing TIN to duplicate should fail | `tests/OpenCashFlow.Test/Tests/API/Company_Tests.cs:284` | Endpoint not implemented yet | Duplicate validation | Duplicate fiscal identifiers can be introduced through updates. | Enforce TIN uniqueness on update and re-enable the test. | P1 |
| ST-012 | PUT /v1/company/{id} invalidating required fields should fail | `tests/OpenCashFlow.Test/Tests/API/Company_Tests.cs:303` | Endpoint not implemented yet | Company validation | Updates may persist invalid company data. | Enforce required fields in update and re-enable the test. | P1 |
| ST-013 | DELETE /v1/company/{id} soft delete should succeed for admin | `tests/OpenCashFlow.Test/Tests/API/Company_Tests.cs:324` | Endpoint not implemented yet | Company CRUD | Company lifecycle behavior is not tested. | Implement soft delete semantics and re-enable the test. | P1 |
| ST-014 | DELETE /v1/company/{id} by unauthorized user should fail | `tests/OpenCashFlow.Test/Tests/API/Company_Tests.cs:342` | Role/Policy not enforced yet | Company authorization | Unauthorized destructive action risk. | Add delete role/policy enforcement and re-enable the test. | P0 |
| ST-015 | DELETE /v1/company/{id} already deleted should fail | `tests/OpenCashFlow.Test/Tests/API/Company_Tests.cs:360` | Endpoint not implemented yet | Company CRUD | Delete idempotency/error semantics are undefined. | Define soft-delete repeat behavior and re-enable the test. | P1 |
| ST-016 | DELETE /v1/company/{id} with active relations should fail | `tests/OpenCashFlow.Test/Tests/API/Company_Tests.cs:379` | Endpoint not implemented yet | Company data integrity | Deleting active companies may orphan users/payments/cash data. | Enforce relation guard and re-enable the test. | P1 |
| ST-017 | GET /v1/company with insufficient role should fail | `tests/OpenCashFlow.Test/Tests/API/Company_Tests.cs:434` | Role policy not enforced on endpoints yet | Company authorization | Low-privilege users may read company data. | Add read role/policy enforcement and re-enable the test. | P0 |
| ST-018 | POST /v1/company with IsActive=false should succeed | `tests/OpenCashFlow.Test/Tests/API/Company_Tests.cs:453` | Endpoint not implemented yet | Company CRUD | Active/inactive creation semantics are unverified. | Implement or explicitly reject `IsActive` creation semantics, then re-enable/update the test. | P1 |
| ST-019 | GET /v1/company/all?isActive=true should filter active companies | `tests/OpenCashFlow.Test/Tests/API/Company_Tests.cs:471` | Filtering not implemented yet | Company filtering | List APIs may return unexpected records and weaken operational workflows. | Implement active-state filtering or remove the filter contract, then re-enable the test. | P1 |
| ST-020 | GET /v1/company should reflect expired contract state | `tests/OpenCashFlow.Test/Tests/API/Company_Tests.cs:488` | No explicit expired state exposure implemented | Company state | Expiration semantics are unclear after SaaS removal. | Make a product decision: expose a self-hosted state or delete the obsolete expectation with replacement coverage. | P2 |
| ST-021 | POST /v1/employee should fail when company exceeds MaxUsers | `tests/OpenCashFlow.Test/Tests/API/Company_Tests.cs:505` | MaxUsers enforcement not implemented in EmployeeService | Employee limit enforcement | License/plan-derived user limits may be obsolete or unenforced. | Decide whether self-hosted keeps `MaxUsers`; enforce or remove the concept and update tests. | P1 |
| ST-022 | GET /v1/company/all?name=Scunio should filter by name | `tests/OpenCashFlow.Test/Tests/API/Company_Tests.cs:526` | Filtering not implemented yet | Company filtering | Search/list behavior is not contract-tested. | Implement name filtering or remove the filter contract, then re-enable the test. | P1 |
| ST-023 | GET /v1/company/all?tin=IT123 should filter by TIN | `tests/OpenCashFlow.Test/Tests/API/Company_Tests.cs:543` | Filtering not implemented yet | Company filtering | Fiscal-id lookup behavior is not contract-tested. | Implement TIN filtering or remove the filter contract, then re-enable the test. | P1 |
| ST-024 | GET /v1/company/all?revenueFrom=1000&revenueTo=100000 should filter by revenue range | `tests/OpenCashFlow.Test/Tests/API/Company_Tests.cs:560` | Filtering not implemented yet | Company filtering | Financial range filtering is not contract-tested. | Implement revenue filtering or remove the filter contract, then re-enable the test. | P1 |
| ST-025 | POST /v1/payments with unauthorized employee should fail | `tests/OpenCashFlow.Test/Tests/API/Payments_Tests.cs:124` | non ancora implementati ruoli | Payment authorization | Unauthorized payment creation is a high-impact financial integrity risk. | Enforce payment create roles and re-enable the test. | P0 |
| ST-026 | POST /v1/payments to different company should fail | `tests/OpenCashFlow.Test/Tests/API/Payments_Tests.cs:155` | TBF | Payment tenant isolation | Cross-company payment creation can corrupt tenant data. | Enforce tenant ownership on create command/lookup inputs and re-enable the test. | P0 |
| ST-027 | PUT /v1/payments/{id} should fail if PaymentMethod is from another company | `tests/OpenCashFlow.Test/Tests/API/Payments_Tests.cs:562` | Da capire come limitare nel db stesso questo | Payment tenant isolation | Cross-company lookup references can corrupt payment semantics. | Enforce tenant-scoped payment method validation in update path and re-enable the test. | P0 |
| ST-028 | PUT /v1/payments/{id} should fail if DocumentType is from another company | `tests/OpenCashFlow.Test/Tests/API/Payments_Tests.cs:588` | Da capire come limitare nel db questo... | Payment tenant isolation | Cross-company document type references can corrupt payment semantics. | Enforce tenant-scoped document type validation in update path and re-enable the test. | P0 |
| ST-029 | PUT /v1/payments/{id} should fail if trying to modify locked field (UserID) | `tests/OpenCashFlow.Test/Tests/API/Payments_Tests.cs:614` | non ancora previso a sistema... | Payment immutable fields | Payment ownership/audit attribution can be tampered with. | Define immutable fields in update command and reject `UserID` mutation, then re-enable the test. | P0 |
| ST-030 | Any /v1/payments API access with invalid role should fail | `tests/OpenCashFlow.Test/Tests/API/Payments_Tests.cs:772` | not done yet roles | Payment role enforcement | Invalid roles may access financial APIs. | Add role policy enforcement across payment endpoints and re-enable the test. | P0 |

## Release Blockers

The following skipped-test groups are release blockers for a public release:

- Company role/policy enforcement and tenant isolation: ST-002, ST-006, ST-009, ST-010, ST-014, ST-017.
- Payment role enforcement and invalid-role rejection: ST-025, ST-030.
- Payment cross-company validation: ST-026, ST-027, ST-028.
- Payment immutable ownership/audit fields: ST-029.

The remaining P1 items are not necessarily security blockers, but they keep public API behavior unclear and should be resolved before calling the project stable.

## Decision Log

- No skipped tests were deleted in this pass because the current skips represent meaningful, high-value coverage targets.
- No skipped tests were re-enabled in this pass because their own skip reasons point to missing product behavior rather than broken fixtures alone.
- No assertions were weakened.
- No production behavior was changed solely to satisfy tests.
