using SkyRoute.Contracts.Flights;

namespace SkyRoute.Core.Services;

public class AirportReferenceService : IAirportReferenceService
{
    private static readonly IReadOnlyList<AirportResponse> Airports =
    [
        new("JFK", "New York (JFK)", "United States"),
        new("LAX", "Los Angeles (LAX)", "United States"),
        new("ORD", "Chicago (ORD)", "United States"),
        new("MAD", "Madrid (MAD)", "Spain"),
        new("BCN", "Barcelona (BCN)", "Spain"),
        new("LHR", "London (LHR)", "United Kingdom")
    ];

    public IReadOnlyList<AirportResponse> GetAirports() => Airports;

    public bool IsValidAirportCode(string code) =>
        Airports.Any(airport => airport.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
}
