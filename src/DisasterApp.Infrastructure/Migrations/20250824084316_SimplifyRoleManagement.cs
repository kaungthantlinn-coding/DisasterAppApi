using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DisasterApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyRoleManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Backup existing role data
            migrationBuilder.Sql(@"
                CREATE TABLE #RoleBackup (
                    RoleId UNIQUEIDENTIFIER,
                    Name NVARCHAR(50)
                );
                
                INSERT INTO #RoleBackup (RoleId, Name)
                SELECT role_id, name FROM Role;
                
                CREATE TABLE #UserRoleBackup (
                    UserId UNIQUEIDENTIFIER,
                    RoleId UNIQUEIDENTIFIER
                );
                
                INSERT INTO #UserRoleBackup (UserId, RoleId)
                SELECT user_id, role_id FROM UserRole;
            ");

            // Drop existing constraints and indexes
            migrationBuilder.DropForeignKey(
                name: "FK_UserRole_Role",
                table: "UserRole");

            migrationBuilder.DropIndex(
                name: "IX_UserRole_role_id",
                table: "UserRole");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Role__760965CCCEA47220",
                table: "Role");

            migrationBuilder.DropIndex(
                name: "UQ__Role__72E12F1B6DD5B5D7",
                table: "Role");

            // Clear existing data
            migrationBuilder.Sql("DELETE FROM UserRole");
            migrationBuilder.Sql("DELETE FROM Role");

            // Drop old columns
            migrationBuilder.DropColumn(
                name: "role_id",
                table: "Role");

            // Add new columns
            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Role",
                type: "int",
                nullable: false)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Role",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Role",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSystem",
                table: "Role",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Role",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Role",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Role",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "System");

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Role",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "System");

            // Update name column
            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "Role",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            // Add new primary key
            migrationBuilder.AddPrimaryKey(
                name: "PK_Role",
                table: "Role",
                column: "Id");

            // Add unique constraint on name
            migrationBuilder.CreateIndex(
                name: "IX_Role_Name",
                table: "Role",
                column: "name",
                unique: true);

            // Update UserRole table to use int instead of Guid
            migrationBuilder.DropColumn(
                name: "role_id",
                table: "UserRole");

            migrationBuilder.AddColumn<int>(
                name: "role_id",
                table: "UserRole",
                type: "int",
                nullable: false);

            // Add new primary key for UserRole
            migrationBuilder.AddPrimaryKey(
                name: "PK_UserRole",
                table: "UserRole",
                columns: new[] { "user_id", "role_id" });

            // Add new foreign key
            migrationBuilder.AddForeignKey(
                name: "FK_UserRole_Role",
                table: "UserRole",
                column: "role_id",
                principalTable: "Role",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_role_id",
                table: "UserRole",
                column: "role_id");

            // Insert default roles
            migrationBuilder.Sql(@"
                INSERT INTO Role (name, Description, IsActive, IsSystem, CreatedBy, UpdatedBy)
                VALUES 
                ('SuperAdmin', 'Full system administrator with complete access to all features and settings', 1, 1, 'System', 'System'),
                ('Admin', 'System administrator with user management and operational oversight capabilities', 1, 1, 'System', 'System'),
                ('Manager', 'Department manager with team oversight and reporting capabilities', 1, 0, 'System', 'System'),
                ('User', 'Standard user with basic system access and reporting capabilities', 1, 0, 'System', 'System');
            ");

            // Migrate existing user roles (map old role names to new IDs)
            migrationBuilder.Sql(@"
                INSERT INTO UserRole (user_id, role_id)
                SELECT DISTINCT ur.UserId, r.Id
                FROM #UserRoleBackup ur
                INNER JOIN #RoleBackup rb ON ur.RoleId = rb.RoleId
                INNER JOIN Role r ON LOWER(rb.Name) = LOWER(r.name)
                WHERE r.Id IS NOT NULL;
                
                -- Clean up backup tables
                DROP TABLE #RoleBackup;
                DROP TABLE #UserRoleBackup;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // This is a destructive migration - down migration would lose data
            // Consider creating a separate migration if rollback is needed
            throw new NotSupportedException("This migration cannot be rolled back as it involves destructive changes to the role system.");
        }
    }
}
