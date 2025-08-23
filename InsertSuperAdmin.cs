using DisasterApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using DisasterApp.Domain.Entities;

namespace DisasterApp.Tools;

public class InsertSuperAdmin
{
    public static async Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("src/DisasterApp.WebApi/appsettings.json", optional: false)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        var options = new DbContextOptionsBuilder<DisasterDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        using var context = new DisasterDbContext(options);
        
        // Check if superadmin role already exists
        var existingSuperAdmin = await context.Roles
            .FirstOrDefaultAsync(r => r.Name.ToLower() == "superadmin");
            
        if (existingSuperAdmin != null)
        {
            Console.WriteLine("SuperAdmin role already exists.");
            return;
        }
        
        // Insert superadmin role
        var superAdminRole = new Role
        {
            RoleId = Guid.NewGuid(),
            Name = "superadmin"
        };
        
        await context.Roles.AddAsync(superAdminRole);
        await context.SaveChangesAsync();
        
        Console.WriteLine($"SuperAdmin role inserted successfully with ID: {superAdminRole.RoleId}");
    }
}