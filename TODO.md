# OpenCashFlow – UI & Open Source First (Tabler)

## ✅ PHASE 0: TABLER THEME (PUBLISHING BLOCKER)
**Goal**: Replace any non-redistributable UI assets with a fully open-source theme so the repo can be published safely.

### 0.1 Choose a default OSS theme
- **Decision**: Use **Tabler (MIT, Bootstrap-based)** as the default UI theme.
- Keep all UI assets inside the repo (allowed) and remove/avoid any premium ThemeForest assets.

### 0.2 Add Tabler assets
- [ ] Download Tabler (source or dist) and place assets under:
  - `OpenCashFlow.Web/wwwroot/vendor/tabler/` (css/js/fonts/images)
- [ ] Ensure all references in Layout point to `/vendor/tabler/...`
- [ ] Keep Tabler license file in-repo:
  - `OpenCashFlow.Web/wwwroot/vendor/tabler/LICENSE`

### 0.3 Update Razor layout(s)
- [ ] Replace current layout with a Tabler-based layout:
  - `OpenCashFlow.Web/Views/Shared/_Layout.cshtml` (MVC)
  - or `OpenCashFlow.Web/Pages/Shared/_Layout.cshtml` (Razor Pages)
- [ ] Create partials for maintainability:
  - `_Navbar.cshtml`, `_Sidebar.cshtml`, `_Footer.cshtml`
- [ ] Make sure existing pages render without breaking routes.

### 0.4 Minimal pages for public repo readiness
- [ ] Ensure these key screens look clean and compile:
  - Login
  - Dashboard (KPI Billing Dashboard)
  - Plans / Subscriptions page
  - Billing page
- [ ] Remove unused demo pages/components to reduce noise.

### 0.5 Open-source hygiene (do BEFORE publishing)
- [ ] Add/verify `.gitignore` excludes any proprietary assets (old template folders, zips, etc.)
- [ ] Add `THIRD-PARTY-NOTICES.md` listing Tabler + other major OSS dependencies
- [ ] Add `README.md` quick-start (docker + env vars) and screenshots (optional)
- [ ] Add `LICENSE` for your project (choose MIT/Apache-2.0/GPL-3.0 depending on strategy)

---

# OpenCashFlow – Production Roadmap (Complete)

## 📊 Current Status
- ✅ KPI Billing Dashboard implemented
- ✅ Subscriptions and Plans management (UI + API)
- ✅ Advanced filters and saved views
- ✅ Companies integration with subscription history
- ⚠️ **CRITICAL**: No payment enforcement yet → unlimited free access!

---

## 🎯 PHASE 1: STRIPE INTEGRATION (TOP PRIORITY)
**Goal**: Enable real payments and block unpaid access  
**Estimated time**: 8–10 hours

### 1.1 Stripe Infrastructure Setup
- [x] Install Stripe.NET SDK (`Stripe.net` NuGet package)
  - [x] `dotnet add OpenCashFlow.API/OpenCashFlow.API.csproj package Stripe.net`
  - [x] Run `dotnet restore OpenCashFlow.sln` to validate there are no version conflicts
- [x] Add Stripe configuration in `OpenCashFlow.API/appsettings.json` and `appsettings.Development.json`
  ```json
  "Stripe": {
    "SecretKey": "sk_test_...",
    "PublishableKey": "pk_test_...",
    "WebhookSecret": "whsec_..."
  }
  ```
  - [x] Set real keys via environment variables (`STRIPE__SecretKey`, etc.) also in `docker-compose*.yml`
- [x] Create `OpenCashFlow.Shared/Options/StripeSettings.cs` for configuration binding
  - [x] Properties: `SecretKey`, `PublishableKey`, `WebhookSecret`, `DashboardUrl` (optional)
- [x] Register Stripe in `OpenCashFlow.API/Program.cs`
  - [x] `builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"))`
  - [x] `builder.Services.AddSingleton<IStripeClient>(_ => new StripeClient(stripeSettings.SecretKey))`
  - [x] Set `StripeConfiguration.ApiKey` at startup by reading `SecretKey`
  - [x] Validate required keys and log warnings if missing

### 1.2 Extend Database Models
- [x] Add fields to `Company`:
  - `StripeCustomerID` (string, nullable)
  - `StripeDefaultPaymentMethodID` (string, nullable)
  - `BillingEmail` (string, nullable)
- [x] Add fields to `Company_Subscription`:
  - `StripeSubscriptionID` (string, nullable)
  - `StripePriceID` (string, nullable)
  - `StripeInvoiceID` (string, nullable) for last invoice
- [x] Add fields to `Plan`:
  - `StripeProductID` (string, nullable)
  - `StripePriceID` (string, nullable) for default price
- [x] Create migration and update database

