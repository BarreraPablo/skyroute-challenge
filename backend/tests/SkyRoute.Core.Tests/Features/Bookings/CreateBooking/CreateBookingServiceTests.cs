using Moq;
using SkyRoute.Core.Entities;
using SkyRoute.Core.Enums;
using SkyRoute.Core.ExternalServices;
using SkyRoute.Core.Features.Bookings.CreateBooking;
using SkyRoute.Core.Interfaces;
using SkyRoute.Core.Models;
using SkyRoute.Core.Models.Validation;
using SkyRoute.Core.Services;

namespace SkyRoute.Core.Tests.Features.Bookings.CreateBooking;

[TestFixture]
public class CreateBookingServiceTests
{
    private Mock<IFlightProviderExternalService> _providerMock = null!;
    private Mock<IBookingRepository> _bookingRepositoryMock = null!;
    private AirportReferenceService _airportReferenceService = null!;
    private CreateBookingService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _providerMock = new Mock<IFlightProviderExternalService>();
        _providerMock.Setup(provider => provider.ProviderName).Returns("BudgetWings");
        _bookingRepositoryMock = new Mock<IBookingRepository>();
        _airportReferenceService = new AirportReferenceService();

        _sut = new CreateBookingService(
            [_providerMock.Object],
            _bookingRepositoryMock.Object,
            _airportReferenceService);
    }

    [Test]
    public async Task CreateAsync_WhenSnapshotMatches_ReturnsSuccessAndPersistsBooking()
    {
        // Arrange
        var request = BuildValidRequest();
        var flight = CreateMatchingFlight();

        _providerMock
            .Setup(provider => provider.GetFlightByIdAsync(
                request.FlightId,
                It.IsAny<SearchFlightRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(flight);

        // Act
        var (validationResult, response) = await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        Assert.That(validationResult.StatusCode, Is.EqualTo(200));
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.BookingReference, Does.StartWith("SKY-"));

        _bookingRepositoryMock.Verify(
            repository => repository.AddAsync(
                It.Is<Booking>(booking =>
                    booking.OriginCountry == "US"
                    && booking.DestinationCountry == "ES"
                    && booking.IsInternational
                    && booking.TotalPrice == request.ExpectedPrice
                    && booking.Passengers.Any(passenger =>
                        passenger.DocumentType == DocumentType.Passport)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task CreateAsync_WhenProviderDataDiffers_ReturnsConflict()
    {
        // Arrange
        var request = BuildValidRequest();
        var flight = CreateMatchingFlight();
        flight.TotalPrice += 10;

        _providerMock
            .Setup(provider => provider.GetFlightByIdAsync(
                request.FlightId,
                It.IsAny<SearchFlightRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(flight);

        // Act
        var (validationResult, response) = await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        Assert.That(validationResult.StatusCode, Is.EqualTo(409));
        Assert.That(response, Is.Null);
        Assert.That(
            validationResult.Conditions,
            Has.Some.Matches<ValidationDto>(item =>
                item.Message.Contains("stale", StringComparison.OrdinalIgnoreCase)));

        _bookingRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task CreateAsync_WhenUnknownProvider_ReturnsValidationError()
    {
        // Arrange
        var request = BuildValidRequest() with { Provider = "UnknownAir" };

        // Act
        var (validationResult, response) = await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        Assert.That(validationResult.StatusCode, Is.EqualTo(400));
        Assert.That(response, Is.Null);
        Assert.That(
            validationResult.Conditions,
            Has.Some.Matches<ValidationDto>(item =>
                item.Message == "Unknown provider: UnknownAir."));

        _providerMock.Verify(
            provider => provider.GetFlightByIdAsync(
                It.IsAny<string>(),
                It.IsAny<SearchFlightRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task CreateAsync_WhenDomesticDocumentIsNotNationalId_ReturnsValidationError()
    {
        // Arrange
        var request = BuildValidRequest() with
        {
            DestinationCode = "LAX",
            Passenger = new CreateBookingPassengerRequest("John Doe", "john.doe@email.com", "AB123456")
        };

        // Act
        var (validationResult, response) = await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        Assert.That(validationResult.StatusCode, Is.EqualTo(400));
        Assert.That(response, Is.Null);
        Assert.That(
            validationResult.Conditions,
            Has.Some.Matches<ValidationDto>(item =>
                item.Message.Contains("National ID", StringComparison.OrdinalIgnoreCase)));

        _bookingRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task CreateAsync_WhenInternationalPassportIsInvalid_ReturnsValidationError()
    {
        // Arrange
        var request = BuildValidRequest() with
        {
            Passenger = new CreateBookingPassengerRequest("John Doe", "john.doe@email.com", "123")
        };

        // Act
        var (validationResult, response) = await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        Assert.That(validationResult.StatusCode, Is.EqualTo(400));
        Assert.That(response, Is.Null);
        Assert.That(
            validationResult.Conditions,
            Has.Some.Matches<ValidationDto>(item =>
                item.Message.Contains("Passport Number is invalid", StringComparison.OrdinalIgnoreCase)));
    }

    [Test]
    public async Task CreateAsync_WhenCabinClassIsInvalid_ReturnsValidationError()
    {
        // Arrange
        var request = BuildValidRequest() with { CabinClass = "Premium Economy" };

        // Act
        var (validationResult, response) = await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        Assert.That(validationResult.StatusCode, Is.EqualTo(400));
        Assert.That(response, Is.Null);
        Assert.That(
            validationResult.Conditions,
            Has.Some.Matches<ValidationDto>(item =>
                item.Message.Contains("Cabin class must be one of", StringComparison.OrdinalIgnoreCase)));
    }

    [Test]
    public async Task CreateAsync_WhenMissingFlightId_ReturnsValidationError()
    {
        // Arrange
        var request = BuildValidRequest() with { FlightId = "  " };

        // Act
        var (validationResult, response) = await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        Assert.That(validationResult.StatusCode, Is.EqualTo(400));
        Assert.That(response, Is.Null);
        Assert.That(
            validationResult.Conditions,
            Has.Some.Matches<ValidationDto>(item => item.Message == "FlightId is required."));
    }

    [Test]
    public async Task CreateAsync_WhenDomesticFlight_UsesNationalIdDocumentType()
    {
        // Arrange
        var request = BuildValidRequest() with
        {
            DestinationCode = "LAX",
            Passenger = new CreateBookingPassengerRequest("John Doe", "john.doe@email.com", "1234567890")
        };
        var flight = CreateMatchingFlight();
        flight.Destination = "LAX";
        flight.DestinationCountry = "US";

        _providerMock
            .Setup(provider => provider.GetFlightByIdAsync(
                request.FlightId,
                It.IsAny<SearchFlightRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(flight);

        // Act
        var (validationResult, response) = await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        Assert.That(validationResult.StatusCode, Is.EqualTo(200));
        Assert.That(response, Is.Not.Null);

        _bookingRepositoryMock.Verify(
            repository => repository.AddAsync(
                It.Is<Booking>(booking =>
                    !booking.IsInternational
                    && booking.Passengers.Any(passenger =>
                        passenger.DocumentType == DocumentType.NationalId)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static CreateBookingRequest BuildValidRequest() =>
        new(
            FlightId: "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
            Provider: "BudgetWings",
            OriginCode: "JFK",
            DestinationCode: "MAD",
            DepartureTimeUtc: new DateTime(2026, 8, 20, 18, 0, 0, DateTimeKind.Utc),
            ArrivalTimeUtc: new DateTime(2026, 8, 20, 18, 0, 0, DateTimeKind.Utc).AddHours(9),
            CabinClass: "Economy",
            PricePerPassenger: 262.26m,
            PassengerCount: 4,
            ExpectedPrice: 1049.04m,
            Passenger: new CreateBookingPassengerRequest("John Doe", "john.doe@email.com", "A1234567"));

    private static FlightResponse CreateMatchingFlight() =>
        new()
        {
            ProviderName = "BudgetWings",
            FlightId = "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
            Origin = "JFK",
            Destination = "MAD",
            OriginCountry = "US",
            DestinationCountry = "ES",
            DepartureTimeUtc = new DateTime(2026, 8, 20, 18, 0, 0, DateTimeKind.Utc),
            ArrivalTimeUtc = new DateTime(2026, 8, 20, 18, 0, 0, DateTimeKind.Utc).AddHours(9),
            CabinClass = "Economy",
            PricePerPassenger = 262.26m,
            TotalPrice = 1049.04m
        };
}
