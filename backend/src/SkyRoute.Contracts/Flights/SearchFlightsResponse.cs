namespace SkyRoute.Contracts.Flights;

public record SearchFlightsResponse(IReadOnlyList<FlightResult> Flights);