### 1.3 Service Layer – StripeService
- [x] Create `IStripeService` interface in `Services/Interfaces/`
- [x] Implement `StripeService` methods:

  **Customer Management**
  - [x] `CreateCustomerAsync(Company company)` → returns Customer
  - [x] `UpdateCustomerAsync(string customerId, Company company)`
  - [x] `GetCustomerAsync(string customerId)`
  - [x] `DeleteCustomerAsync(string customerId)`
  - [x] `AddPaymentMethodAsync(string customerId, string paymentMethodId)`
  - [x] `SetDefaultPaymentMethodAsync(string customerId, string paymentMethodId)`

  **Subscription Management**
  - [x] `CreateSubscriptionAsync(string customerId, string priceId, SubscriptionCreateOptions options)`
  - [x] `UpdateSubscriptionAsync(string subscriptionId, SubscriptionUpdateOptions options)`
  - [x] `CancelSubscriptionAsync(string subscriptionId, bool immediately)`
  - [x] `ReactivateSubscriptionAsync(string subscriptionId)`
  - [x] `ChangeSubscriptionPlanAsync(string subscriptionId, string newPriceId)`
  - [x] `GetSubscriptionAsync(string subscriptionId)`

  **Payment & Checkout**
  - [x] `CreateCheckoutSessionAsync(Company company, Plan plan, string successUrl, string cancelUrl)`
  - [x] `CreateCustomerPortalSessionAsync(string customerId, string returnUrl)`
  - [x] `CreatePaymentIntentAsync(decimal amount, string currency, string customerId)`

  **Invoice & Billing**
  - [x] `GetInvoiceAsync(string invoiceId)`
  - [x] `GetUpcomingInvoiceAsync(string customerId)`
  - [x] `ListInvoicesAsync(string customerId, int limit)`
  - [x] `SendInvoiceAsync(string invoiceId)`

- [x] Handle retry logic with Polly for Stripe API calls
- [x] Implement detailed logging for each Stripe call
- [x] Handle idempotency keys for critical operations

### 1.4 Stripe ↔ Database Synchronization
- [x] Create `StripeSyncService` to synchronize data:
  - [x] `SyncCustomerFromStripeAsync(string stripeCustomerId)` → updates Company
  - [x] `SyncSubscriptionFromStripeAsync(string stripeSubscriptionId)` → updates Company_Subscription
  - [x] `SyncPlanFromStripeAsync(string stripeProductId)` → updates Plan
  - [x] `SyncAllPlansAsync()` → bulk import from Stripe
  - [x] `EnsureCustomerExistsAsync(Guid companyId)` → creates if missing

### 1.5 Webhook Handler
- [x] Create `StripeWebhookController` endpoint `POST /api/stripe/webhook`
- [x] Implement signature verification using `WebhookSecret`
- [x] Implement handlers for Stripe events:

  **Subscription Events**
  - [x] `customer.subscription.created` → create Company_Subscription
  - [x] `customer.subscription.updated` → update status
  - [x] `customer.subscription.deleted` → mark as cancelled
  - [x] `customer.subscription.trial_will_end` → trigger notification

  **Payment Events**
  - [x] `payment_intent.succeeded` → confirm payment
  - [x] `payment_intent.payment_failed` → mark failed, attempt recovery
  - [x] `payment_method.attached` → store payment method
  - [x] `payment_method.detached` → remove payment method

  **Invoice Events**
  - [x] `invoice.created` → store invoice reference
  - [x] `invoice.finalized` → generate and store PDF
  - [x] `invoice.paid` → update payment status
  - [x] `invoice.payment_failed` → notify + retry
  - [x] `invoice.payment_action_required` → notify customer (3D Secure)

  **Customer Events**
  - [x] `customer.updated` → sync customer data
  - [x] `customer.deleted` → handle deletion

- [x] Implement idempotency for duplicate events (persist processed `stripe_event_id`)
- [x] Detailed logging of all incoming events
- [x] Error handling and retry for failed event processing

### 1.6 Billing API Endpoints
- [x] Extend `BillingController` with:
  - [x] `POST /v1/Billing/CreateCheckout` → create Stripe Checkout session
  - [x] `POST /v1/Billing/CreatePortalSession` → create Customer Portal session
  - [x] `POST /v1/Billing/Subscribe` → create company subscription
  - [x] `PUT /v1/Billing/Subscription/{id}/Cancel` → cancel subscription
  - [x] `PUT /v1/Billing/Subscription/{id}/Reactivate` → reactivate subscription
  - [x] `PUT /v1/Billing/Subscription/{id}/ChangePlan` → change plan
  - [x] `GET /v1/Billing/Subscription/{id}/Invoice` → invoice details
  - [x] `GET /v1/Billing/Subscription/{id}/UpcomingInvoice` → upcoming invoice preview
  - [x] `POST /v1/Billing/PaymentMethod` → add payment method
  - [x] `DELETE /v1/Billing/PaymentMethod/{id}` → remove payment method

### 1.7 Middleware – Subscription Check
- [x] Create `SubscriptionAuthorizationMiddleware`:
  - [x] Verify the Company has an active subscription
  - [x] Allow states (Active, Trialing, PastDue)
  - [x] Block access if (Canceled, Expired, Suspended)
  - [x] Grace period for PastDue (e.g., 3 days)
  - [x] Admin bypass
  - [x] Log blocked attempts
- [x] Register middleware after Authentication
- [x] Create `/subscription-expired` page for redirects

