# Project Architecture - Gestione Incassi

**Tech Stack**: .NET 9, ASP.NET Core, PostgreSQL, Entity Framework Core
**Updated**: 2025-10-13

---

## Technology Stack

### Backend
- **.NET 9** - Latest .NET framework
- **ASP.NET Core** - Web framework
- **Entity Framework Core** - ORM
- **PostgreSQL (Npgsql)** - Primary database
- **AutoMapper** - Object-object mapping
- **JWT Authentication** - Token-based auth
- **SignalR** - Real-time communication
- **Serilog** - Structured logging
- **Sentry** - Error tracking

### Frontend
- **ASP.NET Core MVC + Razor** - Server-side rendering
- **Bootstrap** - UI framework
- **SignalR Client** - Real-time updates

### Infrastructure
- **Docker** - Containerization
- **Azure** - Cloud hosting (target)
- **Stripe** - Payment processing (in progress)

---

## Solution Structure

The solution consists of 7 projects with clear separation of concerns:

### 1. GestioneIncassi-API
**Purpose**: RESTful API backend
**Responsibilities**:
- Controllers for all business operations
- Services layer
- Repository pattern
- JWT authentication
- Rate limiting
- Global exception handling

**Key Directories**:
```
GestioneIncassi-API/
├── Controllers/          # API endpoints
├── Services/            # Business logic
├── Repositories/        # Data access
├── Middleware/          # Custom middleware
└── Program.cs          # Startup configuration
```

### 2. GestioneIncassi-App
**Purpose**: Main customer-facing web application
**Responsibilities**:
- MVC controllers and views
- User interface
- API client via HttpClient
- Multi-language support (it, en, ro, es, de, fr, pt)
- Cookie-based authentication

**Communication Pattern**: Does NOT access database directly. All data via API HTTP calls.

### 3. GestioneIncassi-Admin
**Purpose**: Administrative portal
**Responsibilities**:
- Company management
- Subscription management
- Billing dashboard
- Plan configuration
- User administration

**Communication Pattern**: Same as App - uses API via HttpClient.

### 4. GestioneIncassi-Landing
**Purpose**: Public marketing/landing page
**Responsibilities**:
- Marketing content
- Lead generation
- Public-facing information

