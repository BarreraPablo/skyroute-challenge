using Moq;
using SkyRoute.Core.ExternalServices.GlobalAir;
using SkyRoute.Core.ExternalServices.GlobalAir.Dtos;
using SkyRoute.Core.Models;
using SkyRoute.Core.Pricing;

namespace SkyRoute.Core.Tests.ExternalServices.GlobalAir;

[TestFixture]
public class GlobalAirServiceTests
{
    private Mock<IGlobalAirProxy> _proxyMock = null!;
    private Mock<IGlobalAirPricingStrategy> _pricingStrategyMock = null!;
    private GlobalAirService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _proxyMock = new Mock<IGlobalAirProxy>();
        _pricingStrategyMock = new Mock<IGlobalAirPricingStrategy>();
        _sut = new GlobalAirService(_proxyMock.Object, _pricingStrategyMock.Object);
    }

    [Test]
    public void ProviderName_ReturnsGlobalAir()
    {
        // Assert
        Assert.That(_sut.ProviderName, Is.EqualTo("GlobalAir"));
    }

    [Test]
    public async Task SearchFlightsAsync_WithLegs_MapsAndPricesEachFlight()
    {
        // Arrange
        var request = CreateSearchRequest();
        var leg = CreateLeg();
        var pricing = new PricingResult(115m, 230m);

        _proxyMock
            .Setup(proxy => proxy.SearchFlightsAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GlobalAirAvailabilityResponse { Results = [leg] });

        _pricingStrategyMock
            .Setup(strategy => strategy.Calculate(leg.Fare.PricePerPax, request.NumberOfPassengers))
            .Returns(pricing);

        // Act
        var results = await _sut.SearchFlightsAsync(request, CancellationToken.None);

        // Assert
        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].ProviderName, Is.EqualTo("GlobalAir"));
        Assert.That(results[0].FlightId, Is.EqualTo("GA-123"));
        Assert.That(results[0].Origin, Is.EqualTo("JFK"));
        Assert.That(results[0].Destination, Is.EqualTo("MAD"));
        Assert.That(results[0].PricePerPassenger, Is.EqualTo(115m));
        Assert.That(results[0].TotalPrice, Is.EqualTo(230m));

        _proxyMock.Verify(
            proxy => proxy.SearchFlightsAsync(request, It.IsAny<CancellationToken>()),
            Times.Once);
        _pricingStrategyMock.Verify(
            strategy => strategy.Calculate(leg.Fare.PricePerPax, request.NumberOfPassengers),
            Times.Once);
    }

    [Test]
    public async Task GetFlightByIdAsync_WhenLegExists_ReturnsMappedFlight()
    {
        // Arrange
        var request = CreateSearchRequest();
        var leg = CreateLeg();
        var pricing = new PricingResult(115m, 230m);
        const string flightId = "GA-123";

        _proxyMock
            .Setup(proxy => proxy.GetFlightByIdAsync(flightId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(leg);

        _pricingStrategyMock
            .Setup(strategy => strategy.Calculate(leg.Fare.PricePerPax, request.NumberOfPassengers))
            .Returns(pricing);

        // Act
        var result = await _sut.GetFlightByIdAsync(flightId, request, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.FlightId, Is.EqualTo(flightId));
        Assert.That(result.CabinClass, Is.EqualTo("Business"));
        Assert.That(result.TotalPrice, Is.EqualTo(230m));

        _proxyMock.Verify(
            proxy => proxy.GetFlightByIdAsync(flightId, request, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetFlightByIdAsync_WhenLegNotFound_ReturnsNull()
    {
        // Arrange
        var request = CreateSearchRequest();

        _proxyMock
            .Setup(proxy => proxy.GetFlightByIdAsync("missing", request, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GlobalAirLeg?)null);

        // Act
        var result = await _sut.GetFlightByIdAsync("missing", request, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Null);
        _pricingStrategyMock.Verify(
            strategy => strategy.Calculate(It.IsAny<decimal>(), It.IsAny<int>()),
            Times.Never);
    }

    private static SearchFlightRequest CreateSearchRequest() =>
        new()
        {
            Origin = "JFK",
            Destination = "MAD",
            DepartureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            NumberOfPassengers = 2,
            CabinClass = "Business"
        };

    private static GlobalAirLeg CreateLeg() =>
        new()
        {
            Id = "GA-123",
            Departure = new GlobalAirLocation { Airport = "JFK", Country = "US" },
            Arrival = new GlobalAirLocation { Airport = "MAD", Country = "ES" },
            Schedule = new GlobalAirSchedule
            {
                DepartUtc = new DateTime(2026, 8, 20, 18, 0, 0, DateTimeKind.Utc),
                ArriveUtc = new DateTime(2026, 8, 21, 3, 0, 0, DateTimeKind.Utc)
            },
            Fare = new GlobalAirFare { Cabin = "Business", PricePerPax = 100m },
            Inventory = new GlobalAirInventory { NumberOfPassengersAvailable = 9 }
        };
}