### 1.8 Admin UI – Subscription Management
- [x] Page `/Admin/Billing/ManageSubscription/{subscriptionId}`:
  - [x] Show Stripe details (customer ID, subscription ID)
  - [x] Dual mode: Stripe subscriptions and manual subscriptions (bank transfer/cash)
  - [x] Show payment methods (Stripe only)
  - [x] “View in Stripe Dashboard” button (direct link for Stripe subscriptions)
  - [x] Admin actions for Stripe subscriptions:
    - [x] Force sync from Stripe
    - [x] Extend trial
    - [x] Apply credit
    - [x] Send invoice manually
  - [x] Admin actions for manual subscriptions:
    - [x] Extend subscription (1, 3, 6, 12, 24 months)
    - [x] Update status manually
    - [x] Activate subscription
  - [x] Common actions:
    - [x] Change plan with proration (Stripe) or direct (manual)
    - [x] Cancel subscription (with confirmation + reason)
  - [x] Webhook event history for this subscription
  - [x] Admin operation logs (audit trail) – full UI for subscription lifecycle events

### 1.9 User-Facing UI – Billing Portal
- [x] Page `/App/Billing`:
  - [x] Current subscription summary (plan, cost, next renewal)
  - [x] Status badges (Active, Trial, Paused, Cancelled, Expired, Suspended)
  - [x] “Manage Payments” → Stripe Customer Portal
  - [x] Invoice history with PDF download
  - [x] Upcoming invoice preview
  - [x] Payment failed banner with CTA “Update Payment Method”
  - [x] Upgrade/Downgrade modal with plan change form
  - [x] Manual subscriptions support (no Stripe)
  - [x] Discount/promo code display
  - [x] Responsive (mobile + desktop)

### 1.10 Testing & Validation
- [x] Complete testing documentation:
  - [x] [TESTING-STRIPE.md](TESTING-STRIPE.md) – full test scenarios
  - [x] [PRE-TESTING-CHECKLIST.md](PRE-TESTING-CHECKLIST.md) – configuration checklist
  - [x] [.env.example](.env.example) – environment template
  - [x] [scripts/start-dev.sh](scripts/start-dev.sh) – dev environment startup script
- [x] Test cards documented (**Stripe test-only**):
  - [x] Success: `4242 4242 4242 4242`
  - [x] Failure: `4000 0000 0000 0341`
  - [x] 3D Secure: `4000 0025 0000 3155`
  - [x] Additional test cards for specific scenarios
- [x] Test scenarios defined:
  - [x] Test 1: Successful subscription creation
  - [x] Test 2: Payment failed + notifications
  - [x] Test 3: 3D Secure authentication flow
  - [x] Test 4: Subscription cancellation (Admin + Customer)
  - [x] Test 5: Plan change (upgrade/downgrade with proration)
  - [x] Test 6: Trial expiry and conversion
  - [x] Test 7: Manual subscriptions (no Stripe)
- [x] Webhook validation instructions using Stripe CLI
- [x] Troubleshooting guide
- [x] Final verification checklist

**Note**: Manual tests must be executed by a developer/operator following the documentation. The infrastructure and the guide are complete.

---

## 🔐 PHASE 2: SECURITY & ACCESS CONTROL
**Goal**: Ensure security and traceability  
**Estimated time**: 4–5 hours

### 2.1 Admin User Management ✅ COMPLETED
- [x] **SECURITY.md** – complete security documentation (600+ lines)
- [x] API endpoints `/v1/Admin/Users`:
  - [x] `GET /Users` → admin user list with filters (role, status, last login, company, date range, pagination)
  - [x] `GET /Users/{id}` → user detail with audit log
  - [x] `POST /Users` → create new admin
  - [x] `PUT /Users/{id}` → update user
  - [x] `POST /Users/{id}/Lock` → lock access
  - [x] `POST /Users/{id}/Unlock` → unlock access
  - [x] `POST /Users/{id}/ResetPassword` → reset password
  - [x] `PUT /Users/{id}/Roles` → assign/revoke roles
  - [x] `GET /Roles` → list roles with user count
  - [x] `DELETE /Users/{id}` → soft delete user and company_staff
- [x] Backend:
  - [x] 7 DTOs with validation (User_List, User_Detail, User_Create, User_Update, User_Filter, User_ResetPassword, User_Roles, Role)
  - [x] `IUserManagementService` interface (9 methods)
  - [x] `UserManagementService` implementation (~560 lines)
  - [x] UsersController API (10 RESTful endpoints)
  - [x] Integration: AspNetUser_DS, AspNetRole_DS, AspNetUserRole_DS, Company_Staff_DS
  - [x] Password hashing (SHA256)
  - [x] Audit log generation from existing data
  - [x] Advanced filters and pagination
  - [x] Multi-tenancy support
- [x] Admin UI `/Admin/Users`:
  - [x] MVC controller actions (Index, Details, Create, Edit, Lock, Unlock, ResetPassword, UpdateRoles, Delete)
  - [x] `UserManagementAPIService` HTTP client (~330 lines)
  - [x] Index view with table, filters, pagination
  - [x] Status badges (Active, Inactive, Locked)
  - [x] Role badges + email confirmation badge
  - [x] Quick actions (details, edit)
  - [x] Companies + Roles integration

