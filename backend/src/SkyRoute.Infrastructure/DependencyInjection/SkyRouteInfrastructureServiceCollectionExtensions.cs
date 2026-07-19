using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SkyRoute.Infrastructure.Persistence;

namespace SkyRoute.Infrastructure.DependencyInjection;

public static class SkyRouteInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddSkyRouteInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SkyRoute")
            ?? throw new InvalidOperationException("Connection string 'SkyRoute' was not found.");

        services.AddDbContext<SkyRouteDbContext>(options =>
            options.UseSqlServer(connectionString));

        return services;
    }
}