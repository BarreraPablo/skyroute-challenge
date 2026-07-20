namespace SkyRoute.Core.Features.Flights.SearchFlights;

public record SearchFlightsRequest(
    string OriginCode,
    string DestinationCode,
    DateOnly DepartureDate,
    int NumberOfPassengers,
    string CabinClass);