**Technical notes**
- Fixed property names: AspNetRole.RoleName, Company.CompanyName
- DbSet names use `_DS` suffix (AspNetUser_DS, AspNetRole_DS, Company_Staff_DS)
- AspNetUserRole uses UserID/RoleID (not UserId/RoleId)
- Company_Staff has no navigation to Company (separate fetch)
- Authorization: `[Authorize(Roles = "Administrator")]`

### 2.2 Centralized Audit Log ✅ COMPLETED (Backend)
- [x] Created `Admin_AuditLog` model + migration
- [x] Enum `AuditEventType` with 50+ events (Login, UserCreated, SubscriptionChanged, PaymentFailed, etc.)
- [x] `IAuditLogService` + `AuditLogService` implementation:
  - [x] `LogEventAsync` using a fire-and-forget pattern
  - [x] `GetAuditLogsAsync` with filters + pagination
  - [x] `GetAuditLogDetailAsync` with full JSON payload
  - [x] `ExportAuditLogAsync` for CSV export
  - [x] Automatic capture: IP, UserAgent, Username
- [x] API endpoints `/v1/Admin/AuditLog` (GET list, GET detail, GET export)
- [x] 3 DTOs: AuditLog_List, AuditLog_Detail, AuditLog_Filter
- [x] Admin UI `/Admin/AuditLog` + integration with critical operations

### 2.3 Security Alerts
- [ ] Implement `SecurityAlertService`:
  - [ ] Monitor suspicious events:
    - [ ] Repeated failed logins (>5 in 15 min)
    - [ ] Logins from new IP/location
    - [ ] Critical role changes (e.g., User → Admin)
    - [ ] Payment override actions
    - [ ] Mass deletions
  - [ ] Trigger notifications via email/Slack for critical alerts
  - [ ] Admin alert dashboard with red badge when alerts exist

### 2.4 MFA (Multi-Factor Authentication)
- [ ] Configure ASP.NET Identity MFA:
  - [ ] Enable TOTP (Time-based One-Time Password)
  - [ ] Generate QR code for authenticator apps
  - [ ] Backup codes for recovery

- [ ] API `/v1/Auth/MFA`:
  - [ ] `POST /EnableMFA` → generate secret and QR code
  - [ ] `POST /VerifyMFA` → confirm setup with code
  - [ ] `POST /DisableMFA` → require password + code
  - [ ] `GET /BackupCodes` → generate new recovery codes

- [ ] User UI `/Profile/Security`:
  - [ ] Toggle “Enable 2FA”
  - [ ] QR code for authenticator (Google Auth, Authy, etc.)
  - [ ] Backup codes list (show once, then masked)
  - [ ] “Generate new backup codes” button

- [ ] Admin UI – MFA Management:
  - [ ] View MFA status per user
  - [ ] “Disable MFA” button for emergencies (requires confirmation + reason)
  - [ ] Log enable/disable MFA in audit trail

---

## 📄 PHASE 3: ITALIAN E-INVOICING (Post-Stripe)
**Goal**: Handle Italian electronic invoices (in addition to Stripe)  
**Estimated time**: 5–6 hours

### 3.1 Invoice Models
- [ ] Create `Invoices` table:
  - [ ] Fields: `InvoiceID`, `InvoiceNumber`, `TenantID`, `StripeInvoiceID`, `IssueDate`, `DueDate`, `Status` (Draft, Issued, Sent, Paid, Overdue, Void), `TotalAmount`, `TaxAmount`, `Currency`, `InvoicePDF` (blob/path), `SDIStatus`, `SDIIdentifier`

- [ ] Create `Invoice_Items` table:
  - [ ] Fields: `ItemID`, `InvoiceID`, `Description`, `Quantity`, `UnitPrice`, `TaxRate`, `TotalAmount`

### 3.2 Invoice Generation
- [ ] Implement `InvoiceService`:
  - [ ] `GenerateInvoiceFromStripeAsync(string stripeInvoiceId)` → sync from Stripe
  - [ ] `CreateManualInvoiceAsync(Company company, items, metadata)` → custom invoice
  - [ ] `GeneratePDFAsync(Invoice invoice)` → generate PDF via template
  - [ ] `MarkAsPaidAsync(Guid invoiceId, DateTime paidDate)`
  - [ ] `VoidInvoiceAsync(Guid invoiceId, string reason)`
  - [ ] `SendInvoiceEmailAsync(Guid invoiceId, string toEmail)`

- [ ] Invoice PDF template:
  - [ ] Company logo
  - [ ] Issuer tax data (your company)
  - [ ] Customer tax data (Company)
  - [ ] Progressive invoice number
  - [ ] Line details with VAT
  - [ ] Totals (net, VAT, gross)
  - [ ] IBAN for bank transfer
  - [ ] Notes/payment terms

### 3.3 SDI Integration (Sistema di Interscambio)
- [ ] Evaluate SDI providers (e.g., Fatture in Cloud, Aruba, TeamSystem):
  - [ ] API for sending FatturaPA XML invoices
  - [ ] Receive status notifications (accepted, rejected, delivered)
  - [ ] Split payment handling

