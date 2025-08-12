using System;
using System.Collections.Generic;
using DisasterApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DisasterApp.Infrastructure.Data;

public partial class DisasterDbContext : DbContext
{
    public DisasterDbContext()
    {
    }

    public DisasterDbContext(DbContextOptions<DisasterDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<Chat> Chats { get; set; }

    public virtual DbSet<DisasterEvent> DisasterEvents { get; set; }

    public virtual DbSet<DisasterReport> DisasterReports { get; set; }

    public virtual DbSet<DisasterType> DisasterTypes { get; set; }

    public virtual DbSet<Donation> Donations { get; set; }

    public virtual DbSet<ImpactDetail> ImpactDetails { get; set; }

    public virtual DbSet<ImpactType> ImpactTypes { get; set; }

    public virtual DbSet<Location> Locations { get; set; }

    public virtual DbSet<Organization> Organizations { get; set; }

    public virtual DbSet<Photo> Photos { get; set; }

    public virtual DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<SupportRequest> SupportRequests { get; set; }

    public virtual DbSet<SupportType> SupportTypes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<OtpCode> OtpCodes { get; set; }

    public virtual DbSet<BackupCode> BackupCodes { get; set; }

    public virtual DbSet<OtpAttempt> OtpAttempts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

        if (!optionsBuilder.IsConfigured)
        {

        }
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {


        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.AuditLogId).HasName("PK__AuditLog__3213E83F7F7A8BE5");

            entity.ToTable("AuditLog");

            entity.Property(e => e.AuditLogId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("audit_log_id");
            entity.Property(e => e.Action)
                .HasMaxLength(100)
                .HasColumnName("action");
            entity.Property(e => e.Severity)
                .HasMaxLength(20)
                .HasColumnName("severity");
            entity.Property(e => e.EntityType)
                .HasMaxLength(100)
                .HasColumnName("entity_type");
            entity.Property(e => e.EntityId)
                .HasMaxLength(100)
                .HasColumnName("entity_id");
            entity.Property(e => e.Details)
                .HasColumnType("nvarchar(max)")
                .HasColumnName("details");
            entity.Property(e => e.OldValues)
                .HasColumnType("nvarchar(max)")
                .HasColumnName("old_values");
            entity.Property(e => e.NewValues)
                .HasColumnType("nvarchar(max)")
                .HasColumnName("new_values");
            entity.Property(e => e.UserId)
                .HasColumnName("user_id");
            entity.Property(e => e.UserName)
                .HasMaxLength(100)
                .HasColumnName("user_name");
            entity.Property(e => e.Timestamp)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("timestamp");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(45)
                .HasColumnName("ip_address");
            entity.Property(e => e.UserAgent)
                .HasMaxLength(500)
                .HasColumnName("user_agent");
            entity.Property(e => e.Resource)
                .HasMaxLength(100)
                .HasColumnName("resource");
            entity.Property(e => e.Metadata)
                .HasColumnType("nvarchar(max)")
                .HasColumnName("metadata");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.User).WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_AuditLog_User");
        });

        modelBuilder.Entity<Chat>(entity =>
        {
            entity.HasKey(e => e.ChatId).HasName("PK__Chats__FD040B1769A39242");

            entity.Property(e => e.ChatId).HasColumnName("chat_id");
            entity.Property(e => e.AttachmentUrl)
                .HasMaxLength(512)
                .HasColumnName("attachment_url");
            entity.Property(e => e.IsRead)
                .HasDefaultValue(false)
                .HasColumnName("is_read");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.ReceiverId).HasColumnName("receiver_id");
            entity.Property(e => e.SenderId).HasColumnName("sender_id");
            entity.Property(e => e.SentAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("sent_at");

            entity.HasOne(d => d.Receiver).WithMany(p => p.ChatReceivers)
                .HasForeignKey(d => d.ReceiverId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Chat_Receiver");

            entity.HasOne(d => d.Sender).WithMany(p => p.ChatSenders)
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Chat_Sender");
        });



        modelBuilder.Entity<DisasterEvent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Disaster__3213E83F0C630599");

            entity.ToTable("DisasterEvent");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.DisasterTypeId).HasColumnName("disaster_type_id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");

            entity.HasOne(d => d.DisasterType).WithMany(p => p.DisasterEvents)
                .HasForeignKey(d => d.DisasterTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DisasterE__disas__41B8C09B");
        });

        modelBuilder.Entity<DisasterReport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Disaster__3213E83F1C33B9DA");

            entity.ToTable("DisasterReport");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DisasterEventId).HasColumnName("disaster_event_id");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Severity)
                .HasConversion<string>()
                .HasColumnName("severity");
            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasColumnName("status");
            entity.Property(e => e.Timestamp).HasColumnName("timestamp");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.VerifiedAt).HasColumnName("verified_at");
            entity.Property(e => e.VerifiedBy).HasColumnName("verified_by");

            entity.HasOne(d => d.DisasterEvent).WithMany(p => p.DisasterReports)
                .HasForeignKey(d => d.DisasterEventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DisasterReport_DisasterEvent");

            entity.HasOne(d => d.User).WithMany(p => p.DisasterReportUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DisasterReport_User");

            entity.HasOne(d => d.VerifiedByNavigation).WithMany(p => p.DisasterReportVerifiedByNavigations)
                .HasForeignKey(d => d.VerifiedBy)
                .HasConstraintName("FK_DisasterReport_VerifiedBy");
        });

        modelBuilder.Entity<DisasterType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Disaster__3213E83F43004DCF");

            entity.ToTable("DisasterType");

            entity.HasIndex(e => e.Name, "UQ__Disaster__72E12F1B433397EB").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Category)
                .HasConversion<string>()
                .HasColumnName("category");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Donation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Donation__3213E83F4C3E4EA3");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DonationType)
                .HasConversion<string>()
                .HasColumnName("donation_type");
            entity.Property(e => e.DonorContact)
                .HasMaxLength(255)
                .HasColumnName("donor_contact");
            entity.Property(e => e.DonorName)
                .HasMaxLength(100)
                .HasColumnName("donor_name");
            entity.Property(e => e.OrganizationId).HasColumnName("organization_id");
            entity.Property(e => e.ReceivedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("received_at");
            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasColumnName("status");
            entity.Property(e => e.TransactionPhotoUrl)
                .HasMaxLength(512)
                .HasColumnName("transaction_photo_url");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.VerifiedAt).HasColumnName("verified_at");
            entity.Property(e => e.VerifiedBy).HasColumnName("verified_by");

            entity.HasOne(d => d.Organization).WithMany(p => p.Donations)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Donations_Organization");

            entity.HasOne(d => d.User).WithMany(p => p.DonationUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Donations_User");

            entity.HasOne(d => d.VerifiedByNavigation).WithMany(p => p.DonationVerifiedByNavigations)
                .HasForeignKey(d => d.VerifiedBy)
                .HasConstraintName("FK_Donations_VerifiedBy");
        });

        modelBuilder.Entity<ImpactDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ImpactDe__3213E83F8E740B80");

            entity.ToTable("ImpactDetail");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description).HasColumnName("description");
           
            entity.Property(e => e.IsResolved)
                .HasDefaultValue(false)
                .HasColumnName("is_resolved");
            entity.Property(e => e.ReportId).HasColumnName("report_id");
            entity.Property(e => e.ResolvedAt).HasColumnName("resolved_at");
            entity.Property(e => e.Severity)
                .HasConversion<string>()
                .HasColumnName("severity");

            modelBuilder.Entity<ImpactDetail>()
    .HasMany(d => d.ImpactTypes)
    .WithMany(t => t.ImpactDetails)
    .UsingEntity<Dictionary<string, object>>(
        "ImpactDetailImpactType",  // join table name
        r => r.HasOne<ImpactType>()
              .WithMany()
              .HasForeignKey("ImpactTypeId")
              .HasConstraintName("FK_ImpactDetailImpactType_ImpactType")
              .OnDelete(DeleteBehavior.Cascade),
        l => l.HasOne<ImpactDetail>()
              .WithMany()
              .HasForeignKey("ImpactDetailId")
              .HasConstraintName("FK_ImpactDetailImpactType_ImpactDetail")
              .OnDelete(DeleteBehavior.Cascade),
        je =>
        {
            je.HasKey("ImpactDetailId", "ImpactTypeId");
            je.ToTable("ImpactDetailImpactType");
        }
    );

        });

        modelBuilder.Entity<ImpactType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ImpactTy__3213E83F560AA2C1");

            entity.ToTable("ImpactType");

            entity.HasIndex(e => e.Name, "UQ__ImpactTy__72E12F1B4AA4C730").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(e => e.LocationId).HasName("PK__Location__771831EA81520751");

            entity.ToTable("Location");

            entity.HasIndex(e => e.ReportId, "UQ__Location__779B7C59E4A3E040").IsUnique();

            entity.Property(e => e.LocationId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("location_id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.CoordinatePrecision)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("coordinate_precision");
            entity.Property(e => e.FormattedAddress)
                .HasMaxLength(512)
                .HasColumnName("formatted_address");
            entity.Property(e => e.Latitude)
                .HasColumnType("decimal(10, 8)")
                .HasColumnName("latitude");
            entity.Property(e => e.Longitude)
                .HasColumnType("decimal(11, 8)")
                .HasColumnName("longitude");
            entity.Property(e => e.ReportId).HasColumnName("report_id");

            entity.HasOne(d => d.Report).WithOne(p => p.Location)
                .HasForeignKey<Location>(d => d.ReportId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Location_Report");
        });

        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Organiza__3213E83FE53B6B28");

            entity.HasIndex(e => e.Name, "UQ__Organiza__72E12F1B7113F934").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ContactEmail)
                .HasMaxLength(255)
                .HasColumnName("contact_email");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsVerified)
                .HasDefaultValue(false)
                .HasColumnName("is_verified");
            entity.Property(e => e.LogoUrl)
                .HasMaxLength(512)
                .HasColumnName("logo_url");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.WebsiteUrl)
                .HasMaxLength(512)
                .HasColumnName("website_url");

            entity.HasOne(d => d.User).WithMany(p => p.Organizations)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Organizations_User");
        });

        modelBuilder.Entity<Photo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Photo__3213E83F676C6DC5");

            entity.ToTable("Photo");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Caption)
                .HasMaxLength(255)
                .HasColumnName("caption");
            entity.Property(e => e.PublicId)
                .HasMaxLength(255)
                .HasColumnName("public_id");
            entity.Property(e => e.ReportId).HasColumnName("report_id");
            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("uploaded_at");
            entity.Property(e => e.Url)
                .HasMaxLength(512)
                .HasColumnName("url");

            entity.HasOne(d => d.Report).WithMany(p => p.Photos)
                .HasForeignKey(d => d.ReportId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Photo_Report");
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasKey(e => e.PasswordResetTokenId).HasName("PK__Password__B0A1F7C71F7A268B");

            entity.ToTable("PasswordResetToken");

            entity.HasIndex(e => e.UserId, "IX_PasswordResetToken_user_id");

            entity.HasIndex(e => e.Token, "UQ__Password__CA90DA7A24B1CB17").IsUnique();

            entity.Property(e => e.PasswordResetTokenId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("password_reset_token_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.ExpiredAt).HasColumnName("expired_at");
            entity.Property(e => e.Token)
                .HasMaxLength(512)
                .HasColumnName("token");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.IsUsed)
                .HasDefaultValue(false)
                .HasColumnName("is_used");

            entity.HasOne(d => d.User).WithMany(p => p.PasswordResetTokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PasswordResetToken_User");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.RefreshTokenId).HasName("PK__RefreshT__B0A1F7C71F7A268A");

            entity.ToTable("RefreshToken");

            entity.HasIndex(e => e.UserId, "IX_RefreshToken_user_id");

            entity.HasIndex(e => e.Token, "UQ__RefreshT__CA90DA7A24B1CB16").IsUnique();

            entity.Property(e => e.RefreshTokenId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("refresh_token_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.ExpiredAt).HasColumnName("expired_at");
            entity.Property(e => e.Token)
                .HasMaxLength(512)
                .HasColumnName("token");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefreshToken_User");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Role__760965CCCEA47220");

            entity.ToTable("Role");

            entity.HasIndex(e => e.Name, "UQ__Role__72E12F1B6DD5B5D7").IsUnique();

            entity.Property(e => e.RoleId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("role_id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<SupportRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SupportR__3213E83F7F7A8BE5");

            entity.ToTable("SupportRequest");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ReportId).HasColumnName("report_id");
            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.Urgency).HasColumnName("urgency");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Report).WithMany(p => p.SupportRequests)
                .HasForeignKey(d => d.ReportId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SupportRequest_Report");

            entity.HasOne(d => d.User).WithMany(p => p.SupportRequests)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SupportRequest_User");

            entity.HasMany(d => d.SupportTypes).WithMany(p => p.SupportRequests)
                .UsingEntity<SupportRequestSupportType>(
                    "SupportRequestSupportType",
                    l => l.HasOne<SupportType>().WithMany().HasForeignKey("SupportTypeId")
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    r => r.HasOne<SupportRequest>().WithMany().HasForeignKey("SupportRequestId")
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j =>
                    {
                        j.HasKey("SupportRequestId", "SupportTypeId");
                        j.ToTable("SupportRequestSupportType");
                        j.HasIndex(new[] { "SupportRequestId" }, "IX_SupportRequestSupportType_SupportRequestId");
                        j.HasIndex(new[] { "SupportTypeId" }, "IX_SupportRequestSupportType_SupportTypeId");
                    });
        });

        modelBuilder.Entity<SupportType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SupportT__3213E83FC38913DF");

            entity.ToTable("SupportType");

            entity.HasIndex(e => e.Name, "UQ__SupportT__72E12F1B1361FEE0").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__B9BE370FF42BE9EA");

            entity.ToTable("User");

            entity.HasIndex(e => e.Email, "IX_User_Email");

            entity.HasIndex(e => new { e.AuthProvider, e.AuthId }, "UQ_User_AuthProviderId").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__User__AB6E61647E5028D0").IsUnique();

            entity.Property(e => e.UserId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("user_id");
            entity.Property(e => e.AuthId)
                .HasMaxLength(255)
                .HasColumnName("auth_id");
            entity.Property(e => e.AuthProvider)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("auth_provider");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.IsBlacklisted)
                .HasDefaultValue(false)
                .HasColumnName("is_blacklisted");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.PhotoUrl)
                .HasMaxLength(512)
                .HasColumnName("photo_url");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .HasColumnName("phone_number");
            entity.Property(e => e.TwoFactorEnabled)
                .HasDefaultValue(false)
                .HasColumnName("two_factor_enabled");
            entity.Property(e => e.BackupCodesRemaining)
                .HasDefaultValue(0)
                .HasColumnName("backup_codes_remaining");
            entity.Property(e => e.TwoFactorLastUsed)
                .HasColumnName("two_factor_last_used");

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserRole",
                    r => r.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_UserRole_Role"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_UserRole_User"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("UserRole");
                        j.HasIndex(new[] { "RoleId" }, "IX_UserRole_role_id");
                        j.IndexerProperty<Guid>("UserId").HasColumnName("user_id");
                        j.IndexerProperty<Guid>("RoleId").HasColumnName("role_id");
                    });
        });

        modelBuilder.Entity<OtpCode>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_OtpCode_Id");

            entity.ToTable("OtpCode");

            entity.HasIndex(e => e.UserId, "IX_OtpCode_UserId");
            entity.HasIndex(e => e.Code, "IX_OtpCode_Code");
            entity.HasIndex(e => e.ExpiresAt, "IX_OtpCode_ExpiresAt");
            entity.HasIndex(e => e.Type, "IX_OtpCode_Type");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.UserId)
                .HasColumnName("user_id");
            entity.Property(e => e.Code)
                .HasMaxLength(6)
                .HasColumnName("code");
            entity.Property(e => e.Type)
                .HasMaxLength(20)
                .HasColumnName("type");
            entity.Property(e => e.ExpiresAt)
                .HasColumnName("expires_at");
            entity.Property(e => e.UsedAt)
                .HasColumnName("used_at");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.AttemptCount)
                .HasDefaultValue(0)
                .HasColumnName("attempt_count");

            entity.HasOne(d => d.User).WithMany(p => p.OtpCodes)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_OtpCode_User");
        });

        modelBuilder.Entity<BackupCode>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_BackupCode_Id");

            entity.ToTable("BackupCode");

            entity.HasIndex(e => e.UserId, "IX_BackupCode_UserId");
            entity.HasIndex(e => e.CodeHash, "IX_BackupCode_CodeHash");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.UserId)
                .HasColumnName("user_id");
            entity.Property(e => e.CodeHash)
                .HasMaxLength(255)
                .HasColumnName("code_hash");
            entity.Property(e => e.UsedAt)
                .HasColumnName("used_at");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");

            entity.HasOne(d => d.User).WithMany(p => p.BackupCodes)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_BackupCode_User");
        });

        modelBuilder.Entity<OtpAttempt>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_OtpAttempt_Id");

            entity.ToTable("OtpAttempt");

            entity.HasIndex(e => new { e.UserId, e.AttemptedAt }, "IX_OtpAttempt_UserId_AttemptedAt");
            entity.HasIndex(e => new { e.IpAddress, e.AttemptedAt }, "IX_OtpAttempt_IpAddress_AttemptedAt");
            entity.HasIndex(e => new { e.Email, e.AttemptedAt }, "IX_OtpAttempt_Email_AttemptedAt");
            entity.HasIndex(e => e.AttemptType, "IX_OtpAttempt_AttemptType");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.UserId)
                .HasColumnName("user_id");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(45)
                .HasColumnName("ip_address");
            entity.Property(e => e.AttemptType)
                .HasMaxLength(20)
                .HasColumnName("attempt_type");
            entity.Property(e => e.AttemptedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("attempted_at");
            entity.Property(e => e.Success)
                .HasDefaultValue(false)
                .HasColumnName("success");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");

            entity.HasOne(d => d.User).WithMany(p => p.OtpAttempts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_OtpAttempt_User");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
