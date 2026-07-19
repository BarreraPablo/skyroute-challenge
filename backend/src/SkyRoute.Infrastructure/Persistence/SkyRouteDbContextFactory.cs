using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SkyRoute.Infrastructure.Persistence;

public class SkyRouteDbContextFactory : IDesignTimeDbContextFactory<SkyRouteDbContext>
{
    public SkyRouteDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../SkyRoute.Api"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("SkyRoute")
            ?? throw new InvalidOperationException("Connection string 'SkyRoute' was not found.");

        var optionsBuilder = new DbContextOptionsBuilder<SkyRouteDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new SkyRouteDbContext(optionsBuilder.Options);
    }
}
