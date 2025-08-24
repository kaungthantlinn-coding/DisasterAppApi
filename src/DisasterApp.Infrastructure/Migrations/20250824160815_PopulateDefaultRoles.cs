using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DisasterApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PopulateDefaultRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Clear existing roles to avoid conflicts
            migrationBuilder.Sql("DELETE FROM [UserRole]");
            migrationBuilder.Sql("DELETE FROM [Role]");

            // Insert default roles with proper structure
            migrationBuilder.Sql(@"
                INSERT INTO [Role] ([name], [Description], [IsActive], [IsSystem], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy])
                VALUES 
                ('SuperAdmin', 'Super Administrator with full system access', 1, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 'System', 'System'),
                ('Admin', 'Administrator with management privileges', 1, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 'System', 'System'),
                ('CJ', 'Community Journalist with reporting capabilities', 1, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 'System', 'System'),
                ('User', 'Standard user with basic access', 1, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 'System', 'System')
            ");

            // Create SuperAdmin user if not exists and assign SuperAdmin role
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM [User] WHERE [email] = 'superadmin@disasterapp.com')
                BEGIN
                    INSERT INTO [User] ([user_id], [name], [email], [password_hash], [auth_provider], [is_blacklisted], [created_at])
                    VALUES (NEWID(), 'Super Admin', 'superadmin@disasterapp.com', '$2a$11$8K1p/a0dRTlHDkcLiDlOqOb8WGUpd3gfRR0em33wGSLnGw5s2uCof', 'Local', 0, SYSUTCDATETIME())
                END

                -- Assign SuperAdmin role to SuperAdmin user
                DECLARE @SuperAdminUserId UNIQUEIDENTIFIER = (SELECT [user_id] FROM [User] WHERE [email] = 'superadmin@disasterapp.com')
                DECLARE @SuperAdminRoleId INT = (SELECT [Id] FROM [Role] WHERE [name] = 'SuperAdmin')

                IF NOT EXISTS (SELECT 1 FROM [UserRole] WHERE [user_id] = @SuperAdminUserId AND [role_id] = @SuperAdminRoleId)
                BEGIN
                    INSERT INTO [UserRole] ([user_id], [role_id])
                    VALUES (@SuperAdminUserId, @SuperAdminRoleId)
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove default roles and user assignments
            migrationBuilder.Sql("DELETE FROM [UserRole] WHERE [role_id] IN (SELECT [Id] FROM [Role] WHERE [IsSystem] = 1)");
            migrationBuilder.Sql("DELETE FROM [Role] WHERE [IsSystem] = 1");
            migrationBuilder.Sql("DELETE FROM [User] WHERE [email] = 'superadmin@disasterapp.com'");
        }
    }
}
