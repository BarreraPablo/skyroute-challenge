using SkyRoute.Core.ExternalServices.BudgetWings.Dtos;
using SkyRoute.Core.Models;
using SkyRoute.Core.Pricing;

namespace SkyRoute.Core.ExternalServices.BudgetWings;

public class BudgetWingsExternalServiceStrategy : IFlightProviderExternalServiceStrategy
{
    private readonly IBudgetWingsProxy _proxy;
    private readonly IFlightPricingService _pricingService;

    public string ProviderName => "BudgetWings";

    public BudgetWingsExternalServiceStrategy(IBudgetWingsProxy proxy, IBudgetWingsPricingService pricingService)
    {
        _proxy = proxy;
        _pricingService = pricingService;
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

    public async Task<FlightResponse?> GetFlightByIdAsync(
        string flightId,
        SearchFlightRequest request,
        CancellationToken cancellationToken)
    {
        var offer = await _proxy.GetFlightByIdAsync(flightId, request, cancellationToken);

        if (offer is null)
        {
            return null;
        }

        return MapToFlightResponse(offer, request.NumberOfPassengers);
    }

    private FlightResponse MapToFlightResponse(BudgetWingsOffer offer, int numberOfPassengers)
    {
        var pricing = _pricingService.Calculate(offer.UnitPrice, numberOfPassengers);

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
