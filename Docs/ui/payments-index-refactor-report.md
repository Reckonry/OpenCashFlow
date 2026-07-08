# Payments Index Refactor Report

Date: 2026-07-07

Branch: `refactor/payments-index-view`

## Goal

Reduce maintainability debt in `src/OpenCashFlow.WebApp/Views/Payments/Index.cshtml` without changing visible behavior, routes, model binding, DOM IDs, CSS classes used by scripts, validation behavior, or JavaScript behavior.

## Line Count

| File | Before | After |
| --- | ---: | ---: |
| `src/OpenCashFlow.WebApp/Views/Payments/Index.cshtml` | 1208 | 1045 |

The extracted markup and CSS now live in smaller focused files. Total page-related lines are similar, but the main view is easier to scan and the reusable sections are isolated.

## Files Created

- `src/OpenCashFlow.WebApp/Views/Payments/Partials/_PaymentsPageHeader.cshtml`
- `src/OpenCashFlow.WebApp/Views/Payments/Partials/_PaymentsKpiWidgets.cshtml`
- `src/OpenCashFlow.WebApp/Views/Payments/Partials/_PaymentsDailyListCard.cshtml`
- `src/OpenCashFlow.WebApp/wwwroot/css/pages/payments-index.css`
- `Docs/ui/payments-index-refactor-report.md`

## Files Updated

- `src/OpenCashFlow.WebApp/Views/Payments/Index.cshtml`

## Behavior Preserved

Preserved stable DOM IDs used by the existing JavaScript:

- `openModalButton`
- `addPaymentWrapper`
- `reports`
- `compactViewToggle`
- `dayNav`
- `dayPrev`
- `dayPicker`
- `dayNext`
- `paymentsList`
- `modalContainer`

Preserved existing routes and AJAX endpoints:

- `/internal/Payment/CreatePaymentModal`
- `/internal/Payment/EditPaymentModal`
- `/internal/Payment/DeletePayment`
- `/internal/Payment/FilterPayments`
- `/internal/Payment/GetCashBalance`
- `/paymentHub`

The `dayPicker` inline width was moved to a page CSS class while keeping the same element ID and control behavior.

## Refactor Scope

Completed:

- moved page-specific CSS from inline `<style>` to `wwwroot/css/pages/payments-index.css`;
- extracted page header markup to `_PaymentsPageHeader.cshtml`;
- extracted KPI widgets to `_PaymentsKpiWidgets.cshtml`;
- extracted daily list card and day navigation to `_PaymentsDailyListCard.cshtml`;
- kept existing modal partial/component calls in the main view.

Deferred:

- large inline JavaScript extraction;
- CDN replacement for `flatpickr` and SignalR;
- extraction of SignalR row-rendering helpers;
- further cleanup of scripts embedded in `_PaymentsList.cshtml`.

## Remaining UI Debt

`Index.cshtml` still contains substantial inline JavaScript because the current script block mixes:

- Razor-localized strings;
- SignalR event handlers;
- AJAX modal loading;
- filter validation;
- daily navigation;
- Bootstrap popover behavior.

Moving that code safely should be done in a dedicated follow-up by first defining a stable data contract between Razor and static JavaScript through `data-*` attributes or a non-executable JSON configuration block.

## Verification

Completed locally:

```bash
git diff --check
dotnet build OpenCashFlow.sln --configuration Release --no-restore
dotnet test OpenCashFlow.sln --configuration Release --no-build
```

Results:

- `git diff --check`: passed.
- `dotnet build OpenCashFlow.sln --configuration Release --no-restore`: passed with 0 warnings and 0 errors.
- `dotnet test OpenCashFlow.sln --configuration Release --no-build`: passed. See current CI output and
  `Docs/testing/skipped-tests-backlog.md` for the current test count.
