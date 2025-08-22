using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DisasterApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSuperAdminRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AuditLog_user_id",
                table: "AuditLog");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_Action_Timestamp",
                table: "AuditLog",
                columns: new[] { "action", "timestamp" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_EntityType_Timestamp",
                table: "AuditLog",
                columns: new[] { "entity_type", "timestamp" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_Resource_Timestamp",
                table: "AuditLog",
                columns: new[] { "resource", "timestamp" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_Search",
                table: "AuditLog",
                columns: new[] { "user_name", "action", "timestamp" },
                descending: new[] { false, false, true })
                .Annotation("SqlServer:Include", new[] { "details" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_Severity_Timestamp",
                table: "AuditLog",
                columns: new[] { "severity", "timestamp" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_Timestamp_DESC",
                table: "AuditLog",
                column: "timestamp",
                descending: new bool[0])
                .Annotation("SqlServer:Include", new[] { "audit_log_id", "action", "severity", "details", "user_id", "user_name", "ip_address", "user_agent", "resource", "metadata" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_UserId_Timestamp",
                table: "AuditLog",
                columns: new[] { "user_id", "timestamp" },
                descending: new[] { false, true },
                filter: "[user_id] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AuditLog_Action_Timestamp",
                table: "AuditLog");

            migrationBuilder.DropIndex(
                name: "IX_AuditLog_EntityType_Timestamp",
                table: "AuditLog");

            migrationBuilder.DropIndex(
                name: "IX_AuditLog_Resource_Timestamp",
                table: "AuditLog");

            migrationBuilder.DropIndex(
                name: "IX_AuditLog_Search",
                table: "AuditLog");

            migrationBuilder.DropIndex(
                name: "IX_AuditLog_Severity_Timestamp",
                table: "AuditLog");

            migrationBuilder.DropIndex(
                name: "IX_AuditLog_Timestamp_DESC",
                table: "AuditLog");

            migrationBuilder.DropIndex(
                name: "IX_AuditLog_UserId_Timestamp",
                table: "AuditLog");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_user_id",
                table: "AuditLog",
                column: "user_id");
        }
    }
}