- [ ] Implement `SDIService`:
  - [ ] `SendToSDIAsync(Invoice invoice)` → generate XML and send
  - [ ] `GetSDIStatusAsync(string sdiIdentifier)` → status check
  - [ ] Webhook handler for SDI notifications

- [ ] Manage SDI invoice states in Admin UI

### 3.4 Invoice API
- [ ] Endpoints `/v1/Invoices`:
  - [ ] `GET /Invoices` → list with filters (company, status, date range)
  - [ ] `GET /Invoices/{id}` → invoice detail
  - [ ] `GET /Invoices/{id}/PDF` → download PDF
  - [ ] `POST /Invoices` → create manual invoice
  - [ ] `PUT /Invoices/{id}` → update (draft only)
  - [ ] `POST /Invoices/{id}/Issue` → issue invoice (generate number)
  - [ ] `POST /Invoices/{id}/Send` → send via email
  - [ ] `POST /Invoices/{id}/SendSDI` → send to SDI
  - [ ] `POST /Invoices/{id}/Void` → void invoice
  - [ ] `GET /Invoices/Export` → export CSV/Excel

### 3.5 Admin UI – Invoice Archive
- [ ] Page `/Admin/Invoices`:
  - [ ] Invoice table: number, customer, date, amount, status, SDI status
  - [ ] Filters: period, status, customer, amount range
  - [ ] Search by invoice number or customer
  - [ ] Actions: download PDF, email send, SDI send, void
  - [ ] Multi-select for export or batch operations
  - [ ] “New Invoice” button (manual)

- [ ] “Invoice Detail” modal:
  - [ ] Full invoice info
  - [ ] Status/action history
  - [ ] Email/SDI send logs
  - [ ] Link to Stripe invoice (if present)
  - [ ] Link to payment/subscription

### 3.6 Customer UI – My Invoices
- [ ] Page `/App/Invoices`:
  - [ ] List paid/unpaid invoices
  - [ ] PDF download
  - [ ] Payment details (date, method, reference)
  - [ ] Filters by year/period

---

## 🔔 PHASE 4: NOTIFICATIONS SYSTEM
**Goal**: Automations and proactive communication  
**Estimated time**: 3–4 hours

### 4.1 Notification Infrastructure
- [ ] Choose providers:
  - [ ] Email: SendGrid, AWS SES, Mailgun, Postmark
  - [ ] SMS (optional): Twilio
  - [ ] Slack: webhook integration
  - [ ] Push (optional): Firebase, OneSignal

- [ ] Create `NotificationService`:
  - [ ] `SendEmailAsync(to, subject, body, template)`
  - [ ] `SendTemplatedEmailAsync(to, templateId, data)`
  - [ ] `SendSMSAsync(to, message)`
  - [ ] `SendSlackNotificationAsync(webhook, message)`
  - [ ] Queue system for batch sends (Hangfire, Azure Queue, RabbitMQ)

### 4.2 Email Templates
- [ ] Responsive HTML templates:
  - [ ] Welcome email
  - [ ] Subscription confirmation
  - [ ] Trial expiring (7/3/1 days)
  - [ ] Upcoming renewal (7 days before)
  - [ ] Payment succeeded
  - [ ] Payment failed (with “update method” link)
  - [ ] Invoice available
  - [ ] Subscription cancelled
  - [ ] Plan change confirmation
  - [ ] Password reset
  - [ ] New user invitation

- [ ] Dynamic variables: `{{CompanyName}}`, `{{Amount}}`, `{{DueDate}}`, `{{ActionLink}}`, etc.

### 4.3 Automated Triggers
- [ ] Implement `NotificationTriggerService`:
  - [ ] Listen to Stripe webhook events
  - [ ] Scheduled proactive checks:
    - [ ] Trial expiring (daily)
    - [ ] Upcoming renewals (daily)
    - [ ] Overdue invoices (daily)
  - [ ] Triggers from admin actions (e.g., invite user)

- [ ] Configure Hangfire recurring jobs:
  - [ ] `RecurringJob.AddOrUpdate("check-trials", () => NotificationService.SendTrialExpiringNotifications(), Cron.Daily)`
  - [ ] `RecurringJob.AddOrUpdate("check-renewals", () => NotificationService.SendRenewalReminders(), Cron.Daily)`

### 4.4 Notification Center (UI)
- [ ] In-app notifications:
  - [ ] Unread badge count
  - [ ] Dropdown with recent notifications
  - [ ] `/Notifications` page with full history
  - [ ] Filters: read/unread, type, date
  - [ ] Actions: mark as read, delete

- [ ] Preferences `/Profile/Notifications`:
  - [ ] Toggles by channel (email, SMS, in-app)
  - [ ] Digest frequency (immediate, daily, weekly)

### 4.5 Admin – Notification Management
- [ ] UI `/Admin/Notifications`:
  - [ ] Send dashboard: totals, open rates, clicks, bounces
  - [ ] Template list with preview
  - [ ] Visual template editor (optional)
  - [ ] Test send
  - [ ] Send logs (sent, delivered, opened, clicked, bounced)
  - [ ] Slack webhook configuration

