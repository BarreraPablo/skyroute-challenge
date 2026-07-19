using SkyRoute.Core.Enums;

namespace SkyRoute.Core.Entities;

/// <summary>
/// Stores the passenger details captured by the booking form.
/// </summary>
public class Passenger
{
    /// <summary>
    /// Unique identifier for the passenger record. This is not the same as the passenger's document number.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key linking the passenger to a specific booking.
    /// </summary>
    public Guid BookingId { get; set; }

    /// <summary>
    /// The full name of the passenger as entered in the booking form.
    /// </summary>
    public required string FullName { get; set; }

    /// <summary>
    /// The email address of the passenger as entered in the booking form.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Document identifier entered by the user; the UI label and validation rule
    /// depend on whether the route is international or domestic.
    /// </summary>
    public required string DocumentNumber { get; set; }

    /// <summary>
    /// Indicates whether the document number should be treated as a passport or
    /// a national ID during booking validation.
    /// </summary>
    public DocumentType DocumentType { get; set; }

    public Booking Booking { get; set; } = null!;
}