### 5. GestioneIncassi-Shared
**Purpose**: Shared library for all projects
**Contains**:
- **Models/** - Entity classes
- **DTOs/** - Data Transfer Objects
- **Data/ApplicationDbContext.cs** - EF Core context
- **Migrations/** - Database migrations
- **Mappings/** - AutoMapper profiles
- **Services/** - Shared business logic
- **Core/Configuration.cs** - Shared configuration

**Critical**: This is where database schema lives. All projects reference this.

### 6. GestioneIncassi-Test
**Purpose**: Unit and integration tests
**API Accessibility**: API exposes `public partial class Program {}` for testing

### 7. GestioneIncassi (Legacy)
**Status**: Original web project, appears to be legacy code
**Action**: Consider archiving or migration to new structure

---

## Key Architecture Patterns

### Multi-Project Communication Pattern

**App and Admin projects communicate with API via HTTP:**

1. **HttpClient Configuration** (in `Program.cs`):
```csharp
builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Account:API"]);
});
```

2. **BearerTokenHandler**:
   - Automatically attaches JWT token from cookie to all API requests
   - Registered in DI as HTTP message handler

3. **Service Layer** (e.g., `PaymentAPIService`):
   - Wraps API HTTP calls
   - Handles serialization/deserialization
   - Manages errors and retries

### Authentication Flow

1. User logs in via App (`HomeController.Login`)
2. API validates credentials, returns JWT token
3. Token stored in HTTP-only cookie (`AuthCookieName`)
4. JWT middleware reads token from cookie for both API and App
5. Multi-audience support: `JwtSettings:Audience` can be string or array

**Key Files**:
- API: `Program.cs` - JWT configuration
- App: `BearerTokenHandler.cs` - Token attachment
- Shared: `Core/Configuration.cs` - Cookie name constant

### Database & Migrations

**Entity Framework Core with PostgreSQL**:
- `ApplicationDbContext` in **GestioneIncassi-Shared/Data/**
- Connection string: env var `DEFAULT_CONN_STRING` or `appsettings.json`
- Migrations: **GestioneIncassi-Shared/Migrations/**
- Auto-migration on API startup: `Program.cs` applies pending migrations

**Seeding Strategy**:
- Default data in `OnModelCreating` (companies, users, roles, payment methods)
- Ensures minimum viable data exists

**Migration Commands**:
```bash
# Add migration (from solution root)
dotnet ef migrations add MigrationName --project GestioneIncassi-Shared --startup-project GestioneIncassi-API

# Apply migrations
dotnet ef database update --project GestioneIncassi-Shared --startup-project GestioneIncassi-API
```

### Multi-Tenancy Architecture

**Company Isolation**:
- Companies identified by `GICompanyID` (Guid)
- Users linked via `Company_Staff` table
- Most entities scoped to `GICompanyID`:
  - Payment
  - PaymentMethod
  - DocumentType
  - CashBalance
  - CashLedger

**Data Access Pattern**:
- Services filter by company from JWT claims (`HttpContext.User`)
- Repository layer enforces company-scoped queries
- Never expose cross-company data

### Cash Balance System (Critical Pattern)

**Optimistic Concurrency with PostgreSQL `xmin`**:

**Tables**:
- `CashBalance` - Single row per company, current balance
- `CashLedger` - Immutable transaction log with `Delta` amounts

**Concurrency Control**:
- `xmin` column (PostgreSQL row version)
- Unique constraint: `(CompanyId, RefType, RefId)` prevents duplicate entries
- Services MUST handle `DbUpdateConcurrencyException` with retry logic

**Transaction Pattern**:
```csharp
// Payment + CashBalance must be in same transaction
using var transaction = await _context.Database.BeginTransactionAsync();
try
{
    // Create payment
    // Update cash balance
    // Create ledger entry
    await transaction.CommitAsync();
}
catch (DbUpdateConcurrencyException)
{
    // Retry logic
}
```

### Service Layer Pattern

**Partial Classes by Feature Area**:

Example: `PaymentService` split into:
- `PaymentService.cs` - Core CRUD
- `PaymentService.PaymentMethod.cs` - Payment method management
- `PaymentService.DocumentType.cs` - Document type management
- `PaymentService.Calendar.cs` - Calendar integration
- `PaymentService.Cash.cs` - Cash balance logic
- `PaymentService.PaymentReports.cs` - Reporting

**Benefits**:
- Separation of concerns
- Easier to navigate
- Reduces merge conflicts
- Clear feature boundaries

**API Service Classes Follow Same Pattern**:
- `PaymentAPIService.PaymentMethod.cs`
- `PaymentAPIService.DocumentType.cs`
- etc.

---

## Configuration Management

### Environment-Specific Settings

Each project supports:
- `appsettings.json` - Base configuration
- `appsettings.Development.json` - Local dev overrides
- `appsettings.Staging.json` - Staging
- `appsettings.Production.json` - Production (not in repo)

**Priority**: Environment variables → Environment JSON → Base JSON

### Critical Configuration Keys

**API Project (`appsettings.json`)**:
```json
{
  "ConnectionStrings": {
    "DefaultConnectionString": "..." // or DEFAULT_CONN_STRING env var
  },
  "JwtSettings": {
    "SecretKey": "...",
    "Issuer": "...",
    "Audience": "..." // string or array
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:5001", "..."]
  },
  "EmailConfiguration": { /* SMTP */ },
  "Slack": {
    "WebhookUrl": "...",
    "TimeoutSeconds": 30
  }
}
```

**App/Admin Projects**:
```json
{
  "Account": {
    "API": "http://localhost:5000", // API base URL
    "Login": "/Home/Login" // Unauthorized redirect
  },
  "JwtSettings": { /* Must match API exactly */ }
}
```

---

## Logging & Error Handling

### Serilog Configuration

**Output**:
- File: `../Logs/log-APP-.txt` (daily rolling)
- Console: Development only
- Sentry: Integrated via `builder.WebHost.UseSentry()`

**Log Levels**:
- Development: Debug
- Production: Warning

**Format**: `yyyy-MM-dd HH:mm:ss.fff zzz`

### Global Exception Handler (API)

**Middleware Maps Exceptions**:
- `ArgumentException` → 400 Bad Request
- `UnauthorizedAccessException` → 403 Forbidden
- `KeyNotFoundException` → 404 Not Found
- Others → 500 Internal Server Error

**Stack Traces**: Only in Development environment

---

## Real-Time Communication

### SignalR Integration

**Hub**: `PaymentHub` at `/paymentHub`

**Purpose**: Real-time payment notifications

**Usage**:
- Clients connect on app load
- Server pushes updates when payments created/updated/deleted
- UI refreshes automatically

**Methods**:
- `PaymentCreated(Payment payment)`
- `PaymentUpdated(Payment payment)`
- `PaymentDeleted(Guid paymentId)`

---

## Payment Processing

### Payment Entry Types

**Enum `Payment_EntryType`**:
- `Income` - Money received
- `Expense` - Money paid out

### Payment Workflow

1. Client submits payment via API endpoint
2. `PaymentService` validates, creates `Payment` record
3. If cash payment: `CashService` updates `CashBalance` + creates `CashLedger` atomically
4. SignalR notification sent to connected clients
5. Audit trail: `CreatedBy`, `DateIns`, `EditedBy`, `DateEdit`

### Soft Delete Pattern

**All entities use soft delete** (never physically deleted):
- `IsDeleted` - Boolean flag
- `IsDeletedBy` - User who deleted
- `IsDeletedWhy` - Reason
- `DateDeleted` - Timestamp

**Query Filters**: EF global filter `IsDeleted == false`

---

## Billing & Subscriptions (Recent Addition)

### Stripe Integration (Migration: `20251012081537_AddStripeIntegrationFields`)

**New Fields**:
- `Company`: `StripeCustomerId`, `StripeDefaultPaymentMethodId`
- `Company_Subscription`: `StripeSubscriptionId`, `StripePriceId`
- `Plan`: `StripeProductId`, `StripePriceId`

**Admin Features**:
- Billing dashboard (`/Admin/Billing`)
- Subscription management (`Company_Subscription`, `Company_Renewal`)
- Plan management (`Plan`, `Plan_Price`, `Plan_Feature`)

**Status**: Integration schema ready, implementation in progress

---

## Rate Limiting

**API Rate Limiting** (ASP.NET Core):
- Global: 5 requests/second, queue limit 2
- Specific limiters: 20 requests/minute
- Configuration: API `Program.cs` `#region RateLimiting`

