using SkyRoute.Contracts.Flights;

namespace SkyRoute.Core.Services;

public interface IAirportReferenceService
{
    IReadOnlyList<AirportResponse> GetAirports();

    bool IsValidAirportCode(string code);
}
