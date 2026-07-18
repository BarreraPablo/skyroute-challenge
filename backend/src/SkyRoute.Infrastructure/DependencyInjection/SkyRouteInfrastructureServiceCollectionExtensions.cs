using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SkyRoute.Infrastructure.DependencyInjection;

public static class SkyRouteInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddSkyRouteInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        return services;
    }
}