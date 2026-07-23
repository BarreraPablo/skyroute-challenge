using SkyRoute.Core.Pricing;

namespace SkyRoute.Core.Tests.Pricing;

[TestFixture]
public class ArcticAirPricingServiceTests
{
    private ArcticAirPricingService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new ArcticAirPricingService();
    }

    [Test]
    public void Calculate_WithStandardBaseFare_AppliesTwentyPercentMarkupThenLoyaltyDiscount()
    {
        // Arrange
        const decimal baseFare = 100m;
        const int numberOfPassengers = 1;

        // Act
        var result = _sut.Calculate(baseFare, numberOfPassengers);

        // Assert
        Assert.That(result.PricePerPassenger, Is.EqualTo(110m));
        Assert.That(result.TotalPrice, Is.EqualTo(110m));
    }

    [Test]
    public void Calculate_WithMultiplePassengers_MultipliesAdjustedPricePerPassenger()
    {
        // Arrange
        const decimal baseFare = 100m;
        const int numberOfPassengers = 4;

        // Act
        var result = _sut.Calculate(baseFare, numberOfPassengers);

        // Assert
        Assert.That(result.PricePerPassenger, Is.EqualTo(110m));
        Assert.That(result.TotalPrice, Is.EqualTo(440m));
    }

    [Test]
    public void Calculate_WhenAdjustedFareIsBelowMinimum_EnforcesMinimumPricePerPassenger()
    {
        // Arrange
        const decimal baseFare = 40m;
        const int numberOfPassengers = 2;

        // Act
        var result = _sut.Calculate(baseFare, numberOfPassengers);

        // Assert
        Assert.That(result.PricePerPassenger, Is.EqualTo(49.99m));
        Assert.That(result.TotalPrice, Is.EqualTo(99.98m));
    }

    [Test]
    public void Calculate_WhenAdjustedFareEqualsMinimum_UsesMinimumPricePerPassenger()
    {
        // Arrange
        const decimal baseFare = 49.991666666666666666666666667m;
        const int numberOfPassengers = 1;

        // Act
        var result = _sut.Calculate(baseFare, numberOfPassengers);

        // Assert
        Assert.That(result.PricePerPassenger, Is.EqualTo(49.99m));
        Assert.That(result.TotalPrice, Is.EqualTo(49.99m));
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
        Assert.That(result.PricePerPassenger, Is.EqualTo(94.80m));
        Assert.That(result.TotalPrice, Is.EqualTo(189.60m));
    }
}
