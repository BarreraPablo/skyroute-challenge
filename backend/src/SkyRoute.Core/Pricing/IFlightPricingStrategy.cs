namespace SkyRoute.Core.Pricing;

public interface IFlightPricingStrategy
{
    PricingResult Calculate(decimal baseFare, int numberOfPassengers);
}
