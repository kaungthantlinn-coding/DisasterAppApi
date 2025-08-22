using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DisasterApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InsertSuperAdminRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insert superadmin role if it doesn't exist
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM [Role] WHERE [name] = 'superadmin')
                BEGIN
                    INSERT INTO [Role] ([role_id], [name])
                    VALUES (NEWID(), 'superadmin')
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove superadmin role
            migrationBuilder.Sql(@"
                DELETE FROM [Role] WHERE [name] = 'superadmin'
            ");
        }
    }
}
