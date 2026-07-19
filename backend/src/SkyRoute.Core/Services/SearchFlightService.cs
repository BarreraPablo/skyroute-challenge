using SkyRoute.Contracts.Flights;
using SkyRoute.Contracts.Validation;
using SkyRoute.Core.ExternalServices;
using SkyRoute.Core.Models;

namespace SkyRoute.Core.Services;

public class SearchFlightService : ISearchFlightService
{
    private readonly IReadOnlyList<IFlightProviderExternalService> _flightProviders;
    private readonly IAirportReferenceService _airportReferenceService;

    public SearchFlightService(
        IEnumerable<IFlightProviderExternalService> flightProviders,
        IAirportReferenceService airportReferenceService)
    {
        _flightProviders = flightProviders.ToList();
        _airportReferenceService = airportReferenceService;
    }

    public async Task<(ValidationResultDto ValidationResult, SearchFlightsResponse Response)> SearchAsync(
        SearchFlightsRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = ValidateRequest(request);

        if (validationResult.Conditions.Any(condition =>
            condition.Severity == ValidationSeverity.Error))
        {
            return (validationResult, new SearchFlightsResponse(Array.Empty<FlightResult>()));
        }

        var searchRequest = new SearchFlightRequest
        {
            Origin = request.Origin,
            Destination = request.Destination,
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

    private ValidationResultDto ValidateRequest(SearchFlightsRequest request)
    {
        var validationResult = new ValidationResultDto();

        if (request is null)
        {
            validationResult.Conditions.Add(new ValidationDto
            {
                Severity = ValidationSeverity.Error,
                Message = "Request is required."
            });

            return validationResult;
        }

        if (string.IsNullOrWhiteSpace(request.Origin))
        {
            validationResult.Conditions.Add(new ValidationDto
            {
                Severity = ValidationSeverity.Error,
                Message = "Origin is required."
            });
        }

        if (string.IsNullOrWhiteSpace(request.Destination))
        {
            validationResult.Conditions.Add(new ValidationDto
            {
                Severity = ValidationSeverity.Error,
                Message = "Destination is required."
            });
        }

        if (!string.IsNullOrWhiteSpace(request.Origin) &&
            !string.IsNullOrWhiteSpace(request.Destination) &&
            request.Origin.Equals(request.Destination, StringComparison.OrdinalIgnoreCase))
        {
            validationResult.Conditions.Add(new ValidationDto
            {
                Severity = ValidationSeverity.Error,
                Message = "Origin and destination must be different."
            });
        }

        if (!string.IsNullOrWhiteSpace(request.Origin) &&
            !_airportReferenceService.IsValidAirportCode(request.Origin))
        {
            validationResult.Conditions.Add(new ValidationDto
            {
                Severity = ValidationSeverity.Error,
                Message = $"Unknown origin airport code: {request.Origin}."
            });
        }

        if (!string.IsNullOrWhiteSpace(request.Destination) &&
            !_airportReferenceService.IsValidAirportCode(request.Destination))
        {
            validationResult.Conditions.Add(new ValidationDto
            {
                Severity = ValidationSeverity.Error,
                Message = $"Unknown destination airport code: {request.Destination}."
            });
        }

        if (request.DepartureDate < DateOnly.FromDateTime(DateTime.UtcNow))
        {
            validationResult.Conditions.Add(new ValidationDto
            {
                Severity = ValidationSeverity.Error,
                Message = "Departure date cannot be in the past."
            });
        }

        if (request.NumberOfPassengers is < 1 or > 9)
        {
            validationResult.Conditions.Add(new ValidationDto
            {
                Severity = ValidationSeverity.Error,
                Message = "Number of passengers must be between 1 and 9."
            });
        }

        if (!CabinClasses.All.Contains(request.CabinClass, StringComparer.OrdinalIgnoreCase))
        {
            validationResult.Conditions.Add(new ValidationDto
            {
                Severity = ValidationSeverity.Error,
                Message = $"Cabin class must be one of: {string.Join(", ", CabinClasses.All)}."
            });
        }

        return validationResult;
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
