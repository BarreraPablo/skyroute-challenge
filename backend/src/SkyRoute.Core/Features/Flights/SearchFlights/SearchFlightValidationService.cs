using SkyRoute.Core.Constants;
using SkyRoute.Core.Models.Validation;
using SkyRoute.Core.Services;

namespace SkyRoute.Core.Features.Flights.SearchFlights;

public class SearchFlightValidationService : ISearchFlightValidationService
{
    private readonly IAirportReferenceService _airportReferenceService;

    public SearchFlightValidationService(IAirportReferenceService airportReferenceService)
    {
        _airportReferenceService = airportReferenceService;
    }

    public ValidationResultDto ValidateRequest(SearchFlightsRequest request)
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

        if (string.IsNullOrWhiteSpace(request.OriginCode))
        {
            validationResult.Conditions.Add(new ValidationDto
            {
                Severity = ValidationSeverity.Error,
                Message = "Origin is required."
            });
        }

        if (string.IsNullOrWhiteSpace(request.DestinationCode))
        {
            validationResult.Conditions.Add(new ValidationDto
            {
                Severity = ValidationSeverity.Error,
                Message = "Destination is required."
            });
        }

        if (!string.IsNullOrWhiteSpace(request.OriginCode) &&
            !string.IsNullOrWhiteSpace(request.DestinationCode) &&
            request.OriginCode.Equals(request.DestinationCode, StringComparison.OrdinalIgnoreCase))
        {
            validationResult.Conditions.Add(new ValidationDto
            {
                Severity = ValidationSeverity.Error,
                Message = "Origin and destination must be different."
            });
        }

        if (!string.IsNullOrWhiteSpace(request.OriginCode) &&
            !_airportReferenceService.IsValidAirportCode(request.OriginCode))
        {
            validationResult.Conditions.Add(new ValidationDto
            {
                Severity = ValidationSeverity.Error,
                Message = $"Unknown origin airport code: {request.OriginCode}."
            });
        }

        if (!string.IsNullOrWhiteSpace(request.DestinationCode) &&
            !_airportReferenceService.IsValidAirportCode(request.DestinationCode))
        {
            validationResult.Conditions.Add(new ValidationDto
            {
                Severity = ValidationSeverity.Error,
                Message = $"Unknown destination airport code: {request.DestinationCode}."
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
}
