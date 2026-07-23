namespace SkyRoute.Core.Pricing;

public class ArcticAirPricingService : IArcticAirPricingService
{
    private const decimal MarkupRate = 1.20m;
    private const decimal LoyaltyDiscount = 10m;
    private const decimal MinimumPrice = 49.99m;

    public PricingResult Calculate(decimal baseFare, int numberOfPassengers)
    {
        var adjustedFare = baseFare * MarkupRate - LoyaltyDiscount;
        var pricePerPassenger = Math.Max(MinimumPrice, Math.Round(adjustedFare, 2));
        var totalPrice = Math.Round(pricePerPassenger * numberOfPassengers, 2);

        return new PricingResult(pricePerPassenger, totalPrice);
    }
}
