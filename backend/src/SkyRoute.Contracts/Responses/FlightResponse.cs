namespace SkyRoute.Contracts.Responses;

/// <summary>
/// 
/// </summary>
public record FlightResponse(
    string FlightId,
    string Provider,
    string Origin,
    string Destination,
    DateTime DepartureTimeUtc,
    DateTime ArrivalTimeUtc,
    string CabinClass,
    decimal FinalPrice 
);