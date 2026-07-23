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

    private readonly IReadOnlyDictionary<string, IFlightProviderExternalServiceStrategy> _flightProviders;
    private readonly IBookingRepository _bookingRepository;
    private readonly IAirportReferenceService _airportReferenceService;
    private readonly ICreateBookingValidationService _validationService;

    public CreateBookingService(
        IEnumerable<IFlightProviderExternalServiceStrategy> flightProviders,
        IBookingRepository bookingRepository,
        IAirportReferenceService airportReferenceService,
        ICreateBookingValidationService validationService)
    {
        _flightProviders = flightProviders
            .Where(provider => !string.IsNullOrWhiteSpace(provider.ProviderName))
            .GroupBy(provider => provider.ProviderName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
        _bookingRepository = bookingRepository;
        _airportReferenceService = airportReferenceService;
        _validationService = validationService;
    }

    public async Task<(ValidationResultDto ValidationResult, CreateBookingResponse? Response)> CreateAsync(
        CreateBookingRequest request,
        CancellationToken cancellationToken)
    {
        var validation = _validationService.ValidateRequest(request);
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

    private IFlightProviderExternalServiceStrategy? ResolveProvider(string providerName) =>
        string.IsNullOrWhiteSpace(providerName)
            ? null
            : _flightProviders.TryGetValue(providerName, out var provider)
                ? provider
                : null;

    private async Task<FlightResponse?> GetCurrentFlightAsync(
        IFlightProviderExternalServiceStrategy provider,
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

    private sealed record RouteContext(
        string OriginCountryCode,
        string DestinationCountryCode,
        bool IsInternational);
}
