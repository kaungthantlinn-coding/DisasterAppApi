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
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Seed roles
            await SeedRolesAsync(context, logger);

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
        // Check if roles already exist
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
}