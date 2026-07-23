using SkyRoute.Core.Pricing;

namespace SkyRoute.Core.Tests.Pricing;

[TestFixture]
public class BudgetWingsPricingServiceTests
{
    private BudgetWingsPricingService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new BudgetWingsPricingService();
    }

    [Test]
    public void Calculate_WithStandardBaseFare_AppliesTenPercentPromotionalDiscount()
    {
        // Arrange
        const decimal baseFare = 100m;
        const int numberOfPassengers = 1;

        // Act
        var result = _sut.Calculate(baseFare, numberOfPassengers);

        // Assert
        Assert.That(result.PricePerPassenger, Is.EqualTo(90m));
        Assert.That(result.TotalPrice, Is.EqualTo(90m));
    }

    [Test]
    public void Calculate_WithMultiplePassengers_MultipliesDiscountedPricePerPassenger()
    {
        // Arrange
        const decimal baseFare = 100m;
        const int numberOfPassengers = 4;

        // Act
        var result = _sut.Calculate(baseFare, numberOfPassengers);

        // Assert
        Assert.That(result.PricePerPassenger, Is.EqualTo(90m));
        Assert.That(result.TotalPrice, Is.EqualTo(360m));
    }

    [Test]
    public void Calculate_WhenDiscountedFareIsBelowMinimum_EnforcesMinimumPricePerPassenger()
    {
        // Arrange
        const decimal baseFare = 30m;
        const int numberOfPassengers = 2;

        // Act
        var result = _sut.Calculate(baseFare, numberOfPassengers);

        // Assert
        Assert.That(result.PricePerPassenger, Is.EqualTo(29.99m));
        Assert.That(result.TotalPrice, Is.EqualTo(59.98m));
    }

    [Test]
    public void Calculate_WhenDiscountedFareEqualsMinimum_UsesMinimumPricePerPassenger()
    {
        // Arrange
        const decimal baseFare = 33.32m;
        const int numberOfPassengers = 1;

        // Act
        var result = _sut.Calculate(baseFare, numberOfPassengers);

        // Assert
        Assert.That(result.PricePerPassenger, Is.EqualTo(29.99m));
        Assert.That(result.TotalPrice, Is.EqualTo(29.99m));
    }

    [Test]
    public void Calculate_WithRealWorldBaseFare_MatchesExpectedBookingPrice()
    {
        // Arrange
        const decimal baseFare = 291.40m;
        const int numberOfPassengers = 4;

        // Act
        var result = _sut.Calculate(baseFare, numberOfPassengers);

        // Assert
        Assert.That(result.PricePerPassenger, Is.EqualTo(262.26m));
        Assert.That(result.TotalPrice, Is.EqualTo(1049.04m));
    }

    [Test]
    public void Calculate_WithFractionalBaseFare_RoundsPricePerPassengerToTwoDecimals()
    {
        // Arrange
        const decimal baseFare = 111.11m;
        const int numberOfPassengers = 3;

        // Act
        var result = _sut.Calculate(baseFare, numberOfPassengers);

        // Assert
        Assert.That(result.PricePerPassenger, Is.EqualTo(100m));
        Assert.That(result.TotalPrice, Is.EqualTo(300m));
    }
}
