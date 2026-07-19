using SkyRoute.Core.Models;

namespace SkyRoute.Core.ExternalServices;

public interface IFlightProviderExternalService
{
    Task<IReadOnlyList<FlightResponse>> SearchFlightsAsync(
        SearchFlightRequest request,
        CancellationToken cancellationToken);
}
