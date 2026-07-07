# Frontend CSP Cleanup

## Current State

The WebApp CSP is nonce-based and does not use `unsafe-inline` or `unsafe-eval`. This cleanup phase started reducing the amount of Razor-rendered inline code that still needs a nonce.

Initial inventory showed inline scripts/styles across shared layout, auth pages, payments, cash ledger, dashboard, company settings and employees. Runtime CDN usage is concentrated in payments and one employee edit locale script.

## Migrated In This Slice

- Shared authenticated session watchdog scripts were moved from `_Scripts.cshtml` and `_ScriptsFront.cshtml` to `wwwroot/js/components/session-watchdog.js`.
- The remote Inter font import was removed and replaced with local Geist font declarations in `wwwroot/css/components/shared-layout.css`.
- The development/staging environment badge style was moved from `_CommonMasterLayout.cshtml` to `shared-layout.css`.
- Footer year rendering was moved from inline JavaScript to server-side Razor.
- Register, reset-password and change-password page scripts were moved to static files under `wwwroot/js/pages`.

## Runtime CDN Residues

The following external runtime dependencies remain and keep `https://cdn.jsdelivr.net` in the CSP:

- `Views/Payments/Index.cshtml`: flatpickr CSS, flatpickr theme, flatpickr JavaScript, flatpickr Italian locale.
- `Views/Payments/Index.cshtml`: `@microsoft/signalr` browser script.
- `Views/Employees/Edit_Employee.cshtml`: flatpickr Italian locale.

README files under `wwwroot/libs/**` also mention CDN URLs, but those are vendored library documentation and are not runtime page dependencies.

## Inline Residues

The main remaining inline code is in:

- `Views/Home/Login.cshtml` and `Views/Home/FastLogin.cshtml`.
- `Views/Payments/Index.cshtml` and payment modal/list partials.
- `Views/Company/Company_CashLedger.cshtml`, security/settings/lookups pages.
- `Views/Employees/*.cshtml`.
- Dashboard components.
- SVG illustration `<style>` blocks and dynamic avatar/style attributes.

These are still covered by nonce where applicable. The next slices should migrate them page by page, starting with Payments and Company Cash Ledger because those are core operational flows.

## Next Steps

1. Vendor local flatpickr and SignalR assets or replace the references with already available local equivalents, then remove `https://cdn.jsdelivr.net` from CSP.
2. Extract `Views/Payments/Index.cshtml` scripts into `wwwroot/js/pages/payments-index.js`.
3. Replace `onclick` handlers in Payments, Cash Ledger and Employees with delegated event listeners.
4. Move static inline styles to page/component CSS and keep only validated dynamic values as `data-*` attributes or predefined classes.
