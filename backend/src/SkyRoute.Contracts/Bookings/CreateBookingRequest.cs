namespace SkyRoute.Contracts.Bookings;

public record CreateBookingRequest(
    string FlightId,
    string Provider,
    string OriginCode,
    string DestinationCode,
    DateTime DepartureTimeUtc,
    DateTime ArrivalTimeUtc,
    string CabinClass,
    decimal PricePerPassenger,
    int PassengerCount,
    decimal ExpectedPrice,
    CreateBookingPassengerRequest Passenger);

public record CreateBookingPassengerRequest(
    string FullName,
    string Email,
    string DocumentNumber);
