namespace SkyRoute.Contracts.Flights;

public record FlightResult(
    string ProviderName,
    string FlightId,
    string Origin,
    string Destination,
    string OriginCountry,
    string DestinationCountry,
    DateTime DepartureTimeUtc,
    DateTime ArrivalTimeUtc,
    string CabinClass,
    decimal PricePerPassenger,
    decimal TotalPrice);
