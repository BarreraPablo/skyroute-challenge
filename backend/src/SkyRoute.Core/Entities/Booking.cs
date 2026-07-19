namespace SkyRoute.Core.Entities;

/// <summary>
/// Captures the selected flight details shown on the booking screen
/// </summary>
public class Booking
{
    public Guid Id { get; set; }

    /// <summary>
    /// Unique booking reference returned after a successful confirm booking action.
    /// </summary>
    public required string BookingReference { get; set; }

    /// <summary>
    /// The UTC timestamp when the booking was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// The unique identifier of the flight selected by the user. This is used to retrieve the flight details from the provider's system.
    /// Setting it as string allows for flexibility in handling different provider formats and identifiers, which may not always be numeric.
    /// </summary>
    public required string FlightId { get; set; }

    /// <summary>
    /// The name of the airline or travel provider offering the selected flight. This to identify the source of the flight data.
    /// </summary>
    public required string Provider { get; set; }

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
    /// Indicates whether the flight is international or domestic. This is used to determine the appropriate document requirements and validation rules for the booking.
    /// </summary>
    public bool IsInternational { get; set; }

    /// <summary>
    /// The UTC timestamp for the flight's departure time.
    /// </summary>
    public DateTime DepartureTimeUtc { get; set; }

    /// <summary>
    /// The UTC timestamp for the flight's arrival time.
    /// </summary>
    public DateTime ArrivalTimeUtc { get; set; }

    /// <summary>
    /// The cabin class selected by the user for the flight.
    /// </summary>
    public required string CabinClass { get; set; }

    /// <summary>
    /// Price charged per passenger before multiplying by the number of passengers.
    /// </summary>
    public decimal PricePerPassenger { get; set; }

    /// <summary>
    /// Number of passengers included in the booking request.
    /// </summary>
    public int PassengerCount { get; set; }

    /// <summary>
    /// Total booking price derived from the per-passenger price and passenger count.
    /// </summary>
    public decimal TotalPrice { get; set; }

    /// <summary>
    /// Collection of passengers associated with this booking.
    /// The initial version of the application only supports a single passenger per booking, but this collection allows for future expansion to support multiple passengers.
    /// </summary>
    public ICollection<Passenger> Passengers { get; set; } = [];
}
