using SkyRoute.Core.Models.Validation;

namespace SkyRoute.Core.Features.Flights.SearchFlights;

public interface ISearchFlightValidationService
{
    ValidationResultDto ValidateRequest(SearchFlightsRequest request);
}
