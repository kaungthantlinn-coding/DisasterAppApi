using DisasterApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DisasterApp.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DisasterDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DisasterDbContext>>();

        try
        {
            await SeedRolesAsync(context, logger);

            await SeedDisasterTypesAsync(context, logger);

            await SeedImpactTypesAsync(context, logger);

            await SeedSupportTypesAsync(context, logger);

            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private static async Task SeedRolesAsync(DisasterDbContext context, ILogger logger)
    {
        if (await context.Roles.AnyAsync())
        {
            logger.LogInformation("Roles already exist, skipping role seeding");
            return;
        }

        logger.LogInformation("Seeding roles...");

        var roles = new List<Role>
        {
            new Role
            {
                RoleId = Guid.NewGuid(),
                Name = "user"
            },
            new Role
            {
                RoleId = Guid.NewGuid(),
                Name = "cj"
            },
            new Role
            {
                RoleId = Guid.NewGuid(),
                Name = "admin"
            }
        };

        await context.Roles.AddRangeAsync(roles);
        logger.LogInformation("Successfully seeded {Count} roles", roles.Count);
    }



    private static async Task SeedDisasterTypesAsync(DisasterDbContext context, ILogger logger)
    {
        if (await context.DisasterTypes.AnyAsync())
        {
            logger.LogInformation("Disaster types already exist, skipping disaster type seeding");
            return;
        }

        logger.LogInformation("Seeding disaster types...");

        var disasterTypes = new List<DisasterType>
        {
            new DisasterType { Name = "Earthquake", Category = Domain.Enums.DisasterCategory.Natural },
            new DisasterType { Name = "Flood", Category = Domain.Enums.DisasterCategory.Natural },
            new DisasterType { Name = "Hurricane", Category = Domain.Enums.DisasterCategory.Natural },
            new DisasterType { Name = "Wildfire", Category = Domain.Enums.DisasterCategory.Natural },
            new DisasterType { Name = "Tornado", Category = Domain.Enums.DisasterCategory.Natural },
            new DisasterType { Name = "Tsunami", Category = Domain.Enums.DisasterCategory.Natural },
            new DisasterType { Name = "Volcanic Eruption", Category = Domain.Enums.DisasterCategory.Natural },
            new DisasterType { Name = "Landslide", Category = Domain.Enums.DisasterCategory.Natural },
            new DisasterType { Name = "Industrial Accident", Category = Domain.Enums.DisasterCategory.NonNatural },
            new DisasterType { Name = "Transportation Accident", Category = Domain.Enums.DisasterCategory.NonNatural },
            new DisasterType { Name = "Building Collapse", Category = Domain.Enums.DisasterCategory.NonNatural },
            new DisasterType { Name = "Chemical Spill", Category = Domain.Enums.DisasterCategory.NonNatural }
        };

        await context.DisasterTypes.AddRangeAsync(disasterTypes);
        logger.LogInformation("Successfully seeded {Count} disaster types", disasterTypes.Count);
    }

    private static async Task SeedImpactTypesAsync(DisasterDbContext context, ILogger logger)
    {
        if (await context.ImpactTypes.AnyAsync())
        {
            logger.LogInformation("Impact types already exist, skipping impact type seeding");
            return;
        }

        logger.LogInformation("Seeding impact types...");

        var impactTypes = new List<ImpactType>
        {
            new ImpactType { Name = "Infrastructure Damage" },
            new ImpactType { Name = "Casualties" },
            new ImpactType { Name = "Environmental Impact" },
            new ImpactType { Name = "Economic Loss" },
            new ImpactType { Name = "Displacement" },
            new ImpactType { Name = "Utility Disruption" },
            new ImpactType { Name = "Communication Disruption" },
            new ImpactType { Name = "Transportation Disruption" }
        };

        await context.ImpactTypes.AddRangeAsync(impactTypes);
        logger.LogInformation("Successfully seeded {Count} impact types", impactTypes.Count);
    }

    private static async Task SeedSupportTypesAsync(DisasterDbContext context, ILogger logger)
    {
        if (await context.SupportTypes.AnyAsync())
        {
            logger.LogInformation("Support types already exist, skipping support type seeding");
            return;
        }

        logger.LogInformation("Seeding support types...");

        var supportTypes = new List<SupportType>
        {
            new SupportType { Name = "Emergency Medical" },
            new SupportType { Name = "Search and Rescue" },
            new SupportType { Name = "Food and Water" },
            new SupportType { Name = "Shelter" },
            new SupportType { Name = "Transportation" },
            new SupportType { Name = "Communication" },
            new SupportType { Name = "Security" },
            new SupportType { Name = "Psychological Support" },
            new SupportType { Name = "Financial Aid" },
            new SupportType { Name = "Equipment and Supplies" }
        };

        await context.SupportTypes.AddRangeAsync(supportTypes);
        logger.LogInformation("Successfully seeded {Count} support types", supportTypes.Count);
    }



    private static async Task EnsureTwoFactorTablesExistAsync(DisasterDbContext context, ILogger logger)
    {
        logger.LogInformation("Ensuring 2FA tables exist...");

        try
        {
            var sql = @"
                -- Add 2FA columns to User table if they don't exist
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('[User]') AND name = 'two_factor_enabled')
                BEGIN
                    ALTER TABLE [User] ADD [two_factor_enabled] BIT NOT NULL DEFAULT 0;
                END

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('[User]') AND name = 'backup_codes_remaining')
                BEGIN
                    ALTER TABLE [User] ADD [backup_codes_remaining] INT NOT NULL DEFAULT 0;
                END

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('[User]') AND name = 'two_factor_last_used')
                BEGIN
                    ALTER TABLE [User] ADD [two_factor_last_used] DATETIME2 NULL;
                END

                -- Create OtpCode table if it doesn't exist
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OtpCode')
                BEGIN
                    CREATE TABLE [OtpCode] (
                        [id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
                        [user_id] UNIQUEIDENTIFIER NOT NULL,
                        [code] NVARCHAR(6) NOT NULL,
                        [type] NVARCHAR(20) NOT NULL,
                        [expires_at] DATETIME2 NOT NULL,
                        [used_at] DATETIME2 NULL,
                        [created_at] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
                        [attempt_count] INT NOT NULL DEFAULT 0,
                        CONSTRAINT [PK_OtpCode_Id] PRIMARY KEY ([id]),
                        CONSTRAINT [FK_OtpCode_User] FOREIGN KEY ([user_id]) REFERENCES [User] ([user_id]) ON DELETE CASCADE
                    );

                    CREATE INDEX [IX_OtpCode_UserId] ON [OtpCode] ([user_id]);
                    CREATE INDEX [IX_OtpCode_Code] ON [OtpCode] ([code]);
                    CREATE INDEX [IX_OtpCode_ExpiresAt] ON [OtpCode] ([expires_at]);
                    CREATE INDEX [IX_OtpCode_Type] ON [OtpCode] ([type]);
                END

                -- Create BackupCode table if it doesn't exist
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BackupCode')
                BEGIN
                    CREATE TABLE [BackupCode] (
                        [id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
                        [user_id] UNIQUEIDENTIFIER NOT NULL,
                        [code_hash] NVARCHAR(255) NOT NULL,
                        [used_at] DATETIME2 NULL,
                        [created_at] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
                        CONSTRAINT [PK_BackupCode_Id] PRIMARY KEY ([id]),
                        CONSTRAINT [FK_BackupCode_User] FOREIGN KEY ([user_id]) REFERENCES [User] ([user_id]) ON DELETE CASCADE
                    );

                    CREATE INDEX [IX_BackupCode_UserId] ON [BackupCode] ([user_id]);
                    CREATE INDEX [IX_BackupCode_CodeHash] ON [BackupCode] ([code_hash]);
                END

                -- Create OtpAttempt table if it doesn't exist
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OtpAttempt')
                BEGIN
                    CREATE TABLE [OtpAttempt] (
                        [id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
                        [user_id] UNIQUEIDENTIFIER NULL,
                        [ip_address] NVARCHAR(45) NOT NULL,
                        [attempt_type] NVARCHAR(20) NOT NULL,
                        [attempted_at] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
                        [success] BIT NOT NULL DEFAULT 0,
                        [email] NVARCHAR(255) NULL,
                        CONSTRAINT [PK_OtpAttempt_Id] PRIMARY KEY ([id]),
                        CONSTRAINT [FK_OtpAttempt_User] FOREIGN KEY ([user_id]) REFERENCES [User] ([user_id]) ON DELETE SET NULL
                    );

                    CREATE INDEX [IX_OtpAttempt_UserId_AttemptedAt] ON [OtpAttempt] ([user_id], [attempted_at]);
                    CREATE INDEX [IX_OtpAttempt_IpAddress_AttemptedAt] ON [OtpAttempt] ([ip_address], [attempted_at]);
                    CREATE INDEX [IX_OtpAttempt_Email_AttemptedAt] ON [OtpAttempt] ([email], [attempted_at]);
                    CREATE INDEX [IX_OtpAttempt_AttemptType] ON [OtpAttempt] ([attempt_type]);
                END";

            await context.Database.ExecuteSqlRawAsync(sql);
            logger.LogInformation("2FA tables ensured successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error ensuring 2FA tables exist");
            throw;
        }
    }
}