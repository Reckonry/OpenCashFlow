using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddStripeIntegrationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Companies_Companies_Subscriptions_Company_SubscriptionSubsc~",
                table: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_Companies_Company_SubscriptionSubscriptionID",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "Company_SubscriptionSubscriptionID",
                table: "Companies");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Plans",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean")
                .Annotation("Relational:ColumnOrder", 29)
                .OldAnnotation("Relational:ColumnOrder", 28);

            migrationBuilder.AddColumn<string>(
                name: "StripePriceID",
                table: "Plans",
                type: "varchar(100)",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 28);

            migrationBuilder.AddColumn<string>(
                name: "StripeProductID",
                table: "Plans",
                type: "varchar(100)",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 27);

            migrationBuilder.AddColumn<string>(
                name: "StripeInvoiceID",
                table: "Companies_Subscriptions",
                type: "varchar(100)",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 28);

            migrationBuilder.AddColumn<string>(
                name: "StripePriceID",
                table: "Companies_Subscriptions",
                type: "varchar(100)",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 27);

            migrationBuilder.AddColumn<string>(
                name: "StripeSubscriptionID",
                table: "Companies_Subscriptions",
                type: "varchar(100)",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 26);

            migrationBuilder.AddColumn<string>(
                name: "BillingEmail",
                table: "Companies",
                type: "varchar(256)",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 48);

            migrationBuilder.AddColumn<string>(
                name: "StripeCustomerID",
                table: "Companies",
                type: "varchar(100)",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 46);

            migrationBuilder.AddColumn<string>(
                name: "StripeDefaultPaymentMethodID",
                table: "Companies",
                type: "varchar(100)",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 47);

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

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "TenantID",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                columns: new[] { "BillingEmail", "StripeCustomerID", "StripeDefaultPaymentMethodID" },
                values: new object[] { null, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Subscriptions_TenantID",
                table: "Companies_Subscriptions",
                column: "TenantID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Renewals_PlanID",
                table: "Companies_Renewals",
                column: "PlanID");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Renewals_SubscriptionID",
                table: "Companies_Renewals",
                column: "SubscriptionID");

            migrationBuilder.AddForeignKey(
                name: "FK_Companies_Subscriptions_Companies_TenantID",
                table: "Companies_Subscriptions",
                column: "TenantID",
                principalTable: "Companies",
                principalColumn: "TenantID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Companies_Subscriptions_Companies_TenantID",
                table: "Companies_Subscriptions");

            migrationBuilder.DropTable(
                name: "Companies_Renewals");

            migrationBuilder.DropIndex(
                name: "IX_Companies_Subscriptions_TenantID",
                table: "Companies_Subscriptions");

            migrationBuilder.DropColumn(
                name: "StripePriceID",
                table: "Plans");

            migrationBuilder.DropColumn(
                name: "StripeProductID",
                table: "Plans");

            migrationBuilder.DropColumn(
                name: "StripeInvoiceID",
                table: "Companies_Subscriptions");

            migrationBuilder.DropColumn(
                name: "StripePriceID",
                table: "Companies_Subscriptions");

            migrationBuilder.DropColumn(
                name: "StripeSubscriptionID",
                table: "Companies_Subscriptions");

            migrationBuilder.DropColumn(
                name: "BillingEmail",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "StripeCustomerID",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "StripeDefaultPaymentMethodID",
                table: "Companies");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Plans",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean")
                .Annotation("Relational:ColumnOrder", 28)
                .OldAnnotation("Relational:ColumnOrder", 29);

            migrationBuilder.AddColumn<Guid>(
                name: "Company_SubscriptionSubscriptionID",
                table: "Companies",
                type: "uuid",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "TenantID",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "Company_SubscriptionSubscriptionID",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Company_SubscriptionSubscriptionID",
                table: "Companies",
                column: "Company_SubscriptionSubscriptionID");

            migrationBuilder.AddForeignKey(
                name: "FK_Companies_Companies_Subscriptions_Company_SubscriptionSubsc~",
                table: "Companies",
                column: "Company_SubscriptionSubscriptionID",
                principalTable: "Companies_Subscriptions",
                principalColumn: "SubscriptionID");
        }
    }
}
