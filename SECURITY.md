# 🔐 Security & Access Control – OpenCashFlow

This document describes the **security measures, architecture, and best practices** implemented in OpenCashFlow.  
It is intended as a **reference for contributors, auditors, and operators**.

---

## 🛡️ Security Overview

### Security Architecture

```
┌─────────────────────────────────────────────────────┐
│                  Internet / Users                   │
└──────────────────┬──────────────────────────────────┘
                   │
          ┌────────▼────────┐
          │   HTTPS / TLS   │ ← Encryption in transit
          └────────┬────────┘
                   │
     ┌─────────────▼───────────────┐
     │   Rate Limiting             │ ← DDoS protection
     └─────────────┬───────────────┘
                   │
     ┌─────────────▼───────────────┐
     │   CORS Policy               │ ← Cross‑origin control
     └─────────────┬───────────────┘
                   │
     ┌─────────────▼───────────────┐
     │   JWT Authentication        │ ← User authentication
     └─────────────┬───────────────┘
                   │
     ┌─────────────▼───────────────┐
     │   Subscription Middleware   │ ← Access & billing checks
     └─────────────┬───────────────┘
                   │
     ┌─────────────▼───────────────┐
     │   Role‑Based Authorization  │ ← Permission enforcement
     └─────────────┬───────────────┘
                   │
     ┌─────────────▼───────────────┐
     │   Business Logic            │
     └─────────────┬───────────────┘
                   │
     ┌─────────────▼───────────────┐
     │   PostgreSQL Database       │ ← Encryption at rest
     └─────────────────────────────┘
```

---

## 1. Authentication & Authorization

### 1.1 JWT Token‑Based Authentication

**Implementation**: ASP.NET Core Identity + JWT

**Example configuration** (`OpenCashFlow.API/Program.cs`):

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(...)
        };
    });
```

**Token Storage**
- Stored in **HTTP‑only cookies**
- Not accessible from JavaScript (XSS mitigation)
- `Secure` flag enabled in production
- `SameSite` policy configured

**Token Lifecycle**
- Lifetime: configurable (default 24h)
- Refresh: manual (re‑login required)
- Revocation: client‑side logout (server‑side blacklist not implemented)

---

### 1.2 Roles & Permissions

**Model**
- `AspNetUser` – User
- `AspNetRole` – Role (Administrator, Manager, User)
- `AspNetUserRole` – User ↔ Role mapping
- `AspNetUserPermission` – Explicit permissions
- `AspNetUserDeniedPermission` – Explicitly denied permissions

**Default Roles**
- **Administrator** – Full access, user management, billing, configuration
- **Manager** – Payments, reports, staff management
- **User** – Basic read/write operations

**Usage**
```csharp
[Authorize(Roles = "Administrator")]
public IActionResult AdminOnly() { }

[Authorize(Roles = "Administrator,Manager")]
public IActionResult ManagerOrAdmin() { }
```

---

### 1.3 Multi‑Tenancy (Company Isolation)

**Implementation**
- Each user belongs to a `Company` via `Company_Staff`
- `TenantID` stored in JWT claims
- All queries are automatically filtered by tenant

**Service Pattern**
```csharp
var tenantId = _authenticationService.GetTenantID();
var payments = await _repository.GetPaymentsAsync(tenantId, filters);
```

**Isolation Rules**
- No cross‑company queries
- Admin users may bypass isolation via explicit flags

---

## 2. Subscription‑Based Access Control

### Subscription Authorization Middleware

**Purpose**
- Enforces access based on subscription status

**Allowed States**
- ✅ `ACTIVE`
- ✅ `TRIAL`
- ⚠️ `PAST_DUE` (grace period)
- ❌ `CANCELLED`
- ❌ `EXPIRED`
- ❌ `SUSPENDED`

**Behavior**
- Blocks requests when subscription is invalid
- Optional admin bypass
- Redirects to `/subscription-expired` when blocked

---

## 3. Data Protection

### 3.1 Encryption

**In Transit**
- HTTPS/TLS 1.2+
- HSTS enabled in production

**At Rest**
- Passwords: hashed via ASP.NET Identity (PBKDF2)
- Database: encryption at rest (infrastructure‑level)
- Secrets: stored in secure vaults or environment variables

**Sensitive Data Rules**
- ❌ Never log passwords, tokens, or API keys
- ❌ Never commit secrets to Git
- ✅ Mask sensitive values in logs
- ✅ Exclude sensitive fields from DTO serialization

---

### 3.2 Input Validation & Injection Prevention

**Model Validation**
```csharp
[Required, EmailAddress]
public string Email { get; set; }

[Range(0, 999999)]
public decimal Amount { get; set; }
```

**SQL Injection**
- Entity Framework parameterized queries
- Raw SQL only with parameters

**XSS Protection**
- Automatic Razor encoding
- CSP headers
- Avoid `Html.Raw` unless sanitized

---

## 4. Audit Logging

**Logged Events**
- User login / logout
- Administrative actions
- Subscription changes
- Security‑related events
- External webhook processing

**Retention**
- Configurable (default example: 90 days)

---

## 5. Security Headers

**Examples**
```csharp
X-Content-Type-Options: nosniff
X-Frame-Options: DENY
Referrer-Policy: no-referrer
Permissions-Policy: geolocation=(), camera=(), microphone=()
Strict-Transport-Security: max-age=31536000; includeSubDomains
```

---

## 6. Rate Limiting

**Example Limits**
- Global: 100 requests/minute per user
- Authentication endpoints: stricter limits
- Company‑scoped API usage

**Response**
- HTTP 429 – Too Many Requests

---

## 7. CORS Policy

**Rules**
- Explicit allow‑list of origins
- Credentials allowed
- No wildcard origins in production

---

## 8. Payment & Webhook Security

**Webhook Verification**
- Signature validation required
- Idempotency enforced
- Duplicate event detection

**Secrets Management**
- Environment variables or secret vaults
- No secrets in source code

---

## 9. Future Security Enhancements (Planned)

- Multi‑Factor Authentication (TOTP)
- Advanced audit log storage
- Automated security alerts
- Anomaly detection on login patterns

---

## 10. Incident Response (High Level)

1. Isolate affected systems  
2. Revoke compromised credentials  
3. Analyze logs and scope  
4. Patch vulnerabilities  
5. Notify affected users if required  
6. Document and review the incident  

---

## 📚 References

- OWASP Top 10  
- ASP.NET Core Security Documentation  
- Stripe Security Guidelines  
- GDPR / Data Protection Regulations  

---

**Last updated**: 2026‑01‑04
**Maintained by**: OpenCashFlow contributors  
**Review cycle**: Quarterly
