# Skipped Tests Backlog

Date: 2026-07-08

Source command:

```bash
dotnet test OpenCashFlow.sln --configuration Release --no-build --logger "console;verbosity=normal"
```

Current result after the Company CRUD/contract coverage pass: 255 passed, 0 skipped, 0 failed.

This file is kept as an audit trail. It should be updated whenever a skipped test is introduced, removed, or resolved.

## Summary

| Area | Previous Count | Current Count | Status |
| --- | ---: | ---: | --- |
| Company CRUD endpoints and validation | 11 | 0 | Fixed |
| Company authorization and tenant isolation | 0 | 0 | Fixed in the P0 pass |
| Company list filtering and state exposure | 5 | 0 | Fixed |
| Company user limit enforcement | 1 | 0 | Fixed |
| Payment authorization and role enforcement | 0 | 0 | Fixed in the P0 pass |
| Payment tenant/cross-company validation | 0 | 0 | Fixed in the P0 pass |
| Payment locked field enforcement | 0 | 0 | Fixed in the P0 pass |

## Fixed In This Pass

The remaining P1/P2 Company tests were re-enabled in `test/company-crud-contract-coverage` and now pass without weakening assertions:

- ST-001: `POST /v1/company should create company`.
- ST-003: `POST /v1/company missing required CompanyName should fail`.
- ST-004: `POST /v1/company with duplicate CompanyName should fail`.
- ST-005: `POST /v1/company with duplicate TIN should fail`.
- ST-007: `GET /v1/company/view/{id} with non-existent id should fail`.
- ST-008: `PUT /v1/company/{id} should update company`.
- ST-011: `PUT /v1/company/{id} changing TIN to duplicate should fail`.
- ST-012: `PUT /v1/company/{id} invalidating required fields should fail`.
- ST-013: `DELETE /v1/company/{id} soft delete should succeed for admin`.
- ST-015: `DELETE /v1/company/{id} already deleted should fail`.
- ST-016: `DELETE /v1/company/{id} with active relations should fail`.
- ST-018: `POST /v1/company with IsActive=false should succeed`.
- ST-019: `GET /v1/company/all?isActive=true should filter active companies`.
- ST-020: expired contract behavior was rewritten to the current self-hosted contract: expired contract metadata remains readable and does not imply SaaS lockout.
- ST-021: `POST /v1/employee should fail when company exceeds MaxUsers`.
- ST-022: `GET /v1/company/all?name=... should filter by name`.
- ST-023: `GET /v1/company/all?tin=... should filter by TIN`.
- ST-024: `GET /v1/company/all?revenueFrom=...&revenueTo=... should filter by revenue range`.

## Product Decisions Captured

- Company create/update/delete remain part of the public API contract.
- Company delete is a soft delete.
- Company delete is rejected when active users, payments, or invoices exist for the tenant.
- Company duplicate validation is global for `CompanyName` and TIN.
- The API accepts `TIN` as an input alias while preserving the existing `NIN` response field.
- Company list filtering is an intended API contract for active state, name, TIN, and estimated annual revenue range.
- `MaxUsers` remains a self-hosted company limit and is enforced during employee creation.
- Expired contract dates are metadata in the self-hosted build; they do not block reading the company profile.

## Remaining Backlog Items

None.

## Release Blockers

No skipped-test release blockers remain after this pass.

## Decision Log

- No skipped tests were deleted solely to make the suite green.
- Obsolete seed assumptions were replaced with tests that create isolated test data.
- Assertions were strengthened for filtering tests to verify returned content, not just `200 OK`.
- Production behavior changed only where public endpoints already existed but returned `501` or had undefined contract behavior.
