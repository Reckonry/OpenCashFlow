using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddStripeLookupIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Plans_StripePriceID",
                table: "Plans");

            migrationBuilder.DropIndex(
                name: "IX_Plans_StripeProductID",
                table: "Plans");

            migrationBuilder.DropIndex(
                name: "IX_Companies_Subscriptions_StripeInvoiceID",
                table: "Companies_Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Companies_Subscriptions_StripePriceID",
                table: "Companies_Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Companies_Subscriptions_StripeSubscriptionID",
                table: "Companies_Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Companies_BillingEmail",
                table: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_Companies_StripeCustomerID",
                table: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_Companies_StripeDefaultPaymentMethodID",
                table: "Companies");
        }
    }
}
