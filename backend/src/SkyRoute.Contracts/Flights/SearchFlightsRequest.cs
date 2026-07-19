namespace SkyRoute.Contracts.Flights;

public record SearchFlightsRequest(
    string OriginCode,
    string DestinationCode,
    DateOnly DepartureDate,
    int NumberOfPassengers,
    string CabinClass);
