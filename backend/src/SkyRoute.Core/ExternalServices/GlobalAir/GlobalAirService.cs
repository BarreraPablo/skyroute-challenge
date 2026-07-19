using SkyRoute.Core.ExternalServices.GlobalAir.Dtos;
using SkyRoute.Core.Models;
using SkyRoute.Core.Pricing;

namespace SkyRoute.Core.ExternalServices.GlobalAir;

public class GlobalAirService : IFlightProviderExternalService
{
    private readonly IGlobalAirProxy _proxy;
    private readonly IFlightPricingStrategy _pricingStrategy;

    public GlobalAirService(IGlobalAirProxy proxy, IGlobalAirPricingStrategy pricingStrategy)
    {
        _proxy = proxy;
        _pricingStrategy = pricingStrategy;
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

    private FlightResponse MapToFlightResponse(GlobalAirLeg leg, int numberOfPassengers)
    {
        var pricing = _pricingStrategy.Calculate(leg.Fare.PricePerPax, numberOfPassengers);

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
