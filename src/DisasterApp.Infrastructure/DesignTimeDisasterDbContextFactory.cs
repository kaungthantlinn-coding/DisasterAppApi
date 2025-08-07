using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using DisasterApp.Infrastructure.Data; // Adjust namespace as needed

public class DesignTimeDisasterDbContextFactory : IDesignTimeDbContextFactory<DisasterDbContext>
{
    public DisasterDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DisasterDbContext>();
        // Use your actual connection string here
        optionsBuilder.UseSqlServer(
            "Server=localhost;Database=DisasterDb;Trusted_Connection=True;TrustServerCertificate=True;",
            b => b.MigrationsAssembly("DisasterApp.Infrastructure")
        );

        return new DisasterDbContext(optionsBuilder.Options);
    }
}
