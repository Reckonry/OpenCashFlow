using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestIdToPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add RequestId column with temporary default for existing rows
            migrationBuilder.AddColumn<Guid>(
                name: "RequestId",
                table: "Payments",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()")
                .Annotation("Relational:ColumnOrder", 2);

            // Create unique constraint on TenantID + RequestId for idempotency
            migrationBuilder.CreateIndex(
                name: "IX_Payments_TenantID_RequestId",
                table: "Payments",
                columns: new[] { "TenantID", "RequestId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payments_TenantID_RequestId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "RequestId",
                table: "Payments");
        }
    }
}
