using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DisasterApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRoleManagementSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserRole_Role",
                table: "UserRole");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Role__760965CCCEA47220",
                table: "Role");

            migrationBuilder.DropColumn(
                name: "role_id",
                table: "Role");

            migrationBuilder.RenameIndex(
                name: "UQ__Role__72E12F1B6DD5B5D7",
                table: "Role",
                newName: "IX_Role_Name");

            migrationBuilder.AlterColumn<int>(
                name: "role_id",
                table: "UserRole",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "Role",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Role",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Role",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(sysutcdatetime())");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Role",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "System");

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
                name: "UpdatedAt",
                table: "Role",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(sysutcdatetime())");

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Role",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "System");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Role",
                table: "Role",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserRole_Role",
                table: "UserRole",
                column: "role_id",
                principalTable: "Role",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserRole_Role",
                table: "UserRole");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Role",
                table: "Role");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Role");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Role");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Role");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Role");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Role");

            migrationBuilder.DropColumn(
                name: "IsSystem",
                table: "Role");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Role");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Role");

            migrationBuilder.RenameIndex(
                name: "IX_Role_Name",
                table: "Role",
                newName: "UQ__Role__72E12F1B6DD5B5D7");

            migrationBuilder.AlterColumn<Guid>(
                name: "role_id",
                table: "UserRole",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "Role",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<Guid>(
                name: "role_id",
                table: "Role",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "(newid())");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Role__760965CCCEA47220",
                table: "Role",
                column: "role_id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserRole_Role",
                table: "UserRole",
                column: "role_id",
                principalTable: "Role",
                principalColumn: "role_id");
        }
    }
}
