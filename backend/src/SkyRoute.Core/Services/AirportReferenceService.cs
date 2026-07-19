using SkyRoute.Contracts.Flights;

namespace SkyRoute.Core.Services;

public class AirportReferenceService : IAirportReferenceService
{
    private static readonly IReadOnlyList<AirportResponse> Airports =
    [
        new("JFK", "New York (JFK)", "United States", "US"),
        new("LAX", "Los Angeles (LAX)", "United States", "US"),
        new("ORD", "Chicago (ORD)", "United States", "US"),
        new("MAD", "Madrid (MAD)", "Spain", "ES"),
        new("BCN", "Barcelona (BCN)", "Spain", "ES"),
        new("LHR", "London (LHR)", "United Kingdom", "GB")
    ];

    public IReadOnlyList<AirportResponse> GetAirports() => Airports;

    public bool IsValidAirportCode(string code) =>
        Airports.Any(airport => airport.Code.Equals(code, StringComparison.OrdinalIgnoreCase));

    public string? GetCountryCodeByAirportCode(string code) =>
        Airports.FirstOrDefault(airport => airport.Code.Equals(code, StringComparison.OrdinalIgnoreCase))?.CountryCode;
}
