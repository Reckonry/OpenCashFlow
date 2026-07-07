using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenCashFlow.Infrastructure.Persistence.Migrations
{
    public partial class RemovePersonalDemoSeedData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var demoUserId = new Guid("00000000-0000-0000-0000-000000000001");
            var demoCompanyId = new Guid("00000000-0000-0000-0000-000000000001");

            migrationBuilder.DeleteData(
                table: "AspNetUsersRoles",
                keyColumns: new[] { "UserID", "RoleID" },
                keyValues: new object[] { demoUserId, new Guid("00000000-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "AspNetUsersRoles",
                keyColumns: new[] { "UserID", "RoleID" },
                keyValues: new object[] { demoUserId, new Guid("00000000-0000-0000-0000-000000000003") });

            migrationBuilder.DeleteData(
                table: "Companies_Staff",
                keyColumn: "UserID",
                keyValue: demoUserId);

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "UserID",
                keyValue: demoUserId);

            migrationBuilder.DeleteData(
                table: "Companies",
                keyColumn: "TenantID",
                keyValue: demoCompanyId);

            migrationBuilder.DeleteData(
                table: "Payments_DocumentTypes_LookUps",
                keyColumn: "DocumentTypeID",
                keyValue: new Guid("00000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Payments_DocumentTypes_LookUps",
                keyColumn: "DocumentTypeID",
                keyValue: new Guid("00000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Payments_Methods_Lookups",
                keyColumn: "PaymentMethodID",
                keyValue: new Guid("00000000-0000-0000-0000-000000000004"));

            migrationBuilder.Sql("""
                INSERT INTO "AspNetRoles" ("RoleID", "RoleName", "DateIns", "IsVisible", "IsDeleted")
                VALUES ('00000000-0000-0000-0000-000000000003', 'InstanceAdmin', TIMESTAMPTZ '2025-06-13 22:24:27.530Z', false, false)
                ON CONFLICT ("RoleID") DO UPDATE
                SET "RoleName" = EXCLUDED."RoleName",
                    "IsVisible" = EXCLUDED."IsVisible",
                    "IsDeleted" = EXCLUDED."IsDeleted";
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "RoleID",
                keyValue: new Guid("00000000-0000-0000-0000-000000000003"));
        }
    }
}
