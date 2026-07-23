using SkyRoute.Core.Models;

namespace SkyRoute.Core.ExternalServices;

public interface IFlightProviderExternalServiceStrategy
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
