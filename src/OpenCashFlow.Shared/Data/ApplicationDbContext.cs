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

        #region Legacy SaaS Schema
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
                }
            );
            modelBuilder.Entity<AspNetRole>().HasData(
                new AspNetRole
                {
                    RoleID = Configuration.AdministratorRoleID,
                    RoleName = Configuration.CompanyAdminRoleName,
                    DateIns = new DateTime(2025, 6, 13, 22, 24, 27, 530, DateTimeKind.Utc),
                },
                new AspNetRole
                {
                    RoleID = Configuration.EmployeeRoleID,
                    RoleName = Configuration.EmployeeRoleName,
                    DateIns = new DateTime(2025, 6, 13, 22, 24, 27, 530, DateTimeKind.Utc),
                },
                new AspNetRole
                {
                    RoleID = Configuration.InstanceAdminRoleID,
                    RoleName = Configuration.InstanceAdminRoleName,
                    IsVisible = false,
                    DateIns = new DateTime(2025, 6, 13, 22, 24, 27, 530, DateTimeKind.Utc),
                }
            );
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = Environment.GetEnvironmentVariable("DEFAULT_CONN_STRING") ??
                "Host=localhost;Database=opencashflow;Username=postgres;Password=postgres;";
                optionsBuilder.UseNpgsql(connectionString);
            }
        }

    }
}
