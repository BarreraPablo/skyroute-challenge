using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SkyRoute.Core.Interfaces;
using SkyRoute.Core.ExternalServices.ArcticAir;
using SkyRoute.Core.ExternalServices.BudgetWings;
using SkyRoute.Core.ExternalServices.GlobalAir;
using SkyRoute.Infrastructure.Persistence;
using SkyRoute.Infrastructure.Persistence.Repositories;
using SkyRoute.Infrastructure.Proxies.ArcticAir;
using SkyRoute.Infrastructure.Proxies.BudgetWings;
using SkyRoute.Infrastructure.Proxies.GlobalAir;

namespace SkyRoute.Infrastructure.Extensions;

public static class SkyRouteInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddSkyRouteInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SkyRoute")
            ?? throw new InvalidOperationException("Connection string 'SkyRoute' was not found.");

        services.AddDbContext<SkyRouteDbContext>(options =>
            options.UseSqlServer(connectionString));
        services.AddScoped<IBookingRepository, BookingRepository>();

        services.AddSingleton<IGlobalAirProxy, GlobalAirProxy>();
        services.AddSingleton<IBudgetWingsProxy, BudgetWingsProxy>();
        services.AddSingleton<IArcticAirProxy, ArcticAirProxy>();

        return services;
    }
}