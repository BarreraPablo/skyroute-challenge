using SkyRoute.Contracts.Flights;
using SkyRoute.Contracts.Validation;

namespace SkyRoute.Core.Services;

public interface ISearchFlightService
{
    Task<(ValidationResultDto ValidationResult, SearchFlightsResponse Response)> SearchAsync(
        SearchFlightsRequest request,
        CancellationToken cancellationToken);
}
