using System.Net.Mail;
using SkyRoute.Core.Constants;
using SkyRoute.Core.Entities;
using SkyRoute.Core.Enums;
using SkyRoute.Core.ExternalServices;
using SkyRoute.Core.Interfaces;
using SkyRoute.Core.Models;
using SkyRoute.Core.Models.Validation;
using SkyRoute.Core.Services;

namespace SkyRoute.Core.Features.Bookings.CreateBooking;

public class CreateBookingService : ICreateBookingService
{
    private const decimal PriceTolerance = 0.01m;
    private const int BookingReferenceSuffixLength = 8;

    private readonly IReadOnlyList<IFlightProviderExternalService> _flightProviders;
    private readonly IBookingRepository _bookingRepository;
    private readonly IAirportReferenceService _airportReferenceService;

    public CreateBookingService(
        IEnumerable<IFlightProviderExternalService> flightProviders,
        IBookingRepository bookingRepository,
        IAirportReferenceService airportReferenceService)
    {
        _flightProviders = flightProviders.ToList();
        _bookingRepository = bookingRepository;
        _airportReferenceService = airportReferenceService;
    }

    public async Task<(ValidationResultDto ValidationResult, CreateBookingResponse? Response)> CreateAsync(
        CreateBookingRequest request,
        CancellationToken cancellationToken)
    {
        var validation = ValidateRequest(request);
        if (HasErrors(validation))
        {
            return Fail(validation, HttpStatus.BadRequest);
        }

        var provider = ResolveProvider(request.Provider);
        if (provider is null)
        {
            return Fail(validation, HttpStatus.BadRequest, $"Unknown provider: {request.Provider}.");
        }

        var providerFlight = await GetCurrentFlightAsync(provider, request, cancellationToken);
        if (providerFlight is null)
        {
            return Fail(
                validation,
                HttpStatus.Conflict,
                "Selected flight data is stale. Please search again and reselect your flight.");
        }

        var route = TryResolveRouteContext(request.OriginCode, request.DestinationCode);
        if (route is null)
        {
            return Fail(validation, HttpStatus.BadRequest, "Unable to resolve country for origin or destination.");
        }

        var booking = MapToBooking(request, route);
        await _bookingRepository.AddAsync(booking, cancellationToken);

        validation.StatusCode = HttpStatus.Ok;
        return (validation, new CreateBookingResponse(booking.BookingReference));
    }

