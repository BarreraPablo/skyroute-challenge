namespace SkyRoute.Core.ExternalServices.ArcticAir.Dtos;

public class ArcticAirSearchResponse
{
    public required IReadOnlyList<ArcticAirOffer> Flights { get; set; }
}

public class ArcticAirOffer
{
    public int FlightGuid { get; set; }

    public required string FromCode { get; set; }

    public required string FromCountry { get; set; }

    public required string ToCode { get; set; }

    public required string ToCountry { get; set; }

    public DateTime DepartureTime { get; set; }

    public DateTime ArrivalTime { get; set; }

    public required string ClassType { get; set; }

    public decimal UnitPrice { get; set; }

    public int NumberOfPassengersAvailable { get; set; }
}
