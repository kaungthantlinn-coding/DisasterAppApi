using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DisasterApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DisasterType",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    category = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Disaster__3213E83F43004DCF", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ImpactType",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ImpactTy__3213E83F560AA2C1", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    role_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Role__760965CCCEA47220", x => x.role_id);
                });

            migrationBuilder.CreateTable(
                name: "SupportType",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SupportT__3213E83FC38913DF", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    auth_provider = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    auth_id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    photo_url = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    phone_number = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    is_blacklisted = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(sysutcdatetime())"),
                    two_factor_enabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    backup_codes_remaining = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    two_factor_last_used = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__User__B9BE370FF42BE9EA", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "DisasterEvent",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    disaster_type_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Disaster__3213E83F0C630599", x => x.id);
                    table.ForeignKey(
                        name: "FK__DisasterE__disas__41B8C09B",
                        column: x => x.disaster_type_id,
                        principalTable: "DisasterType",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "AuditLog",
                columns: table => new
                {
                    audit_log_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    entity_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    entity_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    details = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    old_values = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    new_values = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    user_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    timestamp = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    ip_address = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    resource = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__AuditLog__3213E83F7F7A8BE5", x => x.audit_log_id);
                    table.ForeignKey(
                        name: "FK_AuditLog_User",
                        column: x => x.user_id,
                        principalTable: "User",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "BackupCode",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    code_hash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    used_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackupCode_Id", x => x.id);
                    table.ForeignKey(
                        name: "FK_BackupCode_User",
                        column: x => x.user_id,
                        principalTable: "User",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chats",
                columns: table => new
                {
                    chat_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    sender_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    receiver_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sent_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(sysutcdatetime())"),
                    is_read = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    attachment_url = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Chats__FD040B1769A39242", x => x.chat_id);
                    table.ForeignKey(
                        name: "FK_Chat_Receiver",
                        column: x => x.receiver_id,
                        principalTable: "User",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_Chat_Sender",
                        column: x => x.sender_id,
                        principalTable: "User",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    logo_url = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    website_url = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    contact_email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    is_verified = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(sysutcdatetime())"),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Organiza__3213E83FE53B6B28", x => x.id);
                    table.ForeignKey(
                        name: "FK_Organizations_User",
                        column: x => x.user_id,
                        principalTable: "User",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "OtpAttempt",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ip_address = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    attempt_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    attempted_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    success = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtpAttempt_Id", x => x.id);
                    table.ForeignKey(
                        name: "FK_OtpAttempt_User",
                        column: x => x.user_id,
                        principalTable: "User",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "OtpCode",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    code = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    used_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    attempt_count = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtpCode_Id", x => x.id);
                    table.ForeignKey(
                        name: "FK_OtpCode_User",
                        column: x => x.user_id,
                        principalTable: "User",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PasswordResetToken",
                columns: table => new
                {
                    password_reset_token_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    token = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    expired_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(sysutcdatetime())"),
                    is_used = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Password__B0A1F7C71F7A268B", x => x.password_reset_token_id);
                    table.ForeignKey(
                        name: "FK_PasswordResetToken_User",
                        column: x => x.user_id,
                        principalTable: "User",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "RefreshToken",
                columns: table => new
                {
                    refresh_token_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    token = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    expired_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__RefreshT__B0A1F7C71F7A268A", x => x.refresh_token_id);
                    table.ForeignKey(
                        name: "FK_RefreshToken_User",
                        column: x => x.user_id,
                        principalTable: "User",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "UserRole",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    role_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRole", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "FK_UserRole_Role",
                        column: x => x.role_id,
                        principalTable: "Role",
                        principalColumn: "role_id");
                    table.ForeignKey(
                        name: "FK_UserRole_User",
                        column: x => x.user_id,
                        principalTable: "User",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "DisasterReport",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    severity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    verified_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    verified_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    is_deleted = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    disaster_event_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Disaster__3213E83F1C33B9DA", x => x.id);
                    table.ForeignKey(
                        name: "FK_DisasterReport_DisasterEvent",
                        column: x => x.disaster_event_id,
                        principalTable: "DisasterEvent",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_DisasterReport_User",
                        column: x => x.user_id,
                        principalTable: "User",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_DisasterReport_VerifiedBy",
                        column: x => x.verified_by,
                        principalTable: "User",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "Donations",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    organization_id = table.Column<int>(type: "int", nullable: false),
                    donor_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    donor_contact = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    donation_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    amount = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    received_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true, defaultValue: "Pending"),
                    verified_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    verified_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Donation__3213E83F4C3E4EA3", x => x.id);
                    table.ForeignKey(
                        name: "FK_Donations_Organization",
                        column: x => x.organization_id,
                        principalTable: "Organizations",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_Donations_User",
                        column: x => x.user_id,
                        principalTable: "User",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_Donations_VerifiedBy",
                        column: x => x.verified_by,
                        principalTable: "User",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "ImpactDetail",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    report_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    severity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    is_resolved = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    resolved_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    impact_type_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ImpactDe__3213E83F8E740B80", x => x.id);
                    table.ForeignKey(
                        name: "FK_ImpactDetail_ImpactType",
                        column: x => x.impact_type_id,
                        principalTable: "ImpactType",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_ImpactDetail_Report",
                        column: x => x.report_id,
                        principalTable: "DisasterReport",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Location",
                columns: table => new
                {
                    location_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    report_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    latitude = table.Column<decimal>(type: "decimal(10,8)", nullable: false),
                    longitude = table.Column<decimal>(type: "decimal(11,8)", nullable: false),
                    address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    formatted_address = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    coordinate_precision = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Location__771831EA81520751", x => x.location_id);
                    table.ForeignKey(
                        name: "FK_Location_Report",
                        column: x => x.report_id,
                        principalTable: "DisasterReport",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Photo",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    report_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    url = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    caption = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    public_id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    uploaded_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Photo__3213E83F676C6DC5", x => x.id);
                    table.ForeignKey(
                        name: "FK_Photo_Report",
                        column: x => x.report_id,
                        principalTable: "DisasterReport",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "SupportRequest",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    report_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    urgency = table.Column<byte>(type: "tinyint", nullable: false),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    support_type_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SupportR__3213E83F7F7A8BE5", x => x.id);
                    table.ForeignKey(
                        name: "FK_SupportRequest_Report",
                        column: x => x.report_id,
                        principalTable: "DisasterReport",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_SupportRequest_SupportType",
                        column: x => x.support_type_id,
                        principalTable: "SupportType",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_SupportRequest_User",
                        column: x => x.user_id,
                        principalTable: "User",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_user_id",
                table: "AuditLog",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_BackupCode_CodeHash",
                table: "BackupCode",
                column: "code_hash");

            migrationBuilder.CreateIndex(
                name: "IX_BackupCode_UserId",
                table: "BackupCode",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Chats_receiver_id",
                table: "Chats",
                column: "receiver_id");

            migrationBuilder.CreateIndex(
                name: "IX_Chats_sender_id",
                table: "Chats",
                column: "sender_id");

            migrationBuilder.CreateIndex(
                name: "IX_DisasterEvent_disaster_type_id",
                table: "DisasterEvent",
                column: "disaster_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_DisasterReport_disaster_event_id",
                table: "DisasterReport",
                column: "disaster_event_id");

            migrationBuilder.CreateIndex(
                name: "IX_DisasterReport_user_id",
                table: "DisasterReport",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_DisasterReport_verified_by",
                table: "DisasterReport",
                column: "verified_by");

            migrationBuilder.CreateIndex(
                name: "UQ__Disaster__72E12F1B433397EB",
                table: "DisasterType",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Donations_organization_id",
                table: "Donations",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_Donations_user_id",
                table: "Donations",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Donations_verified_by",
                table: "Donations",
                column: "verified_by");

            migrationBuilder.CreateIndex(
                name: "IX_ImpactDetail_impact_type_id",
                table: "ImpactDetail",
                column: "impact_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_ImpactDetail_report_id",
                table: "ImpactDetail",
                column: "report_id");

            migrationBuilder.CreateIndex(
                name: "UQ__ImpactTy__72E12F1B4AA4C730",
                table: "ImpactType",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Location__779B7C59E4A3E040",
                table: "Location",
                column: "report_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_user_id",
                table: "Organizations",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "UQ__Organiza__72E12F1B7113F934",
                table: "Organizations",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OtpAttempt_AttemptType",
                table: "OtpAttempt",
                column: "attempt_type");

            migrationBuilder.CreateIndex(
                name: "IX_OtpAttempt_Email_AttemptedAt",
                table: "OtpAttempt",
                columns: new[] { "email", "attempted_at" });

            migrationBuilder.CreateIndex(
                name: "IX_OtpAttempt_IpAddress_AttemptedAt",
                table: "OtpAttempt",
                columns: new[] { "ip_address", "attempted_at" });

            migrationBuilder.CreateIndex(
                name: "IX_OtpAttempt_UserId_AttemptedAt",
                table: "OtpAttempt",
                columns: new[] { "user_id", "attempted_at" });

            migrationBuilder.CreateIndex(
                name: "IX_OtpCode_Code",
                table: "OtpCode",
                column: "code");

            migrationBuilder.CreateIndex(
                name: "IX_OtpCode_ExpiresAt",
                table: "OtpCode",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "IX_OtpCode_Type",
                table: "OtpCode",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "IX_OtpCode_UserId",
                table: "OtpCode",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetToken_user_id",
                table: "PasswordResetToken",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "UQ__Password__CA90DA7A24B1CB17",
                table: "PasswordResetToken",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Photo_report_id",
                table: "Photo",
                column: "report_id");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_user_id",
                table: "RefreshToken",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "UQ__RefreshT__CA90DA7A24B1CB16",
                table: "RefreshToken",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Role__72E12F1B6DD5B5D7",
                table: "Role",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupportRequest_report_id",
                table: "SupportRequest",
                column: "report_id");

            migrationBuilder.CreateIndex(
                name: "IX_SupportRequest_support_type_id",
                table: "SupportRequest",
                column: "support_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_SupportRequest_user_id",
                table: "SupportRequest",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "UQ__SupportT__72E12F1B1361FEE0",
                table: "SupportType",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                table: "User",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "UQ__User__AB6E61647E5028D0",
                table: "User",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_User_AuthProviderId",
                table: "User",
                columns: new[] { "auth_provider", "auth_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_role_id",
                table: "UserRole",
                column: "role_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLog");

            migrationBuilder.DropTable(
                name: "BackupCode");

            migrationBuilder.DropTable(
                name: "Chats");

            migrationBuilder.DropTable(
                name: "Donations");

            migrationBuilder.DropTable(
                name: "ImpactDetail");

            migrationBuilder.DropTable(
                name: "Location");

            migrationBuilder.DropTable(
                name: "OtpAttempt");

            migrationBuilder.DropTable(
                name: "OtpCode");

            migrationBuilder.DropTable(
                name: "PasswordResetToken");

            migrationBuilder.DropTable(
                name: "Photo");

            migrationBuilder.DropTable(
                name: "RefreshToken");

            migrationBuilder.DropTable(
                name: "SupportRequest");

            migrationBuilder.DropTable(
                name: "UserRole");

            migrationBuilder.DropTable(
                name: "Organizations");

            migrationBuilder.DropTable(
                name: "ImpactType");

            migrationBuilder.DropTable(
                name: "DisasterReport");

            migrationBuilder.DropTable(
                name: "SupportType");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.DropTable(
                name: "DisasterEvent");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "DisasterType");
        }
    }
}
