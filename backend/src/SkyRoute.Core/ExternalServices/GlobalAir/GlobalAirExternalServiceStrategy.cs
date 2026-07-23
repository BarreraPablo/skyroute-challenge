using SkyRoute.Core.ExternalServices.GlobalAir.Dtos;
using SkyRoute.Core.Models;
using SkyRoute.Core.Pricing;

namespace SkyRoute.Core.ExternalServices.GlobalAir;

public class GlobalAirExternalServiceStrategy : IFlightProviderExternalServiceStrategy
{
    private readonly IGlobalAirProxy _proxy;
    private readonly IFlightPricingService _pricingService;

    public string ProviderName => "GlobalAir";

    public GlobalAirExternalServiceStrategy(IGlobalAirProxy proxy, IGlobalAirPricingService pricingService)
    {
        _proxy = proxy;
        _pricingService = pricingService;
    }

    public async Task<IReadOnlyList<FlightResponse>> SearchFlightsAsync(
        SearchFlightRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _proxy.SearchFlightsAsync(request, cancellationToken);

        return response.Results
            .Select(leg => MapToFlightResponse(leg, request.NumberOfPassengers))
            .ToList();
    }

    public async Task<FlightResponse?> GetFlightByIdAsync(
        string flightId,
        SearchFlightRequest request,
        CancellationToken cancellationToken)
    {
        var leg = await _proxy.GetFlightByIdAsync(flightId, request, cancellationToken);

        if (leg is null)
        {
            return null;
        }

        return MapToFlightResponse(leg, request.NumberOfPassengers);
    }

    private FlightResponse MapToFlightResponse(GlobalAirLeg leg, int numberOfPassengers)
    {
        var pricing = _pricingService.Calculate(leg.Fare.PricePerPax, numberOfPassengers);

        return new()
        {
            ProviderName = "GlobalAir",
            FlightId = leg.Id,
            Origin = leg.Departure.Airport,
            Destination = leg.Arrival.Airport,
            OriginCountry = leg.Departure.Country,
            DestinationCountry = leg.Arrival.Country,
            DepartureTimeUtc = leg.Schedule.DepartUtc,
            ArrivalTimeUtc = leg.Schedule.ArriveUtc,
            CabinClass = leg.Fare.Cabin,
            PricePerPassenger = pricing.PricePerPassenger,
            TotalPrice = pricing.TotalPrice
        };
    }
}
