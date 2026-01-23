using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminAuditLog : Migration
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Admin_AuditLog");
        }
    }
}
