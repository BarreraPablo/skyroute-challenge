namespace SkyRoute.Core.Features.Flights.SearchFlights;

public record SearchFlightsResponse(IReadOnlyList<FlightResult> Flights);