---

## 💎 PHASE 5: PROMOTIONS & MARKETING
**Goal**: Tools for acquisition and retention  
**Estimated time**: 4–5 hours

### 5.1 Promo Code Management
- [ ] `Promotion_Codes` table:
  - [ ] Fields: `PromoID`, `Code`, `Type` (Percentage, FixedAmount), `Value`, `Currency`, `MaxUses`, `UsedCount`, `ValidFrom`, `ValidTo`, `ApplicablePlans` (JSON array), `IsActive`, `CreatedBy`, `CreatedAt`

- [ ] `Promotion_Usage` table:
  - [ ] Fields: `UsageID`, `PromoID`, `TenantID`, `SubscriptionID`, `AppliedAt`, `DiscountAmount`

### 5.2 Promo Service
- [ ] Implement `PromotionService`:
  - [ ] `ValidatePromoCodeAsync(string code, Guid planId)` → validate
  - [ ] `ApplyPromoCodeAsync(string code, Guid subscriptionId)` → apply discount
  - [ ] `CreatePromoCodeAsync(PromoDetails)` → create
  - [ ] `DeactivatePromoCodeAsync(Guid promoId)` → deactivate
  - [ ] `GetPromoUsageStatsAsync(Guid promoId)` → usage stats

- [ ] Integrate with Stripe Coupons/Promotion Codes

### 5.3 Promotions API
- [ ] Endpoints `/v1/Promotions`:
  - [ ] `GET /Promotions` → list with filters (active, expired, type)
  - [ ] `GET /Promotions/{id}` → details + stats
  - [ ] `POST /Promotions` → create
  - [ ] `PUT /Promotions/{id}` → update (only if unused)
  - [ ] `DELETE /Promotions/{id}` → deactivate
  - [ ] `POST /Promotions/Validate` → validate code (for checkout UI)
  - [ ] `POST /Promotions/{id}/Apply` → apply to a specific company (admin)
  - [ ] `GET /Promotions/{id}/Usage` → usage history

### 5.4 Admin UI – Promotions
- [ ] Page `/Admin/Promotions`:
  - [ ] Promo table: code, type, value, validity, usage, status
  - [ ] Status badges: active (green), expired (gray), exhausted (red)
  - [ ] Filters: status, type, validity date
  - [ ] “New Promo Code” button
  - [ ] Actions: edit, deactivate, duplicate, export usage

- [ ] “New Promo” form:
  - [ ] Code (auto-generate option)
  - [ ] Discount type (%, fixed)
  - [ ] Value
  - [ ] Validity dates
  - [ ] Max uses
  - [ ] Applicable plans (multi-select)
  - [ ] Stripe options: duration (once, forever, repeating)

- [ ] Promo detail:
  - [ ] KPIs: total uses, revenue impact, conversion rate
  - [ ] Usage timeline chart
  - [ ] Companies that used the promo
  - [ ] “Apply manually” button

### 5.5 Checkout UI – Promo Application
- [ ] In checkout/subscription flow:
  - [ ] “Promo code” input
  - [ ] “Apply” button with real-time validation
  - [ ] Show applied discount and updated total
  - [ ] Remove promo if invalid with error message

### 5.6 History & Analytics
- [ ] Report `/Admin/Reports/Promotions`:
  - [ ] Top promo codes (top 10)
  - [ ] Revenue generated/lost by promos
  - [ ] Conversion rate by promo
  - [ ] Usage distribution by plan
  - [ ] CSV export

---

## 🤖 PHASE 6: AUTOMATIONS & RULES
**Goal**: Reduce manual work with automated business logic  
**Estimated time**: 4–5 hours

### 6.1 Automation Rule Models
- [ ] `Automation_Rules` table:
  - [ ] Fields: `RuleID`, `Name`, `Description`, `Trigger` (TrialExpiring, PaymentFailed, SubscriptionCreated, etc.), `Conditions` (JSON), `Actions` (JSON), `IsActive`, `Priority`, `CreatedBy`, `CreatedAt`

- [ ] `Automation_Executions` table:
  - [ ] Fields: `ExecutionID`, `RuleID`, `TriggeredAt`, `Status`, `Result`, `ErrorMessage`, `TargetResourceID`

### 6.2 Automation Engine
- [ ] Implement `AutomationEngine`:
  - [ ] `EvaluateRulesAsync(string trigger, object context)` → evaluate matching rules
  - [ ] `ExecuteRuleAsync(Rule rule, object context)` → execute actions
  - [ ] Supported actions:
    - [ ] `SendEmail(template, recipient)`
    - [ ] `SendNotification(message, user)`
    - [ ] `ExtendTrial(days)`
    - [ ] `ApplyDiscount(promoCode)`
    - [ ] `ChangeSubscriptionStatus(status)`
    - [ ] `CreateTask(assignee, description)` → manual follow-up
    - [ ] `WebhookCall(url, payload)` → external integration
    - [ ] `UpdateCustomField(field, value)`

- [ ] Conditions engine (JSON-based)
  ```json
  {
    "all": [
      {"field": "subscription.status", "operator": "equals", "value": "trialing"},
      {"field": "subscription.trial_end", "operator": "less_than_days", "value": 3}
    ]
  }
  ```

