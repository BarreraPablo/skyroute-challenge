using SkyRoute.Core.ExternalServices;
using SkyRoute.Core.Models;
using SkyRoute.Core.Models.Validation;

namespace SkyRoute.Core.Features.Flights.SearchFlights;

public class SearchFlightService : ISearchFlightService
{
    private readonly IReadOnlyList<IFlightProviderExternalServiceStrategy> _flightProviders;
    private readonly ISearchFlightValidationService _validationService;

    public SearchFlightService(
        IEnumerable<IFlightProviderExternalServiceStrategy> flightProviders,
        ISearchFlightValidationService validationService)
    {
        _flightProviders = flightProviders.ToList();
        _validationService = validationService;
    }

    public async Task<(ValidationResultDto ValidationResult, SearchFlightsResponse Response)> SearchAsync(
        SearchFlightsRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = _validationService.ValidateRequest(request);

        if (validationResult.Conditions.Any(condition =>
            condition.Severity == ValidationSeverity.Error))
        {
            return (validationResult, new SearchFlightsResponse(Array.Empty<FlightResult>()));
        }

        var searchRequest = new SearchFlightRequest
        {
            Origin = request.OriginCode,
            Destination = request.DestinationCode,
            DepartureDate = request.DepartureDate,
            NumberOfPassengers = request.NumberOfPassengers,
            CabinClass = request.CabinClass
        };

        var providerTasks = _flightProviders
            .Select(provider => provider.SearchFlightsAsync(searchRequest, cancellationToken))
            .ToList();

        var providerResults = await Task.WhenAll(providerTasks);

        var flights = providerResults
            .SelectMany(flights => flights)
            .OrderBy(flight => flight.TotalPrice)
            .Select(MapToFlightResult)
            .ToList();

        return (validationResult, new SearchFlightsResponse(flights));
    }

    private static FlightResult MapToFlightResult(FlightResponse flight) =>
        new(
            flight.ProviderName,
            flight.FlightId,
            flight.Origin,
            flight.Destination,
            flight.OriginCountry,
            flight.DestinationCountry,
            flight.DepartureTimeUtc,
            flight.ArrivalTimeUtc,
            flight.CabinClass,
            flight.PricePerPassenger,
            flight.TotalPrice);
}
