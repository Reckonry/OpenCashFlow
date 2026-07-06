using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Data.Migrations
{
    public partial class NormalizeSecurityRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "AspNetRoles"
                SET "RoleName" = 'CompanyAdmin'
                WHERE "RoleID" = '00000000-0000-0000-0000-000000000001';

                INSERT INTO "AspNetRoles" ("RoleID", "RoleName", "DateIns", "IsVisible", "IsDeleted")
                VALUES ('00000000-0000-0000-0000-000000000003', 'InstanceAdmin', now(), false, false)
                ON CONFLICT ("RoleID") DO UPDATE
                SET "RoleName" = EXCLUDED."RoleName",
                    "IsVisible" = EXCLUDED."IsVisible",
                    "IsDeleted" = EXCLUDED."IsDeleted";

                UPDATE "AspNetUsersRoles" target
                SET "RoleID" = '00000000-0000-0000-0000-000000000003'
                WHERE target."RoleID" = '00000000-9999-9999-9999-000000000009'
                  AND NOT EXISTS (
                      SELECT 1
                      FROM "AspNetUsersRoles" existing
                      WHERE existing."UserID" = target."UserID"
                        AND existing."RoleID" = '00000000-0000-0000-0000-000000000003'
                  );

                DELETE FROM "AspNetUsersRoles"
                WHERE "RoleID" = '00000000-9999-9999-9999-000000000009';

                DELETE FROM "AspNetRoles"
                WHERE "RoleID" = '00000000-9999-9999-9999-000000000009';
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "AspNetRoles"
                SET "RoleName" = 'Administrator'
                WHERE "RoleID" = '00000000-0000-0000-0000-000000000001';
                """);
        }
    }
}
