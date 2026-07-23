using SkyRoute.Core.ExternalServices.ArcticAir.Dtos;
using SkyRoute.Core.Models;

namespace SkyRoute.Core.ExternalServices.ArcticAir;

public interface IArcticAirProxy
{
    Task<ArcticAirSearchResponse> SearchFlightsAsync(
        SearchFlightRequest request,
        CancellationToken cancellationToken);

    Task<ArcticAirOffer?> GetFlightByIdAsync(
        string flightId,
        SearchFlightRequest request,
        CancellationToken cancellationToken);
}
