![OpenCashFlow](.github/OCF_Banner.png)

# 💰 OpenCashFlow

![.NET](https://img.shields.io/badge/.NET-9.0-blueviolet?logo=dotnet&style=for-the-badge)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET-Core-blue?logo=dotnet&style=for-the-badge)
![CSharp](https://img.shields.io/badge/C%23-9.0-239120?logo=c-sharp&logoColor=white&style=for-the-badge)
![EF Core](https://img.shields.io/badge/EF%20Core-9.0-success?style=for-the-badge&logo=ef)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-DB-blue?logo=postgresql&logoColor=white&style=for-the-badge)
![Bootstrap](https://img.shields.io/badge/Bootstrap-5.x-purple?logo=bootstrap&style=for-the-badge)
![jQuery](https://img.shields.io/badge/jQuery-3.x-blue?logo=jquery&style=for-the-badge)
![REST API](https://img.shields.io/badge/API-RESTful-orange?style=for-the-badge)
![OpenAPI](https://img.shields.io/badge/OpenAPI-Swagger-yellowgreen?logo=swagger&style=for-the-badge)
![Architecture](https://img.shields.io/badge/architecture-layered--clean--inspired-blue)
![JWT Auth](https://img.shields.io/badge/Auth-JWT-blue?style=for-the-badge)
![Serilog](https://img.shields.io/badge/Logging-Serilog-informational?style=for-the-badge)
![Slack Integration](https://img.shields.io/badge/Slack-Integration-4A154B?logo=slack&style=for-the-badge)

**OpenCashFlow** is an open-source platform for managing, tracking, and analyzing cash flows and receipts.
It is designed to provide full transparency over incoming and outgoing payments, with strong auditability,
advanced filtering, and access control.

---

## ⚖️ License (Important)

This project is licensed under the **GNU Affero General Public License v3.0 (AGPL-3.0)**.

> **If you run OpenCashFlow as a network service (SaaS), you must provide the complete corresponding source code
> to the users of that service, in compliance with the AGPL.**

See the `LICENSE` file for full details.

---

## 🚀 Key Features

- 📋 **Payment registration** (income / expense)
- 🧾 Classification by:
  - Payment method (Cash, Card, Bank Transfer, etc.)
  - Document type (Invoice, Receipt, etc.)
  - Description and reason
- 🔐 **User management & auditing**
  - Created / edited / deleted tracking
  - Soft delete support
- 🔎 **Advanced filtering system**
  - Date ranges (`FromDate` / `ToDate`)
  - Amount ranges (`MinAmount` / `MaxAmount`)
  - Payment method, transaction type, operator
- 📦 **RESTful API**
  - Pagination
  - Dynamic sorting (`SortBy`)
  - Secure DTO-based queries (`Payment_Filter_DTO`)
- 🖥️ **Responsive Web UI**
  - Bootstrap modals
  - Real-time updates via SignalR
- 📊 Ready for export, dashboards, and forecasting

---

## 🏗️ Architecture

- **Backend:** ASP.NET Core + Entity Framework Core
- **Frontend:** Bootstrap 5 + AJAX + jQuery
- **Database:** PostgreSQL
- **Logging:** Serilog with Slack notifier and retry strategy
- **Security:** JWT authentication + ACL (multi-company ready)

---

## 🧰 Quick Setup

> ⚙️ Requirements: .NET 9 SDK, PostgreSQL, Visual Studio or VS Code

```bash
git clone https://github.com/<your-username>/OpenCashFlow.git
cd OpenCashFlow

# Configure your connection string in appsettings.Development.json
dotnet ef database update
dotnet run
