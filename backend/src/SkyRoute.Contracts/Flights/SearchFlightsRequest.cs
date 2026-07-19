namespace SkyRoute.Contracts.Flights;

public record SearchFlightsRequest(
    string Origin,
    string Destination,
    DateOnly DepartureDate,
    int NumberOfPassengers,
    string CabinClass);
