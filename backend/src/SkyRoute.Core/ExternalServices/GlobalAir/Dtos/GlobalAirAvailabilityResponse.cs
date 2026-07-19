namespace SkyRoute.Core.ExternalServices.GlobalAir.Dtos;

public class GlobalAirAvailabilityResponse
{
    public required IReadOnlyList<GlobalAirLeg> Results { get; set; }
}

public class GlobalAirLeg
{
    public required string Id { get; set; }

    public required GlobalAirLocation Departure { get; set; }

    public required GlobalAirLocation Arrival { get; set; }

    public required GlobalAirSchedule Schedule { get; set; }

    public required GlobalAirFare Fare { get; set; }

    public required GlobalAirInventory Inventory { get; set; }
}

public class GlobalAirLocation
{
    public required string Airport { get; set; }

    public required string Country { get; set; }
}

public class GlobalAirSchedule
{
    public DateTime DepartUtc { get; set; }

    public DateTime ArriveUtc { get; set; }
}

public class GlobalAirFare
{
    public required string Cabin { get; set; }

    public decimal PricePerPax { get; set; }
}

public class GlobalAirInventory
{
    public int NumberOfPassengersAvailable { get; set; }
}
