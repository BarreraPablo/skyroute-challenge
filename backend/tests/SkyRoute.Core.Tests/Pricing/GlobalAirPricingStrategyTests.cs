using SkyRoute.Core.Pricing;

namespace SkyRoute.Core.Tests.Pricing;

[TestFixture]
public class GlobalAirPricingStrategyTests
{
    private GlobalAirPricingStrategy _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new GlobalAirPricingStrategy();
    }

    [Test]
    public void Calculate_WithStandardBaseFare_AppliesFifteenPercentFuelSurcharge()
    {
        // Arrange
        const decimal baseFare = 100m;
        const int numberOfPassengers = 1;

        // Act
        var result = _sut.Calculate(baseFare, numberOfPassengers);

        // Assert
        Assert.That(result.PricePerPassenger, Is.EqualTo(115m));
        Assert.That(result.TotalPrice, Is.EqualTo(115m));
    }

    [Test]
    public void Calculate_WithMultiplePassengers_MultipliesSurchargedPricePerPassenger()
    {
        // Arrange
        const decimal baseFare = 100m;
        const int numberOfPassengers = 4;

        // Act
        var result = _sut.Calculate(baseFare, numberOfPassengers);

        // Assert
        Assert.That(result.PricePerPassenger, Is.EqualTo(115m));
        Assert.That(result.TotalPrice, Is.EqualTo(460m));
    }

    [Test]
    public void Calculate_WithFractionalBaseFare_RoundsPricePerPassengerToTwoDecimals()
    {
        // Arrange
        const decimal baseFare = 87.33m;
        const int numberOfPassengers = 2;

        // Act
        var result = _sut.Calculate(baseFare, numberOfPassengers);

        // Assert
        Assert.That(result.PricePerPassenger, Is.EqualTo(100.43m));
        Assert.That(result.TotalPrice, Is.EqualTo(200.86m));
    }

    [Test]
    public void Calculate_WithSinglePassengerAndZeroBaseFare_ReturnsZeroPrices()
    {
        // Arrange
        const decimal baseFare = 0m;
        const int numberOfPassengers = 1;

        // Act
        var result = _sut.Calculate(baseFare, numberOfPassengers);

        // Assert
        Assert.That(result.PricePerPassenger, Is.EqualTo(0m));
        Assert.That(result.TotalPrice, Is.EqualTo(0m));
    }

    [Test]
    public void Calculate_WithLargePassengerCount_ComputesTotalPriceCorrectly()
    {
        // Arrange
        const decimal baseFare = 200m;
        const int numberOfPassengers = 10;

        // Act
        var result = _sut.Calculate(baseFare, numberOfPassengers);

        // Assert
        Assert.That(result.PricePerPassenger, Is.EqualTo(230m));
        Assert.That(result.TotalPrice, Is.EqualTo(2300m));
    }
}
