namespace SkyRoute.Core.Pricing;

public class GlobalAirPricingService : IGlobalAirPricingService
{
    private const decimal FuelSurchargeRate = 0.15m;

    public PricingResult Calculate(decimal baseFare, int numberOfPassengers)
    {
        var pricePerPassenger = Math.Round(baseFare * (1 + FuelSurchargeRate), 2);
        var totalPrice = Math.Round(pricePerPassenger * numberOfPassengers, 2);

        return new PricingResult(pricePerPassenger, totalPrice);
    }
}