### 6.3 Preconfigured Rules
- [ ] Common rules:
  - [ ] **Trial Extension**: if payment fails during trial, extend +7 days + notify
  - [ ] **Automatic Retry**: on payment failure, retry after 3/7/14 days + reminders
  - [ ] **Downgrade Prevention**: block downgrade if used features exceed plan limits
  - [ ] **Referral Reward**: referral code → 20% discount for 3 months
  - [ ] **Win-back Campaign**: after cancellation, send sequence after 7/30/60 days
  - [ ] **Usage Alert**: notify at 80% of plan limits + suggest upgrade
  - [ ] **Onboarding Check-in**: after 7 days, low usage → tutorial email

### 6.4 Automations API
- [ ] Endpoints `/v1/Admin/Automations`:
  - [ ] `GET /Rules` → list with filters
  - [ ] `GET /Rules/{id}` → detail
  - [ ] `POST /Rules` → create
  - [ ] `PUT /Rules/{id}` → update
  - [ ] `POST /Rules/{id}/Activate` → activate
  - [ ] `POST /Rules/{id}/Deactivate` → deactivate
  - [ ] `DELETE /Rules/{id}` → delete
  - [ ] `GET /Rules/{id}/Executions` → executions history
  - [ ] `POST /Rules/{id}/Test` → test with mock data

### 6.5 Admin UI – Automations
- [ ] Page `/Admin/Automations`:
  - [ ] Rules list: name, trigger, status, recent executions (24h)
  - [ ] On/off toggle
  - [ ] Filters: trigger, status, created by
  - [ ] “New rule” button

- [ ] No-code rule builder (optional):
  - [ ] “When” (trigger)
  - [ ] “If” (conditions) builder
  - [ ] “Then” (actions) sequence builder
  - [ ] Priority
  - [ ] “Enable rule” toggle
  - [ ] “Test” button with sample data

- [ ] Executions dashboard:
  - [ ] executions over time (per rule)
  - [ ] success rate
  - [ ] recent errors
  - [ ] business impact (revenue saved, churn reduced, etc.)

---

## 📊 PHASE 7: ANALYTICS & REPORTING
**Goal**: Data-driven insights  
**Estimated time**: 3–4 hours

### 7.1 Key Business Metrics
- [ ] Implement `AnalyticsService`:
  - [ ] MRR (Monthly Recurring Revenue)
  - [ ] ARR (Annual Recurring Revenue)
  - [ ] Churn Rate
  - [ ] LTV (Customer Lifetime Value)
  - [ ] CAC (Customer Acquisition Cost) – if marketing tracking exists
  - [ ] ARPU (Average Revenue Per User)
  - [ ] Retention Rate
  - [ ] Trial-to-Paid Conversion Rate
  - [ ] Revenue churn vs customer churn
  - [ ] Expansion MRR (upgrades - downgrades)

### 7.2 Admin Analytics Dashboard
- [ ] Page `/Admin/Analytics`:
  - [ ] KPI cards:
    - [ ] MRR (trend vs last month)
    - [ ] projected ARR
    - [ ] churn rate
    - [ ] active customers
  - [ ] Charts:
    - [ ] MRR trend (12 months)
    - [ ] New vs Lost MRR (waterfall)
    - [ ] Cohort analysis
    - [ ] Revenue by plan
    - [ ] Funnel (trial → paid)
  - [ ] Filters: period, plan, customer segment

### 7.3 Export & Integrations
- [ ] Export endpoints:
  - [ ] `/Admin/Analytics/Export/MRR` → monthly CSV
  - [ ] `/Admin/Analytics/Export/Customers` → customers CSV
  - [ ] `/Admin/Analytics/Export/Subscriptions` → subscriptions CSV
  - [ ] `/Admin/Analytics/Export/Payments` → transactions CSV

- [ ] BI integrations (optional):
  - [ ] Webhook to Google Sheets / Airtable
  - [ ] API for Tableau / Power BI
  - [ ] Segment.com integration

### 7.4 Automated Reports
- [ ] Weekly/monthly admin email reports:
  - [ ] MRR summary
  - [ ] new/lost customers
  - [ ] churn alerts
  - [ ] top growing/shrinking accounts
  - [ ] action items (failed payments, expiring trials, etc.)

---

## 🚀 PHASE 8: OPTIMIZATION & SCALABILITY
**Goal**: Performance and growth readiness  
**Estimated time**: ongoing

### 8.1 Caching Strategy
- [ ] Redis caching:
  - [ ] KPI dashboard (1 min cache)
  - [ ] Plan list (5 min cache)
  - [ ] Company data (10 min cache + invalidation on update)
  - [ ] User sessions

- [ ] Cache warming
- [ ] Smart invalidation (event-based)

### 8.2 Database Optimization
- [ ] Indexes:
  - [ ] `Company_Subscription` on `(TenantID, RenewalStatus)`
  - [ ] `Company_Subscription` on `NextBillingDate`
  - [ ] `Invoices` on `(TenantID, Status, IssueDate)`
  - [ ] `Admin_AuditLog` on `(UserID, EventType, Timestamp)`

