using SkyRoute.Core.Services.Airports;

namespace SkyRoute.Core.Services;

public interface IAirportReferenceService
{
    IReadOnlyList<AirportResponse> GetAirports();

    bool IsValidAirportCode(string code);

    string? GetCountryCodeByAirportCode(string code);
}
