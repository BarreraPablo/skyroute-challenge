namespace SkyRoute.Core.Pricing;

public class BudgetWingsPricingStrategy : IBudgetWingsPricingStrategy
{
    private const decimal PromotionalDiscountRate = 0.10m;
    private const decimal MinimumPrice = 29.99m;

    public PricingResult Calculate(decimal baseFare, int numberOfPassengers)
    {
        var discountedBaseFare = baseFare * (1 - PromotionalDiscountRate);
        var pricePerPassenger = Math.Max(MinimumPrice, Math.Round(discountedBaseFare, 2));
        var totalPrice = Math.Round(pricePerPassenger * numberOfPassengers, 2);

        return new PricingResult(pricePerPassenger, totalPrice);
    }
}
