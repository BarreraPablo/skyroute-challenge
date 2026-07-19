using SkyRoute.Core.ExternalServices.GlobalAir.Dtos;
using SkyRoute.Core.Models;

namespace SkyRoute.Core.ExternalServices.GlobalAir;

public interface IGlobalAirProxy
{
    Task<GlobalAirAvailabilityResponse> SearchFlightsAsync(
        SearchFlightRequest request,
        CancellationToken cancellationToken);
}
