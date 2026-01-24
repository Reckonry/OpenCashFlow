using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Shared.Core;
using Shared.Models;
using Shared.Models.Identity;
using Shared.Models.Stripe;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Reflection.Emit;
using static Shared.Logging.LogEvents;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace Shared.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            if (options == null)
            {
                Console.WriteLine("DbContextOptions is null.");
            }
        }

        #region AspNetCoreIdentity
        public DbSet<AspNetRole> AspNetRole_DS { get; set; }
        public DbSet<AspNetRoleClaim> AspNetRoleClaim_DS { get; set; }
        public DbSet<AspNetUser> AspNetUser_DS { get; set; }
        public DbSet<AspNetUserClaim> AspNetUserClaim_DS { get; set; }
        public DbSet<AspNetUserLogin> AspNetUserLogin_DS { get; set; }
        public DbSet<AspNetUserRole> AspNetUserRole_DS { get; set; }
        public DbSet<AspNetUserToken> AspNetUserToken_DS { get; set; }
        #endregion

        #region Companies
        public DbSet<Company> Company_DS { get; set; }
        public DbSet<Company_Address> Company_Address_DS { get; set; }
        public DbSet<Company_Invoice> Company_Invoice_DS { get; set; }
        public DbSet<Company_Staff> Company_Staff_DS { get; set; }
        //public DbSet<> { get; set; }
        #endregion

        #region Payments
        public DbSet<Payment> Payment_DS { get; set; }
        public DbSet<Payment_Method_LookUps> PaymentMethod_DS { get; set; }
        public DbSet<Payment_DocumentType_LookUp> Payment_DocumentType_DS { get; set; }
        public DbSet<Payment_DailyPayments> DailyCash_DS { get; set; }
        #endregion

        #region Billing
        public DbSet<Plan> Plan_DS { get; set; }
        public DbSet<Company_Subscription> Company_Subscription_DS { get; set; }
        public DbSet<Company_Renewal> Company_Renewal_DS { get; set; }
        public DbSet<Stripe_Webhook_Event> Stripe_Webhook_Events { get; set; }
        #endregion

        #region Admin
        public DbSet<Shared.Models.Admin.Admin_AuditLog> Admin_AuditLog_DS { get; set; }
        #endregion

        #region Cash
        public DbSet<Shared.Models.Cash.CashBalance> CashBalances { get; set; } = default!;
        public DbSet<Shared.Models.Cash.CashLedger> CashLedgers { get; set; } = default!;
        #endregion


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Shared.Models.Cash.CashBalance>(b =>
            {
                b.ToTable("CashBalances");
                b.HasKey(x => x.CompanyId);
                b.Property(x => x.Balance).HasColumnType("numeric(18,2)");
                // Use PostgreSQL xmin for optimistic concurrency on this row
                b.Ignore(x => x.RowVersion);
                b.Property<uint>("xmin").IsRowVersion().IsConcurrencyToken().ValueGeneratedOnAddOrUpdate();
            });

            modelBuilder.Entity<Shared.Models.Cash.CashLedger>(b =>
            {
                b.ToTable("CashLedgers");
                b.HasKey(x => x.Id);
                b.Property(x => x.Delta).HasColumnType("numeric(18,2)");
                b.HasIndex(x => new { x.CompanyId, x.RefType, x.RefId }).IsUnique();
                b.HasIndex(x => new { x.CompanyId, x.CreatedAtUtc });
            });
            modelBuilder.Entity<Stripe_Webhook_Event>(b =>
            {
                b.ToTable("Stripe_Webhook_Events");
                b.HasIndex(x => x.EventId).IsUnique();
                b.Property(x => x.Payload).HasColumnType("jsonb");
            });
            modelBuilder.Entity<Company>(b =>
            {
                b.ToTable("Companies");
                b.HasKey(x => x.TenantID);

                // Explicit mapping for CompanyName
                b.Property(x => x.CompanyName)
                 .IsRequired()
                 .HasColumnName("CompanyName")
                 .HasColumnType("varchar(256)")
                 .ValueGeneratedNever(); // Important to force INSERT

                // Explicit mapping for DateIns with DB-side default
                b.Property(x => x.DateIns)
                 .HasColumnType("timestamp with time zone")
                 .HasDefaultValueSql("timezone('utc', now())")
                 .ValueGeneratedOnAdd();
            });
            modelBuilder.Entity<Company_Staff>(b =>
            {
                b.ToTable("Companies_Staff");

                b.Property(x => x.DateIns)
                 .HasColumnType("timestamp with time zone")              // timestamptz
                 .HasDefaultValueSql("timezone('utc', now())")           // DB-side default
                 .ValueGeneratedOnAdd();

                b.Property(x => x.CreatedBy).IsRequired(false);
            });


            modelBuilder.Entity<Company>().HasData(
                new Company
                {
                    TenantID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    CompanyName = "Company SRL",
                    MaxUsers = 50,
                    Avatar = null,
                    BusinessCategory = null,
                    EstimatedAnnualRevenue = null,
                    BusinessHours = null,
                    Website = null,
                    SocialLinks = null,
                    InternalRating = null,
                    PriorityLevel = 0,
                    VATRates = 22,
                    VAT = null,
                    SDI = null,
                    TIN = null,
                    AttorneyName = null,
                    AttorneyMiddleName = null,
                    AttorneySurname = null,
                    IBAN = null,
                    BIC = null,
                    SWIFT = null,
                    PreferredPaymentMethod = null,
                    MonthlyExpenseLimit = null,
                    BaseDiscountPercentage = null,
                    StartingContract = new DateTime(2025, 5, 27, 22, 24, 27, 530, DateTimeKind.Utc),
                    EndingContract = new DateTime(2035, 5, 27, 22, 24, 27, 530, DateTimeKind.Utc),
                    LicenseType = null,
                    GdprConsent = false,
                    GdprConsentDate = null,
                    ContractAcepted = false,
                    ContractAcceptedDate = null,
                    ContractVersion = null,
                    DefaultCurrency = "&euro;",
                    DefaultTimezone = null,
                    MobilePin = null,
                    MasterPassword = null,
                    IsActive = true,
                    StatusID = null,
                    IsDeleted = false,
                    IsDeletedBy = null,
                    IsDeletedWhy = null,
                    DateDeleted = null,
                    CreatedBy = null,
                    DateIns = new DateTime(2025, 5, 27, 22, 24, 27, 530, DateTimeKind.Utc),
                    EditedBy = null,
                    DateEdit = null,
                    CompanySecret = "bS8gmD_6L7zsADdQ17Q-MeeWB1yB5G4k0Q2Wy72yaFdEEJVzy2DhcinmWR3Tx45e68Bn8_b1t-1F35Co9uf_Bg",
                }
            );
            modelBuilder.Entity<Payment_Method_LookUps>().HasData(
                new Payment_Method_LookUps
                {
                    PaymentMethodID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    PaymentMethodName = "Credit Card",
                    PaymentMethodDescription = "Payment By Credit Card",
                    PaymentMethodIcon = null,
                    Visible = true,
                    DisplayOrder = 1,
                    TenantID = null,
                    IsDeleted = false,
                    CreatedBy = null,
                    DateIns = new DateTime(2025, 5, 27, 22, 24, 27, 530, DateTimeKind.Utc),
                },
                new Payment_Method_LookUps
                {
                    PaymentMethodID = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                    PaymentMethodName = "Cash",
                    PaymentMethodDescription = "Cash payments",
                    PaymentMethodIcon = null,
                    Visible = true,
                    DisplayOrder = 0,
                    TenantID = null,
                    IsDeleted = false,
                    CreatedBy = null,
                    DateIns = new DateTime(2025, 5, 27, 22, 24, 27, 530, DateTimeKind.Utc),
                },
                // new Payment_Method_LookUps
                // {
                //     PaymentMethodID = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                //     PaymentMethodName = "Custom method",
                //     PaymentMethodDescription = "method that can be deleted from company",
                //     PaymentMethodIcon = null,
                //     Visible = true,
                //     DisplayOrder = 0,
                //     TenantID = null,
                //     IsDeleted = false,
                //     CreatedBy = null,
                //     DateIns = new DateTime(2025, 5, 27, 22, 24, 27, 530, DateTimeKind.Utc),
                // },
                new Payment_Method_LookUps
                {
                    PaymentMethodID = Guid.Parse("00000000-0000-0000-0000-000000000004"),
                    PaymentMethodName = "Cash",
                    PaymentMethodDescription = "Cash payments",
                    PaymentMethodIcon = null,
                    Visible = true,
                    DisplayOrder = 0,
                    TenantID = Guid.Parse("00000000-0000-0000-0000-000000000002"), // secondary company
                    IsDeleted = false,
                    CreatedBy = null,
                    DateIns = new DateTime(2025, 5, 27, 22, 24, 27, 530, DateTimeKind.Utc),
                }
            );
            modelBuilder.Entity<Payment_DocumentType_LookUp>().HasData(
                new Payment_DocumentType_LookUp()
                {
                    DocumentTypeID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    DocumentTypeName = "Invoice",
                    DocumentTypeDescription = "Invoice",
                    DocumentTypeIcon = null,
                    Visible = true,
                    DisplayOrder = 0,
                    TenantID = null,
                    IsDeleted = false,
                    IsDeletedBy = null,
                    IsDeletedWhy = null,
                    DateDeleted = null,
                    CreatedBy = null,
                    DateIns = new DateTime(2025, 5, 27, 22, 24, 27, 530, DateTimeKind.Utc),
                    DateEdit = null,
                    EditedBy = null
                },
                new Payment_DocumentType_LookUp()
                {
                    DocumentTypeID = Guid.Parse("00000000-0000-0000-0000-000000000099"),
                    DocumentTypeName = "Receipt",
                    DocumentTypeDescription = "Receipt",
                    DocumentTypeIcon = null,
                    Visible = true,
                    DisplayOrder = 0,
                    TenantID = null,
                    IsDeleted = false,
                    IsDeletedBy = null,
                    IsDeletedWhy = null,
                    DateDeleted = null,
                    CreatedBy = null,
                    DateIns = new DateTime(2025, 5, 27, 22, 24, 27, 530, DateTimeKind.Utc),
                    DateEdit = null,
                    EditedBy = null
                },
                new Payment_DocumentType_LookUp()
                {
                    DocumentTypeID = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                    DocumentTypeName = "Second receipt type",
                    DocumentTypeDescription = "Receipt that can be deleted",
                    DocumentTypeIcon = null,
                    Visible = true,
                    DisplayOrder = 0,
                    TenantID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    IsDeleted = false,
                    IsDeletedBy = null,
                    IsDeletedWhy = null,
                    DateDeleted = null,
                    CreatedBy = null,
                    DateIns = new DateTime(2025, 5, 27, 22, 24, 27, 530, DateTimeKind.Utc),
                    DateEdit = null,
                    EditedBy = null
                },
                new Payment_DocumentType_LookUp()
                {
                    DocumentTypeID = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                    DocumentTypeName = "Third receipt type",
                    DocumentTypeDescription = "Receipt that cannot be deleted",
                    DocumentTypeIcon = null,
                    Visible = true,
                    DisplayOrder = 0,
                    TenantID = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                    IsDeleted = false,
                    IsDeletedBy = null,
                    IsDeletedWhy = null,
                    DateDeleted = null,
                    CreatedBy = null,
                    DateIns = new DateTime(2025, 5, 27, 22, 24, 27, 530, DateTimeKind.Utc),
                    DateEdit = null,
                    EditedBy = null
                }
            );
            modelBuilder.Entity<AspNetRole>().HasData(
                new AspNetRole
                {
                    RoleID = Configuration.AdministratorRoleID,
                    RoleName = "Administrator",
                    DateIns = new DateTime(2025, 6, 13, 22, 24, 27, 530, DateTimeKind.Utc),
                },
                new AspNetRole
                {
                    RoleID = Configuration.EmployeeRoleID,
                    RoleName = "Employee",
                    DateIns = new DateTime(2025, 6, 13, 22, 24, 27, 530, DateTimeKind.Utc),
                },
                new AspNetRole
                {
                    RoleID = Configuration.GIManagerRoleID,
                    RoleName = "GIManagers",
                    IsVisible = false,
                    DateIns = new DateTime(2025, 6, 13, 22, 24, 27, 530, DateTimeKind.Utc),
                }
            );
            modelBuilder.Entity<AspNetUser>().HasData(
                new AspNetUser
                {
                    UserID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    UserName = "Nestia User",
                    UserAvatar = null,
                    Language = "IT",
                    Country = "IT",
                    Timezone = null,
                    UserTitle = null,
                    UserFirstName = "Luca",
                    UserMiddleName = null,
                    UserLastName = "Baron",
                    Email = "baron_luca@nestia.local",
                    EmailConfirmed = true,
                    PhoneNumberPrefix = "+39",
                    PhoneNumber = "1234567890",
                    PhoneNumberConfirmed = true,
                    Gender = "Male",
                    Pronouns = null,
                    DoB = null,
                    PoB = null,
                    SoB = null,
                    CoB = null,
                    Nationality = "Italian",
                    PrivacyPolicyAcepted = false,
                    PrivacyPolicyAcceptedDate = null,
                    PrivacyPolicyVersion = null,
                    PasswordHash = "1J9y+7vb6zYOykos44K6UIWBs6yTIR52f6yJVE55N13=",
                    PasswordSalt = "4cB1NmkERk/TiMqrc2DONA==",
                    MobilePin = null,
                    SecurityStamp = null,
                    ConcurrencyStamp = null,
                    PasswordQuestion = "a",
                    PasswordAnswer = "dewafev[pi[w",
                    TwoFactorEnabled = false,
                    AccountValidUntil = null,
                    PasswordValidUntil = null,
                    LockoutEnd = null,
                    LockoutEnabled = false,
                    IsApproved = true,
                    AccessFailedCount = 0,
                    FailedPasswordAnswerAttemptCount = 0,
                    LastLoginDate = null,
                    LastAppLoginDate = null,
                    IpAddress = null,
                    LastKnownLocation = null,
                    IsDeleted = false,
                    IsDeletedBy = null,
                    IsDeletedWhy = null,
                    CreatedBy = null,
                    DateIns = new DateTime(2025, 5, 27, 22, 24, 27, 530, DateTimeKind.Utc),
                    EditedBy = null,
                    DateEdit = null,
                    QuickLoginPinHash = "lRpzr9szDAtETNymgtm7JJQT3PRIfmnjllPASChPxHk=",
                    UserMustChangePassword = false,
                    QuickLoginPinValidUntil = null
                }/*,
                new AspNetUser
                {
                    UserID = Guid.Parse("00000000-0000-0000-0000-000000000005"),
                    UserName = "TPignatta",
                    UserAvatar = null,
                    Language = "IT",
                    Country = "IT",
                    Timezone = null,
                    UserTitle = null,
                    UserFirstName = "Toni",
                    UserMiddleName = null,
                    UserLastName = "Pignatta",
                    Email = "ciccioamante63@aol.com",
                    EmailConfirmed = true,
                    PhoneNumberPrefix = "+39",
                    PhoneNumber = "3589545874",
                    PhoneNumberConfirmed = true,
                    Gender = "Male",
                    Pronouns = null,
                    DoB = null,
                    PoB = null,
                    SoB = null,
                    CoB = null,
                    Nationality = "Italian",
                    PrivacyPolicyAcepted = false,
                    PrivacyPolicyAcceptedDate = null,
                    PrivacyPolicyVersion = null,
                    PasswordHash = "1J9y+7vb6zYOykos49K6UIWBs6yTIR52f6yJVE55N18=",
                    PasswordSalt = "4cB1NmkERk/TiMqrc2DONA==",
                    MobilePin = null,
                    SecurityStamp = null,
                    ConcurrencyStamp = null,
                    PasswordQuestion = "a",
                    PasswordAnswer = "dewafev[pi[w",
                    TwoFactorEnabled = false,
                    AccountValidUntil = null,
                    PasswordValidUntil = null,
                    LockoutEnd = null,
                    LockoutEnabled = false,
                    IsApproved = true,
                    AccessFailedCount = 0,
                    FailedPasswordAnswerAttemptCount = 0,
                    LastLoginDate = null,
                    LastAppLoginDate = null,
                    IpAddress = null,
                    LastKnownLocation = null,
                    IsDeleted = false,
                    IsDeletedBy = null,
                    IsDeletedWhy = null,
                    CreatedBy = null,
                    DateIns = new DateTime(2025, 5, 27, 22, 24, 27, 530, DateTimeKind.Utc),
                    EditedBy = null,
                    DateEdit = null,
                    QuickLoginPinHash = "edjJX2/CU8gLFoHEWzgNBH5848+Z+OLGTczG7PrEujI=",
                    UserMustChangePassword = false,
                    QuickLoginPinValidUntil = null
                }*/
            );
            modelBuilder.Entity<AspNetUserRole>().HasData(
                new AspNetUserRole { RoleID = Configuration.AdministratorRoleID, UserID = Guid.Parse("00000000-0000-0000-0000-000000000001") },
                new AspNetUserRole { RoleID = Configuration.GIManagerRoleID, UserID = Guid.Parse("00000000-0000-0000-0000-000000000001") }
                );
            modelBuilder.Entity<Company_Staff>().HasData(
               new Company_Staff
               {
                   TenantID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                   UserID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                   TimeCost = null,
                   BadgeID = null,
                   OutOfReports = false,
                   RequireShiftCheckIn = true,
                   LastCheckIn = null,
                   LastCheckOut = null,
                   Role = null,
                   Department = null,
                   WorkLocation = null,
                   ContractStartDate = null,
                   ContractEndDate = null,
                   MonthlySalary = null,
                   Bonuses = null,
                   Allowances = null,
                   EmploymentType = null,
                   OvertimeRate = null,
                   Skills = null,
                   SupervisorID = null,
                   AccessLevel = null,
                   AuthorizedAreas = null,
                   InternalNotes = null,
                   PublicNotes = null,
                   ExternalSystemReference = null,
                   SyncStatus = null,
                   IsDeleted = false,
                   IsDeletedBy = null,
                   IsDeletedWhy = null,
                   DateDeleted = null,
                   CreatedBy = null,
                   DateIns = new DateTime(2025, 5, 27, 22, 24, 27, 530, DateTimeKind.Utc),
                   DateEdit = null,
                   EditedBy = null
               }
            );
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = Environment.GetEnvironmentVariable("DEFAULT_CONN_STRING") ??
                "Host=localhost;Database=opencashflow.cloud;Username=postgres;Password=postgres;";
                optionsBuilder.UseNpgsql(connectionString);
            }
        }

    }
}