---

## Testing Strategy

**API Accessibility**: `public partial class Program {}` allows in-memory testing

**Test Project**:
- Unit tests for services
- Integration tests with in-memory database
- Can reference API project for test host creation

---

## AutoMapper Configuration

**Profiles**: **GestioneIncassi-Shared/Mappings/**

**Registration**: `Program.cs` with assembly scanning

**DTO Naming Convention**: `{Entity}_{Action}_DTO`
- Example: `Payment_Create_DTO`, `Payment_List_DTO`

---

## Localization (App Project)

**Supported Cultures**: `["it", "en", "ro", "es", "de", "fr", "pt"]`

**Default**: Italian (`it`)

**Resources**: `/Resources` folder

**Detection**: Query string → Cookie → Accept-Language header

---

## Important Principles

### Security
- Never commit secrets (use env vars or Azure Key Vault)
- Company context required for most operations (`GICompanyID` from JWT)
- CORS: Ensure `AllowedOrigins` includes all frontend URLs

### Data Integrity
- Payment + CashBalance updates MUST be in same transaction
- Idempotency: Payment API accepts `RequestId` header
- Optimistic concurrency for cash balance

### Code Organization
- ALWAYS use partial classes for large services
- DTOs for all API communication
- Shared models in GestioneIncassi-Shared
- No database access in App/Admin projects

---

## Current Development Status

See [CLAUDE.md](../../CLAUDE.md) for detailed status and [TODO.md](../../TODO.md) for roadmap.

**Key Priorities**:
1. Complete Stripe payment integration
2. Implement security features (MFA, audit logging)
3. Italian electronic invoicing (SDI integration)
4. Automated notifications
5. Analytics dashboard

---

**Last Updated**: 2025-10-13
