using SkyRoute.Contracts.Bookings;
using SkyRoute.Contracts.Flights;
using SkyRoute.Contracts.Validation;
using SkyRoute.Core.Entities;
using SkyRoute.Core.Enums;
using SkyRoute.Core.ExternalServices;
using SkyRoute.Core.Interfaces;
using SkyRoute.Core.Models;

namespace SkyRoute.Core.Services;

public class CreateBookingService : ICreateBookingService
{
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

        if (validation.Conditions.Any(condition => condition.Severity == ValidationSeverity.Error))
        {
            validation.StatusCode = 400;
            return (validation, null);
        }

        var provider = _flightProviders.FirstOrDefault(item =>
            item.ProviderName.Equals(request.Provider, StringComparison.OrdinalIgnoreCase));

        if (provider is null)
        {
            validation.Conditions.Add(new ValidationDto
            {
                Severity = ValidationSeverity.Error,
                Message = $"Unknown provider: {request.Provider}."
            });

            validation.StatusCode = 400;
            return (validation, null);
        }

        var lookupRequest = new SearchFlightRequest
        {
            Origin = request.OriginCode,
            Destination = request.DestinationCode,
            DepartureDate = DateOnly.FromDateTime(request.DepartureTimeUtc),
            NumberOfPassengers = request.PassengerCount,
            CabinClass = request.CabinClass
        };

        var providerFlight = await provider.GetFlightByIdAsync(request.FlightId, lookupRequest, cancellationToken);

        if (providerFlight is null || !IsSameFlight(providerFlight, request))
        {
            validation.Conditions.Add(new ValidationDto
            {
                Severity = ValidationSeverity.Error,
                Message = "Selected flight data is stale. Please search again and reselect your flight."
            });

            validation.StatusCode = 409;
            return (validation, null);
        }

        var originCountryCode = _airportReferenceService.GetCountryCodeByAirportCode(request.OriginCode) ?? string.Empty;
        var destinationCountryCode = _airportReferenceService.GetCountryCodeByAirportCode(request.DestinationCode) ?? string.Empty;
        var isInternational = !originCountryCode.Equals(destinationCountryCode, StringComparison.OrdinalIgnoreCase);

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            BookingReference = GenerateBookingReference(),
            CreatedAtUtc = DateTime.UtcNow,
            FlightId = request.FlightId,
            Provider = request.Provider,
            Origin = request.OriginCode,
            Destination = request.DestinationCode,
            OriginCountry = originCountryCode,
            DestinationCountry = destinationCountryCode,
            IsInternational = isInternational,
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
                    DocumentType = isInternational ? DocumentType.Passport : DocumentType.NationalId
                }
            ]
        };

        await _bookingRepository.AddAsync(booking, cancellationToken);

        validation.StatusCode = 200;
        return (validation, new CreateBookingResponse(booking.BookingReference));
    }

    private static bool IsSameFlight(FlightResponse providerFlight, CreateBookingRequest request)
    {
        const decimal epsilon = 0.01m;

        return providerFlight.FlightId.Equals(request.FlightId, StringComparison.OrdinalIgnoreCase)
            && providerFlight.ProviderName.Equals(request.Provider, StringComparison.OrdinalIgnoreCase)
            && providerFlight.Origin.Equals(request.OriginCode, StringComparison.OrdinalIgnoreCase)
            && providerFlight.Destination.Equals(request.DestinationCode, StringComparison.OrdinalIgnoreCase)
            && providerFlight.DepartureTimeUtc == request.DepartureTimeUtc
            && providerFlight.ArrivalTimeUtc == request.ArrivalTimeUtc
            && providerFlight.CabinClass.Equals(request.CabinClass, StringComparison.OrdinalIgnoreCase)
            && Math.Abs(providerFlight.PricePerPassenger - request.PricePerPassenger) <= epsilon
            && Math.Abs(providerFlight.TotalPrice - request.ExpectedPrice) <= epsilon;
    }

    private ValidationResultDto ValidateRequest(CreateBookingRequest request)
    {
        var validation = new ValidationResultDto();

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

        if (request.PassengerCount < 1 || request.PassengerCount > 9)
        {
            validation.Conditions.Add(CreateError("Passenger count must be between 1 and 9."));
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

        if (request.ArrivalTimeUtc <= request.DepartureTimeUtc)
        {
            validation.Conditions.Add(CreateError("Arrival time must be after departure time."));
        }

        if (request.Passenger is null)
        {
            validation.Conditions.Add(CreateError("Passenger details are required."));
            return validation;
        }

        if (string.IsNullOrWhiteSpace(request.Passenger.FullName))
        {
            validation.Conditions.Add(CreateError("Passenger full name is required."));
        }

        if (string.IsNullOrWhiteSpace(request.Passenger.Email))
        {
            validation.Conditions.Add(CreateError("Passenger email is required."));
        }

        if (!string.IsNullOrWhiteSpace(request.Passenger.Email)
            && !request.Passenger.Email.Contains('@', StringComparison.Ordinal))
        {
            validation.Conditions.Add(CreateError("Passenger email is invalid."));
        }

        if (string.IsNullOrWhiteSpace(request.Passenger.DocumentNumber))
        {
            validation.Conditions.Add(CreateError("Passenger document number is required."));
        }

        var originCountryCode = _airportReferenceService.GetCountryCodeByAirportCode(request.OriginCode);
        var destinationCountryCode = _airportReferenceService.GetCountryCodeByAirportCode(request.DestinationCode);
        var isInternational = originCountryCode is not null
            && destinationCountryCode is not null
            && !originCountryCode.Equals(destinationCountryCode, StringComparison.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(request.Passenger.DocumentNumber))
        {
            if (isInternational)
            {
                if (!IsValidPassportNumber(request.Passenger.DocumentNumber))
                {
                    validation.Conditions.Add(CreateError("Passport Number is invalid for international flights."));
                }
            }
            else
            {
                if (!IsValidNationalId(request.Passenger.DocumentNumber))
                {
                    validation.Conditions.Add(CreateError("National ID is invalid for domestic flights."));
                }
            }
        }

        return validation;
    }

    private static bool IsValidPassportNumber(string value)
    {
        var normalized = value.Trim();
        return normalized.Length is >= 6 and <= 9
            && normalized.All(character => char.IsLetterOrDigit(character));
    }

    private static bool IsValidNationalId(string value)
    {
        var normalized = value.Trim();
        return normalized.Length is >= 6 and <= 16
            && normalized.All(char.IsDigit);
    }

    private static ValidationDto CreateError(string message) =>
        new()
        {
            Severity = ValidationSeverity.Error,
            Message = message
        };

    private static string GenerateBookingReference()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        return $"SKY-{suffix}";
    }
}
