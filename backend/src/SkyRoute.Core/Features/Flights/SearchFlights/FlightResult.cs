namespace SkyRoute.Core.Features.Flights.SearchFlights;

public record FlightResult(
    string ProviderName,
    string FlightId,
    string OriginCode,
    string DestinationCode,
    string OriginCountryCode,
    string DestinationCountryCode,
    DateTime DepartureTimeUtc,
    DateTime ArrivalTimeUtc,
    string CabinClass,
    decimal PricePerPassenger,
    decimal TotalPrice);
