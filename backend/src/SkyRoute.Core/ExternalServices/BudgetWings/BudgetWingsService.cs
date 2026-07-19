using SkyRoute.Core.ExternalServices.BudgetWings.Dtos;
using SkyRoute.Core.Models;
using SkyRoute.Core.Pricing;

namespace SkyRoute.Core.ExternalServices.BudgetWings;

public class BudgetWingsService : IFlightProviderExternalService
{
    private readonly IBudgetWingsProxy _proxy;
    private readonly IFlightPricingStrategy _pricingStrategy;

    public BudgetWingsService(IBudgetWingsProxy proxy, IBudgetWingsPricingStrategy pricingStrategy)
    {
        _proxy = proxy;
        _pricingStrategy = pricingStrategy;
    }

    public async Task<IReadOnlyList<FlightResponse>> SearchFlightsAsync(
        SearchFlightRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _proxy.SearchFlightsAsync(request, cancellationToken);

        return response.Flights
            .Select(flight => MapToFlightResponse(flight, request.NumberOfPassengers))
            .ToList();
    }

    private FlightResponse MapToFlightResponse(BudgetWingsOffer offer, int numberOfPassengers)
    {
        var pricing = _pricingStrategy.Calculate(offer.UnitPrice, numberOfPassengers);

        return new()
        {
            ProviderName = "BudgetWings",
            FlightId = offer.FlightGuid.ToString(),
            Origin = offer.FromCode,
            Destination = offer.ToCode,
            OriginCountry = offer.FromCountry,
            DestinationCountry = offer.ToCountry,
            DepartureTimeUtc = offer.DepartureTime,
            ArrivalTimeUtc = offer.ArrivalTime,
            CabinClass = offer.ClassType,
            PricePerPassenger = pricing.PricePerPassenger,
            TotalPrice = pricing.TotalPrice
        };
    }
}
