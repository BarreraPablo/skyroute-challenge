using Microsoft.Extensions.DependencyInjection;
using SkyRoute.Core.ExternalServices;
using SkyRoute.Core.ExternalServices.BudgetWings;
using SkyRoute.Core.ExternalServices.GlobalAir;
using SkyRoute.Core.Features.Bookings.CreateBooking;
using SkyRoute.Core.Features.Flights.SearchFlights;
using SkyRoute.Core.Pricing;
using SkyRoute.Core.Services;

namespace SkyRoute.Core.Extensions;
public static class SkyRouteCoreServiceCollectionExtensions
{
    public static IServiceCollection AddSkyRouteCore(this IServiceCollection services)
    {
        services.AddSingleton<IGlobalAirPricingService, GlobalAirPricingService>();
        services.AddSingleton<IBudgetWingsPricingService, BudgetWingsPricingService>();
        services.AddSingleton<IFlightProviderExternalServiceStrategy, GlobalAirExternalServiceStrategy>();
        services.AddSingleton<IFlightProviderExternalServiceStrategy, BudgetWingsExternalServiceStrategy>();
        services.AddSingleton<IAirportReferenceService, AirportReferenceService>();
        services.AddSingleton<ISearchFlightValidationService, SearchFlightValidationService>();
        services.AddSingleton<ISearchFlightService, SearchFlightService>();
        services.AddScoped<ICreateBookingValidationService, CreateBookingValidationService>();
        services.AddScoped<ICreateBookingService, CreateBookingService>();

        return services;
    }
}