- [ ] Partitioning for large tables (audit log, analytics)
- [ ] Auto-archiving old data (>2 years)

### 8.3 Background Jobs
- [ ] Hangfire:
  - [ ] expiring subscriptions job
  - [ ] Stripe sync job (hourly)
  - [ ] batch notifications job
  - [ ] analytics calculations job (every 6h)
  - [ ] cache cleanup job
  - [ ] automated exports job

- [ ] Hangfire dashboard (`/hangfire`)

### 8.4 Monitoring & Observability
- [ ] Application Insights / Sentry
- [ ] Health checks `/health`:
  - [ ] DB connectivity
  - [ ] Stripe API reachability
  - [ ] Redis connectivity (if used)
  - [ ] Email provider status

- [ ] Structured logging (Serilog):
  - [ ] environment-based levels (Debug in dev, Warning in prod)
  - [ ] Correlation ID
  - [ ] log enrichment with user/company context

- [ ] Performance metrics:
  - [ ] API response times (p50/p95/p99)
  - [ ] DB query times
  - [ ] Stripe API latency
  - [ ] background job durations

### 8.5 Testing
- [ ] Unit tests:
  - [ ] StripeService
  - [ ] PromotionService
  - [ ] AutomationEngine
  - [ ] NotificationService

- [ ] Integration tests:
  - [ ] Stripe webhook end-to-end flow
  - [ ] Subscription lifecycle (create → update → cancel)
  - [ ] Payment failure → retry logic

- [ ] E2E tests (Playwright):
  - [ ] signup → checkout → active subscription
  - [ ] payment method update
  - [ ] plan upgrade/downgrade
  - [ ] admin critical flows

---

## 📋 FINAL PRE-PRODUCTION CHECKLIST

### Security
- [ ] Enforce HTTPS everywhere
- [ ] Correct CORS configuration
- [ ] Rate limiting on public APIs
- [ ] SQL injection prevention (parameterized queries)
- [ ] XSS protection (sanitize input)
- [ ] CSRF tokens on forms
- [ ] Secrets in a vault (not in code!)
- [ ] Stripe webhook signature verification
- [ ] Admin panel behind IP allow-list or VPN (optional)

### Compliance
- [ ] GDPR compliance:
  - [ ] Updated privacy policy
  - [ ] Cookie banner
  - [ ] Right-to-be-forgotten (delete account)
  - [ ] User data export
  - [ ] Consent tracking

- [ ] Updated Terms of Service
- [ ] Italian invoicing compliant with regulations

### Performance
- [ ] CDN for static assets
- [ ] Image optimization (WebP, lazy loading)
- [ ] JS/CSS minification
- [ ] Gzip compression
- [ ] Automated DB backups (daily)
- [ ] Disaster recovery plan

### Monitoring
- [ ] Uptime monitoring (UptimeRobot, Pingdom)
- [ ] Error alerting
- [ ] Log aggregation (ELK, Seq, CloudWatch)
- [ ] Business KPI dashboards for stakeholders

### Documentation
- [ ] API docs (Swagger/OpenAPI)
- [ ] Admin user guide
- [ ] Developer onboarding guide
- [ ] Runbook for common operations
- [ ] Incident response playbook

---

## 🗓️ Suggested Timeline

**Sprint 1 (Week 1–2)**: Phase 1 – Stripe integration  
**Sprint 2 (Week 3)**: Phase 2 – Security & access control  
**Sprint 3 (Week 4)**: Phase 3 – Italian e-invoicing  
**Sprint 4 (Week 5)**: Phase 4 – Notifications  
**Sprint 5 (Week 6)**: Phase 5 – Promotions & marketing  
**Sprint 6 (Week 7)**: Phase 6 – Automations & rules  
**Sprint 7 (Week 8)**: Phase 7 – Analytics & reporting  
**Sprint 8 (Week 9–10)**: Phase 8 – Optimization & final testing  

**Total estimate**: 8–10 weeks for a production-complete release

---

## 💡 Implementation Notes

### Recommended Architecture Patterns
- **Repository Pattern**: already in use — keep it
- **Service Layer**: business logic isolated from controllers
- **DTO Pattern**: separate DB models from API contracts
- **Event-driven**: consider MediatR for internal events
- **Background jobs**: Hangfire for async tasks

### Best Practices
- **Idempotency**: critical operations (payments, webhooks) must be idempotent
- **Retry logic**: exponential backoff for external API calls
- **Graceful degradation**: app should function even if Stripe/email provider is down
- **Feature flags**: gradual rollouts (LaunchDarkly or custom)
- **API versioning**: `/v1/`, `/v2/` for backwards compatibility

### Stripe Best Practices
- **Webhook reliability**: persist raw events before processing
- **Test mode**: heavily use in dev/staging
- **Customer Portal**: reduce development by using Stripe portal
- **Metadata**: store `TenantID`, `PlanID`, etc. in Stripe metadata
- **Subscription Schedule**: handle end-of-cycle downgrades

---

**Next step**: Do you want to start with Phase 2.3 (Security Alerts) or Phase 3 (Italian e-invoicing)?
