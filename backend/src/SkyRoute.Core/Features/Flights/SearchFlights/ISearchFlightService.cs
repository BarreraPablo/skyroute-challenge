using SkyRoute.Core.Models.Validation;

namespace SkyRoute.Core.Features.Flights.SearchFlights;

public interface ISearchFlightService
{
    Task<(ValidationResultDto ValidationResult, SearchFlightsResponse Response)> SearchAsync(
        SearchFlightsRequest request,
        CancellationToken cancellationToken);
}
