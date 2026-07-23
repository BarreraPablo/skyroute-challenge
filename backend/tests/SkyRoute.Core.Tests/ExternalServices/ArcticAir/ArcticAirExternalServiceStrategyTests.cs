using Moq;
using SkyRoute.Core.ExternalServices.ArcticAir;
using SkyRoute.Core.ExternalServices.ArcticAir.Dtos;
using SkyRoute.Core.Models;
using SkyRoute.Core.Pricing;

namespace SkyRoute.Core.Tests.ExternalServices.ArcticAir;

[TestFixture]
public class ArcticAirExternalServiceStrategyTests
{
    private Mock<IArcticAirProxy> _proxyMock = null!;
    private Mock<IArcticAirPricingService> _pricingServiceMock = null!;
    private ArcticAirExternalServiceStrategy _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _proxyMock = new Mock<IArcticAirProxy>();
        _pricingServiceMock = new Mock<IArcticAirPricingService>();
        _sut = new ArcticAirExternalServiceStrategy(_proxyMock.Object, _pricingServiceMock.Object);
    }

    [Test]
    public void ProviderName_ReturnsArcticAir()
    {
        // Assert
        Assert.That(_sut.ProviderName, Is.EqualTo("ArcticAir"));
    }

    [Test]
    public async Task SearchFlightsAsync_WithOffers_MapsAndPricesEachFlight()
    {
        // Arrange
        var request = CreateSearchRequest();
        var offer = CreateOffer();
        var pricing = new PricingResult(90m, 180m);

        _proxyMock
            .Setup(proxy => proxy.SearchFlightsAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ArcticAirSearchResponse { Flights = [offer] });

        _pricingServiceMock
            .Setup(service => service.Calculate(offer.UnitPrice, request.NumberOfPassengers))
            .Returns(pricing);

        // Act
        var results = await _sut.SearchFlightsAsync(request, CancellationToken.None);

        // Assert
        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].ProviderName, Is.EqualTo("ArcticAir"));
        Assert.That(results[0].FlightId, Is.EqualTo(offer.FlightGuid.ToString()));
        Assert.That(results[0].Origin, Is.EqualTo("JFK"));
        Assert.That(results[0].Destination, Is.EqualTo("MAD"));
        Assert.That(results[0].PricePerPassenger, Is.EqualTo(90m));
        Assert.That(results[0].TotalPrice, Is.EqualTo(180m));

        _proxyMock.Verify(
            proxy => proxy.SearchFlightsAsync(request, It.IsAny<CancellationToken>()),
            Times.Once);
        _pricingServiceMock.Verify(
            service => service.Calculate(offer.UnitPrice, request.NumberOfPassengers),
            Times.Once);
    }

    [Test]
    public async Task GetFlightByIdAsync_WhenOfferExists_ReturnsMappedFlight()
    {
        // Arrange
        var request = CreateSearchRequest();
        var offer = CreateOffer();
        var pricing = new PricingResult(90m, 180m);
        var flightId = offer.FlightGuid.ToString();

        _proxyMock
            .Setup(proxy => proxy.GetFlightByIdAsync(flightId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(offer);

        _pricingServiceMock
            .Setup(service => service.Calculate(offer.UnitPrice, request.NumberOfPassengers))
            .Returns(pricing);

        // Act
        var result = await _sut.GetFlightByIdAsync(flightId, request, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.FlightId, Is.EqualTo(flightId));
        Assert.That(result.CabinClass, Is.EqualTo("Economy"));
        Assert.That(result.TotalPrice, Is.EqualTo(180m));

        _proxyMock.Verify(
            proxy => proxy.GetFlightByIdAsync(flightId, request, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetFlightByIdAsync_WhenOfferNotFound_ReturnsNull()
    {
        // Arrange
        var request = CreateSearchRequest();

        _proxyMock
            .Setup(proxy => proxy.GetFlightByIdAsync("missing", request, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ArcticAirOffer?)null);

        // Act
        var result = await _sut.GetFlightByIdAsync("missing", request, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Null);
        _pricingServiceMock.Verify(
            service => service.Calculate(It.IsAny<decimal>(), It.IsAny<int>()),
            Times.Never);
    }

    private static SearchFlightRequest CreateSearchRequest() =>
        new()
        {
            Origin = "JFK",
            Destination = "MAD",
            DepartureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            NumberOfPassengers = 2,
            CabinClass = "Economy"
        };

    private static ArcticAirOffer CreateOffer() =>
        new()
        {
            FlightGuid = 25001,
            FromCode = "JFK",
            FromCountry = "US",
            ToCode = "MAD",
            ToCountry = "ES",
            DepartureTime = new DateTime(2026, 8, 20, 18, 0, 0, DateTimeKind.Utc),
            ArrivalTime = new DateTime(2026, 8, 21, 3, 0, 0, DateTimeKind.Utc),
            ClassType = "Economy",
            UnitPrice = 100m,
            NumberOfPassengersAvailable = 9
        };
}
