using Microsoft.Extensions.DependencyInjection;
using SkyRoute.Core.ExternalServices;
using SkyRoute.Core.ExternalServices.BudgetWings;
using SkyRoute.Core.ExternalServices.GlobalAir;
using SkyRoute.Core.Pricing;
using SkyRoute.Core.Services;

namespace SkyRoute.Core.DependencyInjection;
public static class SkyRouteCoreServiceCollectionExtensions
{
    public static IServiceCollection AddSkyRouteCore(this IServiceCollection services)
    {
        services.AddSingleton<IGlobalAirPricingStrategy, GlobalAirPricingStrategy>();
        services.AddSingleton<IBudgetWingsPricingStrategy, BudgetWingsPricingStrategy>();
        services.AddSingleton<IFlightProviderExternalService, GlobalAirService>();
        services.AddSingleton<IFlightProviderExternalService, BudgetWingsService>();
        services.AddSingleton<IAirportReferenceService, AirportReferenceService>();
        services.AddSingleton<ISearchFlightService, SearchFlightService>();

        return services;
    }
}