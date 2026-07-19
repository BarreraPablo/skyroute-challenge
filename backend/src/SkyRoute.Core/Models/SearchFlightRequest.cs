namespace SkyRoute.Core.Models;

public class SearchFlightRequest
{
    public required string Origin { get; set; }

    public required string Destination { get; set; }

    public DateOnly DepartureDate { get; set; }

    public required int NumberOfPassengers { get; set; }

    public required string CabinClass { get; set; }
}
