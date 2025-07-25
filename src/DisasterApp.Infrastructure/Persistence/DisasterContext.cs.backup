using System;
using System.Collections.Generic;
using DisasterApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DisasterApp.Infrastructure.Persistence;

public partial class DisasterContext : DbContext
{
    public DisasterContext()
    {
    }

    public DisasterContext(DbContextOptions<DisasterContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AssistanceProvided> AssistanceProvideds { get; set; }

    public virtual DbSet<Chat> Chats { get; set; }

    public virtual DbSet<DisasterReport> DisasterReports { get; set; }

    public virtual DbSet<DisasterType> DisasterTypes { get; set; }

    public virtual DbSet<ImpactDetail> ImpactDetails { get; set; }

    public virtual DbSet<Location> Locations { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Photo> Photos { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<SupportRequest> SupportRequests { get; set; }

    public virtual DbSet<Township> Townships { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Connection string is configured in Program.cs via dependency injection
        // No need to configure here when using DI
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AssistanceProvided>(entity =>
        {
            entity.HasKey(e => e.AssistanceId).HasName("PK__Assistan__13BD40CA36EA92BA");

            entity.ToTable("AssistanceProvided");

            entity.HasIndex(e => e.RequestId, "IX_AssistanceProvided_Request");

            entity.HasIndex(e => e.ProviderId, "IX_AssistanceProvided_provider_id");

            entity.Property(e => e.AssistanceId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("assistance_id");
            entity.Property(e => e.ProvidedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("provided_at");
            entity.Property(e => e.ProviderId).HasColumnName("provider_id");
            entity.Property(e => e.RequestId).HasColumnName("request_id");
            entity.Property(e => e.Status)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValue("Pending")
                .HasColumnName("status");

            entity.HasOne(d => d.Provider).WithMany(p => p.AssistanceProvideds)
                .HasForeignKey(d => d.ProviderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AssistanceProvided_Provider");

            entity.HasOne(d => d.Request).WithMany(p => p.AssistanceProvideds)
                .HasForeignKey(d => d.RequestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AssistanceProvided_Request");
        });

        modelBuilder.Entity<Chat>(entity =>
        {
            entity.HasKey(e => e.ChatId).HasName("PK__Chat__FD040B17E1346961");

            entity.ToTable("Chat");

            entity.HasIndex(e => new { e.ReceiverId, e.IsRead }, "IX_Chat_ReceiverUnread");

            entity.HasIndex(e => e.SenderId, "IX_Chat_sender_id");

            entity.Property(e => e.ChatId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("chat_id");
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

        modelBuilder.Entity<DisasterReport>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("PK__Disaster__779B7C58C8BC03EE");

            entity.ToTable("DisasterReport");

            entity.HasIndex(e => e.UserId, "IX_DisasterReport_User");

            entity.HasIndex(e => e.DisasterTypeId, "IX_DisasterReport_disaster_type_id");

            entity.HasIndex(e => e.VerifiedBy, "IX_DisasterReport_verified_by");

            entity.Property(e => e.ReportId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("report_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DisasterTypeId).HasColumnName("disaster_type_id");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Severity)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("severity");
            entity.Property(e => e.Status)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValue("Pending")
                .HasColumnName("status");
            entity.Property(e => e.Timestamp).HasColumnName("timestamp");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.VerifiedAt).HasColumnName("verified_at");
            entity.Property(e => e.VerifiedBy).HasColumnName("verified_by");

            entity.HasOne(d => d.DisasterType).WithMany(p => p.DisasterReports)
                .HasForeignKey(d => d.DisasterTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DisasterReport_DisasterType");

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
            entity.HasKey(e => e.DisasterTypeId).HasName("PK__Disaster__731C64A80F5A5268");

            entity.ToTable("DisasterType");

            entity.HasIndex(e => e.Name, "UQ__Disaster__72E12F1B58BC6E7D").IsUnique();

            entity.Property(e => e.DisasterTypeId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("disaster_type_id");
            entity.Property(e => e.Category)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("category");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<ImpactDetail>(entity =>
        {
            entity.HasKey(e => e.ImpactId).HasName("PK__ImpactDe__BBC672B339BEF2C0");

            entity.ToTable("ImpactDetail");

            entity.HasIndex(e => e.ReportId, "IX_ImpactDetail_report_id");

            entity.Property(e => e.ImpactId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("impact_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ImpactType)
                .HasMaxLength(50)
                .HasColumnName("impact_type");
            entity.Property(e => e.ReportId).HasColumnName("report_id");

            entity.HasOne(d => d.Report).WithMany(p => p.ImpactDetails)
                .HasForeignKey(d => d.ReportId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ImpactDetail_Report");
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(e => e.LocationId).HasName("PK__Location__771831EA61954BF0");

            entity.ToTable("Location");

            entity.HasIndex(e => e.ReportId, "UQ__Location__779B7C5953ED61E6").IsUnique();

            entity.Property(e => e.LocationId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("location_id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.GooglePlaceId)
                .HasMaxLength(100)
                .HasColumnName("google_place_id");
            entity.Property(e => e.Latitude)
                .HasColumnType("decimal(10, 8)")
                .HasColumnName("latitude");
            entity.Property(e => e.Longitude)
                .HasColumnType("decimal(11, 8)")
                .HasColumnName("longitude");
            entity.Property(e => e.ReportId).HasColumnName("report_id");
            entity.Property(e => e.TownshipId).HasColumnName("township_id");

            entity.HasOne(d => d.Report).WithOne(p => p.Location)
                .HasForeignKey<Location>(d => d.ReportId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Location_Report");

            entity.HasOne(d => d.Township).WithMany(p => p.Locations)
                .HasForeignKey(d => d.TownshipId)
                .HasConstraintName("FK_Location_Township");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__E059842F4A100656");

            entity.ToTable("Notification");

            entity.HasIndex(e => e.UserId, "IX_Notification_User");

            entity.Property(e => e.NotificationId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("notification_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.IsRead)
                .HasDefaultValue(false)
                .HasColumnName("is_read");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.RelatedEntity)
                .HasMaxLength(50)
                .HasColumnName("related_entity");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Notification_User");
        });

        modelBuilder.Entity<Photo>(entity =>
        {
            entity.HasKey(e => e.PhotoId).HasName("PK__Photo__CB48C83D9C040954");

            entity.ToTable("Photo");

            entity.HasIndex(e => e.ReportId, "IX_Photo_report_id");

            entity.Property(e => e.PhotoId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("photo_id");
            entity.Property(e => e.ReportId).HasColumnName("report_id");
            entity.Property(e => e.Type)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("type");
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
            entity.HasKey(e => e.RequestId).HasName("PK__SupportR__18D3B90F6E2CEB46");

            entity.ToTable("SupportRequest");

            entity.HasIndex(e => e.ReportId, "IX_SupportRequest_Report");

            entity.HasIndex(e => e.UserId, "IX_SupportRequest_user_id");

            entity.Property(e => e.RequestId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("request_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.QuantityNeeded).HasColumnName("quantity_needed");
            entity.Property(e => e.ReportId).HasColumnName("report_id");
            entity.Property(e => e.SupportType)
                .HasMaxLength(50)
                .HasColumnName("support_type");
            entity.Property(e => e.Urgency)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("urgency");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Report).WithMany(p => p.SupportRequests)
                .HasForeignKey(d => d.ReportId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SupportRequest_Report");

            entity.HasOne(d => d.User).WithMany(p => p.SupportRequests)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SupportRequest_User");
        });

        modelBuilder.Entity<Township>(entity =>
        {
            entity.HasKey(e => e.TownshipId).HasName("PK__Township__990ECE94E5741CA8");

            entity.ToTable("Township");

            entity.HasIndex(e => e.Name, "UQ__Township__72E12F1B97E035A0").IsUnique();

            entity.Property(e => e.TownshipId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("township_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Region)
                .HasMaxLength(100)
                .HasColumnName("region");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__B9BE370FF42BE9EA");

            entity.ToTable("User");

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

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
