namespace SkyRoute.Core.ExternalServices.BudgetWings.Dtos;

public class BudgetWingsSearchResponse
{
    public required IReadOnlyList<BudgetWingsOffer> Flights { get; set; }
}

public class BudgetWingsOffer
{
    public Guid FlightGuid { get; set; }

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
