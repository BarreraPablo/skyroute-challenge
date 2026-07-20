using Moq;
using SkyRoute.Core.ExternalServices;
using SkyRoute.Core.Features.Flights.SearchFlights;
using SkyRoute.Core.Models;
using SkyRoute.Core.Models.Validation;
using SkyRoute.Core.Services;

namespace SkyRoute.Core.Tests.Features.Flights.SearchFlights;

[TestFixture]
public class SearchFlightServiceTests
{
    private Mock<IFlightProviderExternalService> _providerOneMock = null!;
    private Mock<IFlightProviderExternalService> _providerTwoMock = null!;
    private Mock<IAirportReferenceService> _airportReferenceServiceMock = null!;
    private SearchFlightService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _providerOneMock = new Mock<IFlightProviderExternalService>();
        _providerTwoMock = new Mock<IFlightProviderExternalService>();
        _airportReferenceServiceMock = new Mock<IAirportReferenceService>();

        _airportReferenceServiceMock
            .Setup(service => service.IsValidAirportCode(It.IsAny<string>()))
            .Returns(true);

        _sut = new SearchFlightService(
            [_providerOneMock.Object, _providerTwoMock.Object],
            _airportReferenceServiceMock.Object);
    }

    [Test]
    public async Task SearchAsync_ValidRequest_ReturnsFlightsSortedByTotalPrice()
    {
        // Arrange
        var request = CreateValidRequest();
        var expensiveFlight = CreateFlight("BudgetWings", "flight-1", 500m);
        var cheaperFlight = CreateFlight("GlobalAir", "flight-2", 300m);

        _providerOneMock
            .Setup(provider => provider.SearchFlightsAsync(It.IsAny<SearchFlightRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([expensiveFlight]);

        _providerTwoMock
            .Setup(provider => provider.SearchFlightsAsync(It.IsAny<SearchFlightRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([cheaperFlight]);

        // Act
        var (validationResult, response) = await _sut.SearchAsync(request, CancellationToken.None);

        // Assert
        Assert.That(validationResult.Conditions, Is.Empty);
        Assert.That(response.Flights, Has.Count.EqualTo(2));
        Assert.That(response.Flights[0].TotalPrice, Is.EqualTo(300m));
        Assert.That(response.Flights[1].TotalPrice, Is.EqualTo(500m));
        Assert.That(response.Flights[0].ProviderName, Is.EqualTo("GlobalAir"));

        _providerOneMock.Verify(
            provider => provider.SearchFlightsAsync(It.IsAny<SearchFlightRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _providerTwoMock.Verify(
            provider => provider.SearchFlightsAsync(It.IsAny<SearchFlightRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task SearchAsync_NullRequest_ReturnsValidationErrorAndEmptyFlights()
    {
        // Act
        var (validationResult, response) = await _sut.SearchAsync(null!, CancellationToken.None);

        // Assert
        Assert.That(
            validationResult.Conditions,
            Has.Some.Matches<ValidationDto>(condition => condition.Message == "Request is required."));
        Assert.That(response.Flights, Is.Empty);

        _providerOneMock.Verify(
            provider => provider.SearchFlightsAsync(It.IsAny<SearchFlightRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task SearchAsync_MissingOrigin_ReturnsValidationError()
    {
        // Arrange
        var request = CreateValidRequest() with { OriginCode = "  " };

        // Act
        var (validationResult, response) = await _sut.SearchAsync(request, CancellationToken.None);

        // Assert
        Assert.That(
            validationResult.Conditions,
            Has.Some.Matches<ValidationDto>(condition => condition.Message == "Origin is required."));
        Assert.That(response.Flights, Is.Empty);
    }

    [Test]
    public async Task SearchAsync_SameOriginAndDestination_ReturnsValidationError()
    {
        // Arrange
        var request = CreateValidRequest() with { OriginCode = "JFK", DestinationCode = "jfk" };

        // Act
        var (validationResult, response) = await _sut.SearchAsync(request, CancellationToken.None);

        // Assert
        Assert.That(
            validationResult.Conditions,
            Has.Some.Matches<ValidationDto>(condition =>
                condition.Message == "Origin and destination must be different."));
        Assert.That(response.Flights, Is.Empty);
    }

    [Test]
    public async Task SearchAsync_UnknownOriginAirport_ReturnsValidationError()
    {
        // Arrange
        var request = CreateValidRequest() with { OriginCode = "XYZ" };

        _airportReferenceServiceMock
            .Setup(service => service.IsValidAirportCode("XYZ"))
            .Returns(false);

        // Act
        var (validationResult, response) = await _sut.SearchAsync(request, CancellationToken.None);

        // Assert
        Assert.That(
            validationResult.Conditions,
            Has.Some.Matches<ValidationDto>(condition =>
                condition.Message == "Unknown origin airport code: XYZ."));
        Assert.That(response.Flights, Is.Empty);
    }

    [Test]
    public async Task SearchAsync_PastDepartureDate_ReturnsValidationError()
    {
        // Arrange
        var request = CreateValidRequest() with
        {
            DepartureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1))
        };

        // Act
        var (validationResult, response) = await _sut.SearchAsync(request, CancellationToken.None);

        // Assert
        Assert.That(
            validationResult.Conditions,
            Has.Some.Matches<ValidationDto>(condition =>
                condition.Message == "Departure date cannot be in the past."));
        Assert.That(response.Flights, Is.Empty);
    }

    [Test]
    public async Task SearchAsync_InvalidPassengerCount_ReturnsValidationError()
    {
        // Arrange
        var request = CreateValidRequest() with { NumberOfPassengers = 10 };

        // Act
        var (validationResult, response) = await _sut.SearchAsync(request, CancellationToken.None);

        // Assert
        Assert.That(
            validationResult.Conditions,
            Has.Some.Matches<ValidationDto>(condition =>
                condition.Message == "Number of passengers must be between 1 and 9."));
        Assert.That(response.Flights, Is.Empty);
    }

    [Test]
    public async Task SearchAsync_InvalidCabinClass_ReturnsValidationError()
    {
        // Arrange
        var request = CreateValidRequest() with { CabinClass = "Premium Economy" };

        // Act
        var (validationResult, response) = await _sut.SearchAsync(request, CancellationToken.None);

        // Assert
        Assert.That(
            validationResult.Conditions,
            Has.Some.Matches<ValidationDto>(condition =>
                condition.Message.Contains("Cabin class must be one of", StringComparison.OrdinalIgnoreCase)));
        Assert.That(response.Flights, Is.Empty);
    }

    private static SearchFlightsRequest CreateValidRequest() =>
        new(
            OriginCode: "JFK",
            DestinationCode: "MAD",
            DepartureDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            NumberOfPassengers: 2,
            CabinClass: "Economy");

    private static FlightResponse CreateFlight(string providerName, string flightId, decimal totalPrice) =>
        new()
        {
            ProviderName = providerName,
            FlightId = flightId,
            Origin = "JFK",
            Destination = "MAD",
            OriginCountry = "US",
            DestinationCountry = "ES",
            DepartureTimeUtc = new DateTime(2026, 8, 20, 18, 0, 0, DateTimeKind.Utc),
            ArrivalTimeUtc = new DateTime(2026, 8, 21, 3, 0, 0, DateTimeKind.Utc),
            CabinClass = "Economy",
            PricePerPassenger = totalPrice / 2,
            TotalPrice = totalPrice
        };
}
