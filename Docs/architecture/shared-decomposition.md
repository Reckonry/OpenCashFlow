# OpenCashFlow.Shared Decomposition

## Status

`OpenCashFlow.Shared` has been dismantled from the core solution.

The project is no longer part of `OpenCashFlow.sln`, no core project references it, and it no longer contains source files. The previous responsibilities have been moved to the Clean Architecture layer that owns them.

## Final Ownership

| Previous Shared area | New owner | Notes |
| --- | --- | --- |
| `DTOs/*` | `OpenCashFlow.Contracts/DTOs` | Public API/WebApp contract shapes. DTOs that previously exposed EF entities were neutralized with contract-specific DTOs. |
| `Models/ApiResponse.cs` | `OpenCashFlow.Contracts/Models` | Public response envelope. |
| Public neutral enums | `OpenCashFlow.Contracts/Enums`, `OpenCashFlow.Contracts/Security`, `OpenCashFlow.Contracts/Audit` | Kept out of Domain/Application when they are public contracts. |
| `Core/Configuration.cs`, `Core/AvatarGenerator.cs` | `OpenCashFlow.Contracts/Core` | Shared public constants/helpers used by API/WebApp. |
| Password/cookie/secret helpers | `OpenCashFlow.Infrastructure/Auth` | Runtime security implementations. |
| Email and Slack ports | `OpenCashFlow.Application/Abstractions` | Application-facing ports. |
| Email and Slack implementations/options | `OpenCashFlow.Infrastructure/Email`, `OpenCashFlow.Infrastructure/Notifications` | Runtime infrastructure. |
| AutoMapper profile | `OpenCashFlow.API/Mapping` | Boundary mapping between public DTOs and persistence-shaped models. |
| Module manifest/registry prototype | `OpenCashFlow.Application/Modules` | Application-level modularity prototype. |
| EF entities | `OpenCashFlow.Infrastructure/Persistence/Entities` | Persistence models only. |
| `ApplicationDbContext`, factory, migrations | `OpenCashFlow.Infrastructure/Persistence` | EF persistence owner. |
| Logging event constants | `OpenCashFlow.Infrastructure/Logging` | Runtime logging concern. |
| Legacy Billing/Stripe schema | `OpenCashFlow.Infrastructure/Persistence/Entities` | Persistence compatibility only; no Billing/Stripe runtime is registered. |

## WebApp Decoupling

`OpenCashFlow.WebApp` no longer references `OpenCashFlow.Infrastructure`.

The UI now uses `OpenCashFlow.Contracts` for shared API/WebApp data and local `OpenCashFlow.WebApp/Models` types for Razor-only view models.

## Company/Employee/Setup Slice

After the WebApp decoupling, Setup and the first Company/Employee flows were moved behind Application use cases:

- Setup status and completion;
- Company current/all/invoice reads;
- Employee list/detail/delete/update-profile.

`OpenCashFlow.Contracts` remains the public DTO owner for these flows. Since Fase 3G, `OpenCashFlow.Application` uses Application-owned records and no longer references Contracts; API services perform the boundary mapping.

## Employee/Auth Ports Slice

Fase 3G moved employee create/update/PIN and forgot/reset password behind Application use cases:

- `OpenCashFlow.Application/Employees/CreateEmployee`
- `OpenCashFlow.Application/Employees/UpdateEmployee`
- `OpenCashFlow.Application/Employees/ResendPin`
- `OpenCashFlow.Application/Auth/ForgotPassword`
- `OpenCashFlow.Application/Auth/ResetPassword`

Infrastructure now owns EF persistence, password hashing, unique PIN generation, reset-token storage and optional email notifications for those flows.

## Auth Registration/Login/FastLogin Slice

Fase 3H moved the remaining registration/login surface behind Application and Infrastructure:

- `OpenCashFlow.Application/Auth/Login`
- `OpenCashFlow.Application/Auth/FastLogin`
- `OpenCashFlow.Application/Auth/Register`
- `OpenCashFlow.Application/Auth/AccountConfirmation`
- `OpenCashFlow.Application/Auth/Ports`
- `OpenCashFlow.Infrastructure/Auth`

`AuthenticationService` now acts as the API boundary adapter for these flows. `IEmployeeRepository` is removed from API. JWT issuing, fast-login cookie signing, password hashing, PIN verification and registration persistence are Infrastructure-owned.

## Auth Audit Port Slice

Fase 3I moved authentication audit persistence out of the API service:

- `OpenCashFlow.Application/Auth/Audit/AuthAuditEvent`
- `OpenCashFlow.Application/Auth/Ports/IAuthAuditWriter`
- `OpenCashFlow.Infrastructure/Auth/AuthAuditWriter`

`AuthenticationService` collects HTTP-bound audit context and delegates persistence to the Application port. It no longer depends on `ApplicationDbContext`.

## Verification Rules

The expected checks are:

```bash
dotnet sln OpenCashFlow.sln list
dotnet build OpenCashFlow.sln
dotnet test OpenCashFlow.sln --no-build
rg -n "OpenCashFlow.Shared|global::Shared|using Shared|@using Shared" src tests --glob '!**/bin/**' --glob '!**/obj/**'
```

The only known non-project `Shared` text is the vendor JavaScript variable `backstageShared` under `src/OpenCashFlow.WebApp/wwwroot/libs`.
