using SkyRoute.Core.Models;

namespace SkyRoute.Core.ExternalServices;

public interface IFlightProviderExternalService
{
    string ProviderName { get; }

    Task<IReadOnlyList<FlightResponse>> SearchFlightsAsync(
        SearchFlightRequest request,
        CancellationToken cancellationToken);

    Task<FlightResponse?> GetFlightByIdAsync(
        string flightId,
        SearchFlightRequest request,
        CancellationToken cancellationToken);
}
