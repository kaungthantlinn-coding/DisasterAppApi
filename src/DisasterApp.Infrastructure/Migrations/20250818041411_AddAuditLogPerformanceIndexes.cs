using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DisasterApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Primary index for timestamp ordering with included columns for covering index
            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_Timestamp_DESC",
                table: "AuditLog",
                column: "timestamp",
                descending: new[] { true })
                .Annotation("SqlServer:Include", new[] { "audit_log_id", "action", "severity", "details", "user_id", "user_name", "ip_address", "user_agent", "resource", "metadata" });

            // Index for user-specific queries
            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_UserId_Timestamp",
                table: "AuditLog",
                columns: new[] { "user_id", "timestamp" },
                descending: new[] { false, true },
                filter: "[user_id] IS NOT NULL");

            // Index for severity filtering
            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_Severity_Timestamp",
                table: "AuditLog",
                columns: new[] { "severity", "timestamp" },
                descending: new[] { false, true });

            // Index for action filtering
            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_Action_Timestamp",
                table: "AuditLog",
                columns: new[] { "action", "timestamp" },
                descending: new[] { false, true });

            // Index for resource filtering
            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_Resource_Timestamp",
                table: "AuditLog",
                columns: new[] { "resource", "timestamp" },
                descending: new[] { false, true });

            // Index for entity type filtering (used in role audit logs)
            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_EntityType_Timestamp",
                table: "AuditLog",
                columns: new[] { "entity_type", "timestamp" },
                descending: new[] { false, true });

            // Index for search operations
            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_Search",
                table: "AuditLog",
                columns: new[] { "user_name", "action", "timestamp" },
                descending: new[] { false, false, true })
                .Annotation("SqlServer:Include", new[] { "details" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AuditLog_Timestamp_DESC",
                table: "AuditLog");

            migrationBuilder.DropIndex(
                name: "IX_AuditLog_UserId_Timestamp",
                table: "AuditLog");

            migrationBuilder.DropIndex(
                name: "IX_AuditLog_Severity_Timestamp",
                table: "AuditLog");

            migrationBuilder.DropIndex(
                name: "IX_AuditLog_Action_Timestamp",
                table: "AuditLog");

            migrationBuilder.DropIndex(
                name: "IX_AuditLog_Resource_Timestamp",
                table: "AuditLog");

            migrationBuilder.DropIndex(
                name: "IX_AuditLog_EntityType_Timestamp",
                table: "AuditLog");

            migrationBuilder.DropIndex(
                name: "IX_AuditLog_Search",
                table: "AuditLog");
        }
    }
}