    private IFlightProviderExternalService? ResolveProvider(string providerName) =>
        _flightProviders.FirstOrDefault(item =>
            item.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase));

    private async Task<FlightResponse?> GetCurrentFlightAsync(
        IFlightProviderExternalService provider,
        CreateBookingRequest request,
        CancellationToken cancellationToken)
    {
        var lookupRequest = new SearchFlightRequest
        {
            Origin = request.OriginCode,
            Destination = request.DestinationCode,
            DepartureDate = DateOnly.FromDateTime(request.DepartureTimeUtc),
            NumberOfPassengers = request.PassengerCount,
            CabinClass = request.CabinClass
        };

        var providerFlight = await provider.GetFlightByIdAsync(request.FlightId, lookupRequest, cancellationToken);
        return providerFlight is not null && IsSameFlight(providerFlight, request)
            ? providerFlight
            : null;
    }

    private static Booking MapToBooking(CreateBookingRequest request, RouteContext route) =>
        new()
        {
            Id = Guid.NewGuid(),
            BookingReference = GenerateBookingReference(),
            CreatedAtUtc = DateTime.UtcNow,
            FlightId = request.FlightId,
            Provider = request.Provider,
            Origin = request.OriginCode,
            Destination = request.DestinationCode,
            OriginCountry = route.OriginCountryCode,
            DestinationCountry = route.DestinationCountryCode,
            IsInternational = route.IsInternational,
            DepartureTimeUtc = request.DepartureTimeUtc,
            ArrivalTimeUtc = request.ArrivalTimeUtc,
            CabinClass = request.CabinClass,
            PricePerPassenger = request.PricePerPassenger,
            PassengerCount = request.PassengerCount,
            TotalPrice = request.ExpectedPrice,
            Passengers =
            [
                new Passenger
                {
                    Id = Guid.NewGuid(),
                    FullName = request.Passenger.FullName.Trim(),
                    Email = request.Passenger.Email.Trim(),
                    DocumentNumber = request.Passenger.DocumentNumber.Trim(),
                    DocumentType = route.IsInternational ? DocumentType.Passport : DocumentType.NationalId
                }
            ]
        };

    private static bool IsSameFlight(FlightResponse providerFlight, CreateBookingRequest request) =>
        providerFlight.FlightId.Equals(request.FlightId, StringComparison.OrdinalIgnoreCase)
        && providerFlight.ProviderName.Equals(request.Provider, StringComparison.OrdinalIgnoreCase)
        && providerFlight.Origin.Equals(request.OriginCode, StringComparison.OrdinalIgnoreCase)
        && providerFlight.Destination.Equals(request.DestinationCode, StringComparison.OrdinalIgnoreCase)
        && providerFlight.DepartureTimeUtc == request.DepartureTimeUtc
        && providerFlight.ArrivalTimeUtc == request.ArrivalTimeUtc
        && providerFlight.CabinClass.Equals(request.CabinClass, StringComparison.OrdinalIgnoreCase)
        && Math.Abs(providerFlight.PricePerPassenger - request.PricePerPassenger) <= PriceTolerance
        && Math.Abs(providerFlight.TotalPrice - request.ExpectedPrice) <= PriceTolerance;

    private ValidationResultDto ValidateRequest(CreateBookingRequest request)
    {
        var validation = new ValidationResultDto();

        ValidateRequiredFields(request, validation);
        ValidateRoute(request, validation);
        ValidatePricingAndCabin(request, validation);
        ValidateSchedule(request, validation);
        ValidatePassenger(request, validation);

        return validation;
    }

    private static void ValidateRequiredFields(CreateBookingRequest request, ValidationResultDto validation)
    {
        if (string.IsNullOrWhiteSpace(request.FlightId))
        {
            validation.Conditions.Add(CreateError("FlightId is required."));
        }

        if (string.IsNullOrWhiteSpace(request.Provider))
        {
            validation.Conditions.Add(CreateError("Provider is required."));
        }

        if (string.IsNullOrWhiteSpace(request.OriginCode))
        {
            validation.Conditions.Add(CreateError("OriginCode is required."));
        }

        if (string.IsNullOrWhiteSpace(request.DestinationCode))
        {
            validation.Conditions.Add(CreateError("DestinationCode is required."));
        }
    }

    private void ValidateRoute(CreateBookingRequest request, ValidationResultDto validation)
    {
        if (!string.IsNullOrWhiteSpace(request.OriginCode)
            && !_airportReferenceService.IsValidAirportCode(request.OriginCode))
        {
            validation.Conditions.Add(CreateError($"Unknown origin airport code: {request.OriginCode}."));
        }

        if (!string.IsNullOrWhiteSpace(request.DestinationCode)
            && !_airportReferenceService.IsValidAirportCode(request.DestinationCode))
        {
            validation.Conditions.Add(CreateError($"Unknown destination airport code: {request.DestinationCode}."));
        }

        if (!string.IsNullOrWhiteSpace(request.OriginCode)
            && !string.IsNullOrWhiteSpace(request.DestinationCode)
            && request.OriginCode.Equals(request.DestinationCode, StringComparison.OrdinalIgnoreCase))
        {
            validation.Conditions.Add(CreateError("Origin and destination must be different."));
        }

        if (!string.IsNullOrWhiteSpace(request.OriginCode)
            && !string.IsNullOrWhiteSpace(request.DestinationCode)
            && _airportReferenceService.IsValidAirportCode(request.OriginCode)
            && _airportReferenceService.IsValidAirportCode(request.DestinationCode)
            && TryResolveRouteContext(request.OriginCode, request.DestinationCode) is null)
        {
            validation.Conditions.Add(CreateError("Unable to resolve country for origin or destination."));
        }
    }

    private static void ValidatePricingAndCabin(CreateBookingRequest request, ValidationResultDto validation)
    {
        if (request.PassengerCount is < BookingLimits.MinPassengerCount or > BookingLimits.MaxPassengerCount)
        {
            validation.Conditions.Add(CreateError(
                $"Passenger count must be between {BookingLimits.MinPassengerCount} and {BookingLimits.MaxPassengerCount}."));
        }

        if (request.ExpectedPrice <= 0)
        {
            validation.Conditions.Add(CreateError("Expected price must be greater than zero."));
        }

        if (request.PricePerPassenger <= 0)
        {
            validation.Conditions.Add(CreateError("Price per passenger must be greater than zero."));
        }

        if (!CabinClasses.All.Contains(request.CabinClass, StringComparer.OrdinalIgnoreCase))
        {
            validation.Conditions.Add(CreateError($"Cabin class must be one of: {string.Join(", ", CabinClasses.All)}."));
        }
    }

    private static void ValidateSchedule(CreateBookingRequest request, ValidationResultDto validation)
    {
        if (request.ArrivalTimeUtc <= request.DepartureTimeUtc)
        {
            validation.Conditions.Add(CreateError("Arrival time must be after departure time."));
        }
    }

    private void ValidatePassenger(CreateBookingRequest request, ValidationResultDto validation)
    {
        if (request.Passenger is null)
        {
            validation.Conditions.Add(CreateError("Passenger details are required."));
            return;
        }

        if (string.IsNullOrWhiteSpace(request.Passenger.FullName))
        {
            validation.Conditions.Add(CreateError("Passenger full name is required."));
        }

        if (string.IsNullOrWhiteSpace(request.Passenger.Email))
        {
            validation.Conditions.Add(CreateError("Passenger email is required."));
        }
        else if (!IsValidEmail(request.Passenger.Email))
        {
            validation.Conditions.Add(CreateError("Passenger email is invalid."));
        }

        if (string.IsNullOrWhiteSpace(request.Passenger.DocumentNumber))
        {
            validation.Conditions.Add(CreateError("Passenger document number is required."));
            return;
        }

        var route = TryResolveRouteContext(request.OriginCode, request.DestinationCode);
        if (route is null)
        {
            return;
        }

        if (route.IsInternational)
        {
            if (!IsValidPassportNumber(request.Passenger.DocumentNumber))
            {
                validation.Conditions.Add(CreateError("Passport Number is invalid for international flights."));
            }
        }
        else if (!IsValidNationalId(request.Passenger.DocumentNumber))
        {
            validation.Conditions.Add(CreateError("National ID is invalid for domestic flights."));
        }
    }

    private RouteContext? TryResolveRouteContext(string originCode, string destinationCode)
    {
        if (string.IsNullOrWhiteSpace(originCode) || string.IsNullOrWhiteSpace(destinationCode))
        {
            return null;
        }

        var originCountry = _airportReferenceService.GetCountryCodeByAirportCode(originCode);
        var destinationCountry = _airportReferenceService.GetCountryCodeByAirportCode(destinationCode);

        if (originCountry is null || destinationCountry is null)
        {
            return null;
        }

        var isInternational = !originCountry.Equals(destinationCountry, StringComparison.OrdinalIgnoreCase);
        return new RouteContext(originCountry, destinationCountry, isInternational);
    }

    private static bool IsValidEmail(string value)
    {
        try
        {
            var address = new MailAddress(value);
            return address.Address.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase)
                && address.Host.Contains('.', StringComparison.Ordinal);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static bool IsValidPassportNumber(string value)
    {
        var normalized = value.Trim();
        return normalized.Length is >= BookingLimits.MinPassportLength and <= BookingLimits.MaxPassportLength
            && normalized.All(char.IsLetterOrDigit);
    }

    private static bool IsValidNationalId(string value)
    {
        var normalized = value.Trim();
        return normalized.Length is >= BookingLimits.MinNationalIdLength and <= BookingLimits.MaxNationalIdLength
            && normalized.All(char.IsDigit);
    }

    private static bool HasErrors(ValidationResultDto validation) =>
        validation.Conditions.Any(condition => condition.Severity == ValidationSeverity.Error);

    private static (ValidationResultDto ValidationResult, CreateBookingResponse? Response) Fail(
        ValidationResultDto validation,
        int statusCode,
        string? message = null)
    {
        if (message is not null)
        {
            validation.Conditions.Add(CreateError(message));
        }

        validation.StatusCode = statusCode;
        return (validation, null);
    }

    private static ValidationDto CreateError(string message) =>
        new()
        {
            Severity = ValidationSeverity.Error,
            Message = message
        };

    private static string GenerateBookingReference()
    {
        var suffix = Guid.NewGuid().ToString("N")[..BookingReferenceSuffixLength].ToUpperInvariant();
        return $"SKY-{suffix}";
    }

    private static class BookingLimits
    {
        public const int MinPassengerCount = 1;
        public const int MaxPassengerCount = 9;
        public const int MinPassportLength = 6;
        public const int MaxPassportLength = 9;
        public const int MinNationalIdLength = 6;
        public const int MaxNationalIdLength = 16;
    }

    private sealed record RouteContext(
        string OriginCountryCode,
        string DestinationCountryCode,
        bool IsInternational);
}
