using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Shared.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Admin_AuditLog",
                columns: table => new
                {
                    AuditLogID = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "varchar(100)", nullable: false),
                    Resource = table.Column<string>(type: "varchar(100)", nullable: false),
                    ResourceID = table.Column<string>(type: "varchar(100)", nullable: true),
                    Action = table.Column<string>(type: "varchar(100)", nullable: false),
                    UserID = table.Column<Guid>(type: "uuid", nullable: true),
                    Username = table.Column<string>(type: "varchar(256)", nullable: true),
                    Changes = table.Column<string>(type: "text", nullable: true),
                    IPAddress = table.Column<string>(type: "varchar(50)", nullable: true),
                    UserAgent = table.Column<string>(type: "text", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Severity = table.Column<string>(type: "varchar(50)", nullable: true),
                    AdditionalInfo = table.Column<string>(type: "text", nullable: true),
                    TenantID = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admin_AuditLog", x => x.AuditLogID);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    ClaimID = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleID = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: false),
                    ClaimValue = table.Column<string>(type: "text", nullable: true),
                    IsVisible = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeletedWhy = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DateIns = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EditedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DateEdit = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.ClaimID);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    RoleID = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    RoleImage = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    IsVisible = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeletedWhy = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DateIns = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EditedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DateEdit = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.RoleID);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    UserID = table.Column<Guid>(type: "uuid", nullable: false),
                    UserName = table.Column<string>(type: "varchar(256)", nullable: false),
                    UserAvatar = table.Column<string>(type: "text", nullable: true),
                    Language = table.Column<string>(type: "text", nullable: false),
                    Country = table.Column<string>(type: "text", nullable: false),
                    Timezone = table.Column<string>(type: "text", nullable: true),
                    UserTitle = table.Column<string>(type: "text", nullable: true),
                    UserFirstName = table.Column<string>(type: "varchar(256)", nullable: false),
                    UserMiddleName = table.Column<string>(type: "varchar(256)", nullable: true),
                    UserLastName = table.Column<string>(type: "varchar(256)", nullable: true),
                    Email = table.Column<string>(type: "varchar(256)", nullable: false),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PhoneNumberPrefix = table.Column<string>(type: "varchar(128)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    Gender = table.Column<string>(type: "text", nullable: true),
                    Pronouns = table.Column<string>(type: "text", nullable: true),
                    DoB = table.Column<DateOnly>(type: "date", nullable: true),
                    PoB = table.Column<string>(type: "text", nullable: true),
                    SoB = table.Column<string>(type: "text", nullable: true),
                    CoB = table.Column<string>(type: "text", nullable: true),
                    Nationality = table.Column<string>(type: "text", nullable: true),
                    PrivacyPolicyAcepted = table.Column<bool>(type: "boolean", nullable: false),
                    PrivacyPolicyVersion = table.Column<string>(type: "text", nullable: true),
                    PrivacyPolicyAcceptedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    PasswordSalt = table.Column<string>(type: "text", nullable: false),
                    QuickLoginPinHash = table.Column<string>(type: "text", nullable: true),
                    QuickLoginPinValidUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MobilePin = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    UserMustChangePassword = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordQuestion = table.Column<string>(type: "text", nullable: true),
                    PasswordAnswer = table.Column<string>(type: "text", nullable: true),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccountValidUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PasswordValidUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PasswordResetToken = table.Column<string>(type: "text", nullable: true),
                    PasswordResetTokenValidUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LockoutEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false),
                    FailedPasswordAnswerAttemptCount = table.Column<int>(type: "integer", nullable: false),
                    LastLoginDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastAppLoginDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    LastKnownLocation = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeletedWhy = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DateIns = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EditedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DateEdit = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.UserID);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsersLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "varchar(128)", nullable: false),
                    ProviderKey = table.Column<string>(type: "varchar(128)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserID = table.Column<string>(type: "text", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeletedWhy = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DateIns = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EditedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DateEdit = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsersLogins", x => new { x.LoginProvider, x.ProviderKey });
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsersTokens",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    UserID = table.Column<Guid>(type: "uuid", nullable: false),
                    LoginProvider = table.Column<string>(type: "varchar(128)", nullable: false),
                    Name = table.Column<string>(type: "varchar(128)", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamp", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "timestamp", nullable: true),
                    CreatedBy = table.Column<string>(type: "varchar(128)", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    RevokedOn = table.Column<DateTime>(type: "timestamp", nullable: true),
                    RevocationReason = table.Column<string>(type: "varchar(256)", nullable: true),
                    IpAddress = table.Column<string>(type: "varchar(45)", nullable: true),
                    UserAgent = table.Column<string>(type: "varchar(512)", nullable: true),
                    AttemptCount = table.Column<int>(type: "int", nullable: false),
                    TokenType = table.Column<string>(type: "varchar(128)", nullable: true),
                    Metadata = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsersTokens", x => new { x.UserID, x.LoginProvider });
                });

            migrationBuilder.CreateTable(
                name: "CashBalances",
                columns: table => new
                {
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Balance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    LastUpdatedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashBalances", x => x.CompanyId);
                });

            migrationBuilder.CreateTable(
                name: "CashLedgers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    RefType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RefId = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalPaymentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Delta = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashLedgers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    TenantID = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyName = table.Column<string>(type: "varchar(256)", nullable: false),
                    MaxUsers = table.Column<long>(type: "bigint", nullable: false),
                    Avatar = table.Column<string>(type: "varchar(256)", nullable: true),
                    BusinessCategory = table.Column<string>(type: "varchar(256)", nullable: true),
                    EstimatedAnnualRevenue = table.Column<decimal>(type: "numeric(18,3)", nullable: true),
                    BusinessHours = table.Column<string>(type: "varchar(256)", nullable: true),
                    Website = table.Column<string>(type: "varchar(256)", nullable: true),
                    SocialLinks = table.Column<string>(type: "varchar(512)", nullable: true),
                    InternalRating = table.Column<decimal>(type: "numeric(3,2)", nullable: true),
                    PriorityLevel = table.Column<int>(type: "integer", nullable: false),
                    VATRates = table.Column<double>(type: "numeric(18,3)", nullable: true),
                    VAT = table.Column<string>(type: "varchar(256)", nullable: true),
                    SDI = table.Column<string>(type: "varchar(256)", nullable: true),
                    TIN = table.Column<string>(type: "varchar(256)", nullable: true),
                    AttorneyName = table.Column<string>(type: "varchar(256)", nullable: true),
                    AttorneyMiddleName = table.Column<string>(type: "varchar(256)", nullable: true),
                    AttorneySurname = table.Column<string>(type: "varchar(256)", nullable: true),
                    IBAN = table.Column<string>(type: "varchar(256)", nullable: true),
                    BIC = table.Column<string>(type: "varchar(256)", nullable: true),
                    SWIFT = table.Column<string>(type: "varchar(256)", nullable: true),
                    PreferredPaymentMethod = table.Column<string>(type: "varchar(256)", nullable: true),
                    MonthlyExpenseLimit = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    BaseDiscountPercentage = table.Column<double>(type: "numeric(18,3)", nullable: true),
                    StripeCustomerID = table.Column<string>(type: "varchar(100)", nullable: true),
                    StripeDefaultPaymentMethodID = table.Column<string>(type: "varchar(100)", nullable: true),
                    BillingEmail = table.Column<string>(type: "varchar(256)", nullable: true),
                    StartingContract = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndingContract = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LicenseType = table.Column<string>(type: "varchar(256)", nullable: true),
                    GdprConsent = table.Column<bool>(type: "boolean", nullable: false),
                    GdprConsentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ContractAcepted = table.Column<bool>(type: "boolean", nullable: false),
                    ContractVersion = table.Column<string>(type: "text", nullable: true),
                    ContractAcceptedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DefaultCurrency = table.Column<string>(type: "varchar(256)", nullable: true),
                    DefaultTimezone = table.Column<string>(type: "text", nullable: true),
                    DefaultLanguage = table.Column<string>(type: "varchar(5)", nullable: true),
                    DefaultCountry = table.Column<string>(type: "varchar(2)", nullable: true),
                    CompanySecret = table.Column<string>(type: "text", nullable: false),
                    MobilePin = table.Column<string>(type: "varchar(256)", nullable: true),
                    MasterPassword = table.Column<string>(type: "varchar(256)", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    StatusID = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeletedWhy = table.Column<string>(type: "text", nullable: true),
                    DateDeleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DateIns = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())"),
                    EditedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DateEdit = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.TenantID);
                });

            migrationBuilder.CreateTable(
                name: "Companies_Invoices",
                columns: table => new
                {
                    TenantID = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceID = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyName = table.Column<string>(type: "varchar(256)", nullable: false),
                    BillingAddressLine1 = table.Column<string>(type: "varchar(256)", nullable: false),
                    BillingAddressLine2 = table.Column<string>(type: "varchar(256)", nullable: true),
                    BillingCity = table.Column<string>(type: "varchar(100)", nullable: false),
                    BillingState = table.Column<string>(type: "varchar(100)", nullable: false),
                    BillingPostalCode = table.Column<string>(type: "varchar(20)", nullable: false),
                    BillingCountry = table.Column<string>(type: "varchar(100)", nullable: false),
                    TaxIdentificationNumber = table.Column<string>(type: "varchar(50)", nullable: true),
                    ContactName = table.Column<string>(type: "varchar(256)", nullable: true),
                    ContactEmail = table.Column<string>(type: "varchar(100)", nullable: true),
                    ContactPhone = table.Column<string>(type: "varchar(20)", nullable: true),
                    InvoiceNumber = table.Column<string>(type: "varchar(256)", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "varchar(256)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalTaxes = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    IvoicePath = table.Column<string>(type: "text", nullable: false),
                    PaymentMethod = table.Column<string>(type: "varchar(150)", nullable: false),
                    TransactionID = table.Column<string>(type: "varchar(150)", nullable: true),
                    PaymentProcessor = table.Column<string>(type: "varchar(256)", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DateIns = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EditedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DateEdit = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies_Invoices", x => x.InvoiceID);
                });

            migrationBuilder.CreateTable(
                name: "Features",
                columns: table => new
                {
                    FeatureID = table.Column<Guid>(type: "uuid", nullable: false),
                    FeatureName = table.Column<string>(type: "text", nullable: false),
                    FeatureCode = table.Column<string>(type: "varchar(150)", nullable: false),
                    FeatureCategory = table.Column<string>(type: "varchar(128)", nullable: true),
                    FeatureIcon = table.Column<string>(type: "varchar(128)", nullable: false),
                    FeatureIconColor = table.Column<string>(type: "varchar(128)", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    DisplayOrder = table.Column<long>(type: "bigint", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    IsCustomizable = table.Column<bool>(type: "boolean", nullable: false),
                    IsVisible = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Features", x => x.FeatureID);
                });

            migrationBuilder.CreateTable(
                name: "Payments_DailyPayments",
                columns: table => new
                {
                    DailyPaymentsID = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantID = table.Column<Guid>(type: "uuid", nullable: false),
                    CashDate = table.Column<DateTime>(type: "date", nullable: false),
                    Total = table.Column<double>(type: "numeric(18,3)", nullable: false),
                    DateIns = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments_DailyPayments", x => x.DailyPaymentsID);
                });

            migrationBuilder.CreateTable(
                name: "Payments_DocumentTypes_LookUps",
                columns: table => new
                {
                    DocumentTypeID = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentTypeName = table.Column<string>(type: "varchar(256)", nullable: false),
                    DocumentTypeDescription = table.Column<string>(type: "varchar(50)", nullable: false),
                    DocumentTypeIcon = table.Column<string>(type: "text", nullable: true),
                    Visible = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    TenantID = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeletedWhy = table.Column<string>(type: "text", nullable: true),
                    DateDeleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DateIns = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EditedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DateEdit = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments_DocumentTypes_LookUps", x => x.DocumentTypeID);
                });

            migrationBuilder.CreateTable(
                name: "Payments_Methods_Lookups",
                columns: table => new
                {
                    PaymentMethodID = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentMethodName = table.Column<string>(type: "varchar(256)", nullable: false),
                    PaymentMethodDescription = table.Column<string>(type: "varchar(50)", nullable: false),
                    PaymentMethodIcon = table.Column<string>(type: "text", nullable: true),
                    Visible = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    TenantID = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeletedWhy = table.Column<string>(type: "text", nullable: true),
                    DateDeleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DateIns = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EditedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DateEdit = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments_Methods_Lookups", x => x.PaymentMethodID);
                });

            migrationBuilder.CreateTable(
                name: "Plans",
                columns: table => new
                {
                    PlanID = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanCode = table.Column<string>(type: "varchar(100)", nullable: false),
                    Name = table.Column<string>(type: "varchar(256)", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Image = table.Column<string>(type: "text", nullable: true),
                    MaxUsers = table.Column<long>(type: "bigint", nullable: true),
                    HasTrial = table.Column<bool>(type: "boolean", nullable: false),
                    TrialDays = table.Column<int>(type: "int", nullable: true),
                    Price = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    DurationMonths = table.Column<int>(type: "int", nullable: false),
                    BillingCycle = table.Column<string>(type: "varchar(50)", nullable: false),
                    StripeProductID = table.Column<string>(type: "varchar(100)", nullable: true),
                    StripePriceID = table.Column<string>(type: "varchar(100)", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Visible = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<long>(type: "bigint", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeletedWhy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plans", x => x.PlanID);
                });

            migrationBuilder.CreateTable(
                name: "Stripe_Webhook_Events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    EventType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReceivedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Success = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    Payload = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stripe_Webhook_Events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRolePermission",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    Permission = table.Column<string>(type: "text", nullable: true),
                    RoleID = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRolePermission", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRolePermission_AspNetRoles_RoleID",
                        column: x => x.RoleID,
                        principalTable: "AspNetRoles",
                        principalColumn: "RoleID");
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    ClaimID = table.Column<Guid>(type: "uuid", nullable: false),
                    UserID = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: false),
                    ClaimValue = table.Column<string>(type: "text", nullable: true),
                    IsVisible = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeletedWhy = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DateIns = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EditedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DateEdit = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AspNetUserUserID = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.ClaimID);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_AspNetUserUserID",
                        column: x => x.AspNetUserUserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserDeniedPermission",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Permission = table.Column<string>(type: "text", nullable: true),
                    UserID = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserDeniedPermission", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserDeniedPermission_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserPermission",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Permission = table.Column<string>(type: "text", nullable: true),
                    UserID = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserPermission", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserPermission_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsersRoles",
                columns: table => new
                {
                    UserID = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleID = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsersRoles", x => new { x.UserID, x.RoleID });
                    table.ForeignKey(
                        name: "FK_AspNetUsersRoles_AspNetRoles_RoleID",
                        column: x => x.RoleID,
                        principalTable: "AspNetRoles",
                        principalColumn: "RoleID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUsersRoles_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Companies_Staff",
                columns: table => new
                {
                    TenantID = table.Column<Guid>(type: "uuid", nullable: false),
                    UserID = table.Column<Guid>(type: "uuid", nullable: false),
                    TimeCost = table.Column<double>(type: "numeric(18,3)", nullable: true),
                    BadgeID = table.Column<string>(type: "varchar(256)", nullable: true),
                    OutOfReports = table.Column<bool>(type: "boolean", nullable: false),
                    RequireShiftCheckIn = table.Column<bool>(type: "boolean", nullable: false),
                    LastCheckIn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastCheckOut = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Role = table.Column<string>(type: "varchar(50)", nullable: true),
                    Department = table.Column<string>(type: "varchar(256)", nullable: true),
                    WorkLocation = table.Column<string>(type: "varchar(50)", nullable: true),
                    ContractStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ContractEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MonthlySalary = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Bonuses = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Allowances = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    EmploymentType = table.Column<string>(type: "varchar(50)", nullable: true),
                    OvertimeRate = table.Column<decimal>(type: "numeric(18,3)", nullable: true),
                    Skills = table.Column<string>(type: "text", nullable: true),
                    SupervisorID = table.Column<Guid>(type: "uuid", nullable: true),
                    AccessLevel = table.Column<string>(type: "varchar(50)", nullable: true),
                    AuthorizedAreas = table.Column<string>(type: "text", nullable: true),
                    InternalNotes = table.Column<string>(type: "text", nullable: true),
                    PublicNotes = table.Column<string>(type: "text", nullable: true),
                    ExternalSystemReference = table.Column<string>(type: "text", nullable: true),
                    SyncStatus = table.Column<string>(type: "varchar(50)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeletedWhy = table.Column<string>(type: "text", nullable: true),
                    DateDeleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DateIns = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())"),
                    EditedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DateEdit = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies_Staff", x => x.UserID);
                    table.ForeignKey(
                        name: "FK_Companies_Staff_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Companies_Addresses",
                columns: table => new
                {
                    TenantID = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyAddressID = table.Column<Guid>(type: "uuid", nullable: false),
                    Address = table.Column<string>(type: "varchar(256)", nullable: false),
                    AddressNum = table.Column<string>(type: "varchar(256)", nullable: true),
                    AddressInt = table.Column<string>(type: "varchar(256)", nullable: true),
                    AddressStair = table.Column<string>(type: "varchar(256)", nullable: true),
                    City = table.Column<string>(type: "varchar(256)", nullable: true),
                    ZIP = table.Column<string>(type: "varchar(256)", nullable: true),
                    State = table.Column<string>(type: "varchar(256)", nullable: true),
                    Country = table.Column<string>(type: "varchar(256)", nullable: true),
                    AddressNote = table.Column<string>(type: "text", nullable: true),
                    GeoHash = table.Column<string>(type: "varchar(256)", nullable: true),
                    AddressTypeID = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeletedWhy = table.Column<string>(type: "text", nullable: true),
                    DateDeleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DateIns = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EditedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DateEdit = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyTenantID = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies_Addresses", x => x.CompanyAddressID);
                    table.ForeignKey(
                        name: "FK_Companies_Addresses_Companies_CompanyTenantID",
                        column: x => x.CompanyTenantID,
                        principalTable: "Companies",
                        principalColumn: "TenantID");
                });

            migrationBuilder.CreateTable(
                name: "Companies_BillingAddresses",
                columns: table => new
                {
                    TenantID = table.Column<Guid>(type: "uuid", nullable: true),
                    BillingAddressID = table.Column<Guid>(type: "uuid", nullable: false),
                    BillingAddress = table.Column<string>(type: "varchar(256)", nullable: true),
                    BillingAddressNum = table.Column<string>(type: "varchar(256)", nullable: true),
                    BillingAddressInt = table.Column<string>(type: "varchar(256)", nullable: true),
                    BillingAddressStair = table.Column<string>(type: "varchar(256)", nullable: true),
                    BillingZIP = table.Column<string>(type: "varchar(256)", nullable: true),
                    BillingCity = table.Column<string>(type: "varchar(256)", nullable: true),
                    BillingState = table.Column<string>(type: "varchar(256)", nullable: true),
                    BillingCountry = table.Column<string>(type: "varchar(256)", nullable: true),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeletedWhy = table.Column<string>(type: "text", nullable: true),
                    DateDeleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DateIns = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EditedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DateEdit = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyTenantID = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies_BillingAddresses", x => x.BillingAddressID);
                    table.ForeignKey(
                        name: "FK_Companies_BillingAddresses_Companies_CompanyTenantID",
                        column: x => x.CompanyTenantID,
                        principalTable: "Companies",
                        principalColumn: "TenantID");
                });

            migrationBuilder.CreateTable(
                name: "Companies_Contacts_Emails",
                columns: table => new
                {
                    TenantID = table.Column<Guid>(type: "uuid", nullable: true),
                    Email = table.Column<string>(type: "varchar(256)", nullable: true),
                    ContactEmailID = table.Column<Guid>(type: "uuid", nullable: false),
                    EmailTypeID = table.Column<Guid>(type: "uuid", nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeletedWhy = table.Column<string>(type: "text", nullable: true),
                    DateDeleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DateIns = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EditedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DateEdit = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyTenantID = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies_Contacts_Emails", x => x.ContactEmailID);
                    table.ForeignKey(
                        name: "FK_Companies_Contacts_Emails_Companies_CompanyTenantID",
                        column: x => x.CompanyTenantID,
                        principalTable: "Companies",
                        principalColumn: "TenantID");
                });

            migrationBuilder.CreateTable(
                name: "Companies_Contacts_Phones",
                columns: table => new
                {
                    TenantID = table.Column<Guid>(type: "uuid", nullable: true),
                    CountryPrefixCode = table.Column<string>(type: "varchar(100)", nullable: true),
                    Phone = table.Column<string>(type: "varchar(256)", nullable: true),
                    ContactPhoneID = table.Column<Guid>(type: "uuid", nullable: false),
                    Contact_Phone_TypeID = table.Column<Guid>(type: "uuid", nullable: true),
                    PhoneConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeletedWhy = table.Column<string>(type: "text", nullable: true),
                    DateDeleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DateIns = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EditedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DateEdit = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompanyTenantID = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies_Contacts_Phones", x => x.ContactPhoneID);
                    table.ForeignKey(
                        name: "FK_Companies_Contacts_Phones_Companies_CompanyTenantID",
                        column: x => x.CompanyTenantID,
                        principalTable: "Companies",
                        principalColumn: "TenantID");
                });

            migrationBuilder.CreateTable(
                name: "Companies_Invoices_Items",
                columns: table => new
                {
                    TenantID = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceItemID = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceID = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TaxPercent = table.Column<decimal>(type: "numeric(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies_Invoices_Items", x => x.InvoiceItemID);
                    table.ForeignKey(
                        name: "FK_Companies_Invoices_Items_Companies_Invoices_InvoiceID",
                        column: x => x.InvoiceID,
                        principalTable: "Companies_Invoices",
                        principalColumn: "InvoiceID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    PaymentID = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantID = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<double>(type: "numeric(18,3)", nullable: false),
                    EntryType = table.Column<string>(type: "varchar(256)", nullable: false),
                    PaymentMethodID = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentTypeID = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    UserID = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeletedWhy = table.Column<string>(type: "text", nullable: true),
                    DateDeleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DateIns = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EditedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DateEdit = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.PaymentID);
                    table.ForeignKey(
                        name: "FK_Payments_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Payments_Payments_DocumentTypes_LookUps_DocumentTypeID",
                        column: x => x.DocumentTypeID,
                        principalTable: "Payments_DocumentTypes_LookUps",
                        principalColumn: "DocumentTypeID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Payments_Payments_Methods_Lookups_PaymentMethodID",
                        column: x => x.PaymentMethodID,
                        principalTable: "Payments_Methods_Lookups",
                        principalColumn: "PaymentMethodID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Companies_Subscriptions",
                columns: table => new
                {
                    TenantID = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscriptionID = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanID = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp", nullable: false),
                    NextBillingDate = table.Column<DateTime>(type: "timestamp", nullable: false),
                    BillingCycle = table.Column<string>(type: "varchar(50)", nullable: false),
                    BillingDay = table.Column<int>(type: "integer", nullable: false),
                    RenewalStatus = table.Column<string>(type: "varchar(50)", nullable: false),
                    LastReminderDate = table.Column<DateTime>(type: "timestamp", nullable: false),
                    NextReminderDate = table.Column<DateTime>(type: "timestamp", nullable: false),
                    Cost = table.Column<double>(type: "numeric(18,3)", nullable: false),
                    Discount = table.Column<double>(type: "numeric(18,3)", nullable: true),
                    PromoCode = table.Column<string>(type: "text", nullable: true),
                    DiscountExpiration = table.Column<DateTime>(type: "timestamp", nullable: true),
                    CancellationDate = table.Column<DateTime>(type: "timestamp", nullable: true),
                    CancellationReason = table.Column<string>(type: "text", nullable: true),
                    StripeSubscriptionID = table.Column<string>(type: "varchar(100)", nullable: true),
                    StripePriceID = table.Column<string>(type: "varchar(100)", nullable: true),
                    StripeInvoiceID = table.Column<string>(type: "varchar(100)", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DateIns = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EditedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DateEdit = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies_Subscriptions", x => x.SubscriptionID);
                    table.ForeignKey(
                        name: "FK_Companies_Subscriptions_Companies_TenantID",
                        column: x => x.TenantID,
                        principalTable: "Companies",
                        principalColumn: "TenantID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Companies_Subscriptions_Plans_PlanID",
                        column: x => x.PlanID,
                        principalTable: "Plans",
                        principalColumn: "PlanID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Plans_Features",
                columns: table => new
                {
                    PlanID = table.Column<Guid>(type: "uuid", nullable: false),
                    FeatureID = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plans_Features", x => new { x.PlanID, x.FeatureID });
                    table.ForeignKey(
                        name: "FK_Plans_Features_Features_FeatureID",
                        column: x => x.FeatureID,
                        principalTable: "Features",
                        principalColumn: "FeatureID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Plans_Features_Plans_PlanID",
                        column: x => x.PlanID,
                        principalTable: "Plans",
                        principalColumn: "PlanID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Companies_Renewals",
                columns: table => new
                {
                    TenantID = table.Column<Guid>(type: "uuid", nullable: false),
                    RenewalID = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanID = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscriptionID = table.Column<Guid>(type: "uuid", nullable: false),
                    BillingDate = table.Column<DateTime>(type: "timestamp", nullable: true),
                    AmountPaid = table.Column<double>(type: "numeric(18,3)", nullable: false),
                    TaxDetails = table.Column<double>(type: "numeric(18,3)", nullable: false),
                    Currency = table.Column<string>(type: "varchar(50)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "varchar(150)", nullable: false),
                    RenewalStatus = table.Column<string>(type: "varchar(150)", nullable: false),
                    LateFee = table.Column<string>(type: "varchar(150)", nullable: false),
                    InvoiceID = table.Column<string>(type: "varchar(150)", nullable: false),
                    PaymentProcessor = table.Column<string>(type: "varchar(150)", nullable: false),
                    TransactionID = table.Column<string>(type: "varchar(150)", nullable: true),
                    TransactionStatus = table.Column<string>(type: "varchar(150)", nullable: true),
                    RenewalType = table.Column<string>(type: "varchar(150)", nullable: true),
                    Comments = table.Column<string>(type: "Text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies_Renewals", x => x.RenewalID);
                    table.ForeignKey(
                        name: "FK_Companies_Renewals_Companies_Subscriptions_SubscriptionID",
                        column: x => x.SubscriptionID,
                        principalTable: "Companies_Subscriptions",
                        principalColumn: "SubscriptionID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Companies_Renewals_Plans_PlanID",
                        column: x => x.PlanID,
                        principalTable: "Plans",
                        principalColumn: "PlanID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "RoleID", "ConcurrencyStamp", "CreatedBy", "DateEdit", "DateIns", "EditedBy", "IsDeleted", "IsDeletedBy", "IsDeletedWhy", "IsVisible", "RoleImage", "RoleName" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), null, null, null, new DateTime(2025, 6, 13, 22, 24, 27, 530, DateTimeKind.Utc), null, false, null, null, true, null, "CompanyAdmin" },
                    { new Guid("00000000-0000-0000-0000-000000000002"), null, null, null, new DateTime(2025, 6, 13, 22, 24, 27, 530, DateTimeKind.Utc), null, false, null, null, true, null, "Employee" },
                    { new Guid("00000000-0000-0000-0000-000000000003"), null, null, null, new DateTime(2025, 6, 13, 22, 24, 27, 530, DateTimeKind.Utc), null, false, null, null, false, null, "InstanceAdmin" }
                });

            migrationBuilder.InsertData(
                table: "Payments_DocumentTypes_LookUps",
                columns: new[] { "DocumentTypeID", "CreatedBy", "DateDeleted", "DateEdit", "DateIns", "DisplayOrder", "DocumentTypeDescription", "DocumentTypeIcon", "DocumentTypeName", "EditedBy", "IsDeleted", "IsDeletedBy", "IsDeletedWhy", "TenantID", "Visible" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), null, null, null, new DateTime(2025, 5, 27, 22, 24, 27, 530, DateTimeKind.Utc), 0, "Invoice", null, "Invoice", null, false, null, null, null, true },
                    { new Guid("00000000-0000-0000-0000-000000000099"), null, null, null, new DateTime(2025, 5, 27, 22, 24, 27, 530, DateTimeKind.Utc), 0, "Receipt", null, "Receipt", null, false, null, null, null, true }
                });

            migrationBuilder.InsertData(
                table: "Payments_Methods_Lookups",
                columns: new[] { "PaymentMethodID", "CreatedBy", "DateDeleted", "DateEdit", "DateIns", "DisplayOrder", "EditedBy", "IsDeleted", "IsDeletedBy", "IsDeletedWhy", "PaymentMethodDescription", "PaymentMethodIcon", "PaymentMethodName", "TenantID", "Visible" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), null, null, null, new DateTime(2025, 5, 27, 22, 24, 27, 530, DateTimeKind.Utc), 1, null, false, null, null, "Payment By Credit Card", null, "Credit Card", null, true },
                    { new Guid("00000000-0000-0000-0000-000000000002"), null, null, null, new DateTime(2025, 5, 27, 22, 24, 27, 530, DateTimeKind.Utc), 0, null, false, null, null, "Cash payments", null, "Cash", null, true }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Admin_AuditLog_EventType_Timestamp",
                table: "Admin_AuditLog",
                columns: new[] { "EventType", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_Admin_AuditLog_Resource_ResourceID",
                table: "Admin_AuditLog",
                columns: new[] { "Resource", "ResourceID" });

            migrationBuilder.CreateIndex(
                name: "IX_Admin_AuditLog_UserID_Timestamp",
                table: "Admin_AuditLog",
                columns: new[] { "UserID", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRolePermission_RoleID",
                table: "AspNetRolePermission",
                column: "RoleID");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_AspNetUserUserID",
                table: "AspNetUserClaims",
                column: "AspNetUserUserID");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserDeniedPermission_UserID",
                table: "AspNetUserDeniedPermission",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserPermission_UserID",
                table: "AspNetUserPermission",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsersRoles_RoleID",
                table: "AspNetUsersRoles",
                column: "RoleID");

            migrationBuilder.CreateIndex(
                name: "IX_CashLedgers_CompanyId_CreatedAtUtc",
                table: "CashLedgers",
                columns: new[] { "CompanyId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_CashLedgers_CompanyId_RefType_RefId",
                table: "CashLedgers",
                columns: new[] { "CompanyId", "RefType", "RefId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Companies_BillingEmail",
                table: "Companies",
                column: "BillingEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_StripeCustomerID",
                table: "Companies",
                column: "StripeCustomerID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Companies_StripeDefaultPaymentMethodID",
                table: "Companies",
                column: "StripeDefaultPaymentMethodID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Addresses_CompanyTenantID",
                table: "Companies_Addresses",
                column: "CompanyTenantID");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_BillingAddresses_CompanyTenantID",
                table: "Companies_BillingAddresses",
                column: "CompanyTenantID");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Contacts_Emails_CompanyTenantID",
                table: "Companies_Contacts_Emails",
                column: "CompanyTenantID");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Contacts_Phones_CompanyTenantID",
                table: "Companies_Contacts_Phones",
                column: "CompanyTenantID");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Invoices_Items_InvoiceID",
                table: "Companies_Invoices_Items",
                column: "InvoiceID");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Renewals_PlanID",
                table: "Companies_Renewals",
                column: "PlanID");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Renewals_SubscriptionID",
                table: "Companies_Renewals",
                column: "SubscriptionID");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Subscriptions_PlanID",
                table: "Companies_Subscriptions",
                column: "PlanID");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Subscriptions_StripeInvoiceID",
                table: "Companies_Subscriptions",
                column: "StripeInvoiceID");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Subscriptions_StripePriceID",
                table: "Companies_Subscriptions",
                column: "StripePriceID");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Subscriptions_StripeSubscriptionID",
                table: "Companies_Subscriptions",
                column: "StripeSubscriptionID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Subscriptions_TenantID",
                table: "Companies_Subscriptions",
                column: "TenantID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_DocumentTypeID",
                table: "Payments",
                column: "DocumentTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaymentMethodID",
                table: "Payments",
                column: "PaymentMethodID");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_UserID",
                table: "Payments",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_DailyPayments_TenantID_CashDate",
                table: "Payments_DailyPayments",
                columns: new[] { "TenantID", "CashDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Plans_StripePriceID",
                table: "Plans",
                column: "StripePriceID");

            migrationBuilder.CreateIndex(
                name: "IX_Plans_StripeProductID",
                table: "Plans",
                column: "StripeProductID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Plans_Features_FeatureID",
                table: "Plans_Features",
                column: "FeatureID");

            migrationBuilder.CreateIndex(
                name: "IX_Stripe_Webhook_Events_EventId",
                table: "Stripe_Webhook_Events",
                column: "EventId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Admin_AuditLog");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetRolePermission");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserDeniedPermission");

            migrationBuilder.DropTable(
                name: "AspNetUserPermission");

            migrationBuilder.DropTable(
                name: "AspNetUsersLogins");

            migrationBuilder.DropTable(
                name: "AspNetUsersRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsersTokens");

            migrationBuilder.DropTable(
                name: "CashBalances");

            migrationBuilder.DropTable(
                name: "CashLedgers");

            migrationBuilder.DropTable(
                name: "Companies_Addresses");

            migrationBuilder.DropTable(
                name: "Companies_BillingAddresses");

            migrationBuilder.DropTable(
                name: "Companies_Contacts_Emails");

            migrationBuilder.DropTable(
                name: "Companies_Contacts_Phones");

            migrationBuilder.DropTable(
                name: "Companies_Invoices_Items");

            migrationBuilder.DropTable(
                name: "Companies_Renewals");

            migrationBuilder.DropTable(
                name: "Companies_Staff");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "Payments_DailyPayments");

            migrationBuilder.DropTable(
                name: "Plans_Features");

            migrationBuilder.DropTable(
                name: "Stripe_Webhook_Events");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Companies_Invoices");

            migrationBuilder.DropTable(
                name: "Companies_Subscriptions");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Payments_DocumentTypes_LookUps");

            migrationBuilder.DropTable(
                name: "Payments_Methods_Lookups");

            migrationBuilder.DropTable(
                name: "Features");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "Plans");
        }
    }
}
