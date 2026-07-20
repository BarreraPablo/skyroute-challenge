namespace SkyRoute.Core.ExternalServices;

public class FlightResponse
{
    /// <summary>
    /// The name of the airline or travel provider offering the selected flight. This to identify the source of the flight data.
    /// </summary>
    public required string ProviderName { get; set; }

    /// <summary>
    /// The unique identifier of the flight selected by the user. This is used to retrieve the flight details from the provider's system.
    /// Setting it as string allows for flexibility in handling different provider formats and identifiers, which may not always be numeric.
    /// </summary>
    public required string FlightId { get; set; }

    /// <summary>
    /// The airport code or city name of the flight's origin. This is used to display the departure location to the user.
    /// </summary>
    public required string Origin { get; set; }

    /// <summary>
    /// The airport code or city name of the flight's destination. This is used to display the arrival location to the user.
    /// </summary>
    public required string Destination { get; set; }

    /// <summary>
    /// The country of the flight's origin. This is used for determining international travel requirements and for display purposes.
    /// </summary>
    public required string OriginCountry { get; set; }

    /// <summary>
    /// The country of the flight's destination. This is used for determining international travel requirements and for display purposes.
    /// </summary>
    public required string DestinationCountry { get; set; }

    /// <summary>
    /// The UTC timestamp for the flight's departure time.
    /// </summary>
    public DateTime DepartureTimeUtc { get; set; }

    /// <summary>
    /// The UTC timestamp for the flight's arrival time.
    /// </summary>
    public DateTime ArrivalTimeUtc { get; set; }

    /// <summary>
    /// The cabin class selected by the user for the flight. Could be Economy, Business, or First Class
    /// </summary>
    public required string CabinClass { get; set; }

    /// <summary>
    /// Price charged per passenger before multiplying by the number of passengers.
    /// </summary>
    public decimal PricePerPassenger { get; set; }

    /// <summary>
    /// The total price for the flight for the number of passengers.
    /// </summary>
    public decimal TotalPrice { get; set; }
}
