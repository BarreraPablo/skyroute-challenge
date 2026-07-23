namespace SkyRoute.Core.Pricing;

public interface IFlightPricingService
{
    PricingResult Calculate(decimal baseFare, int numberOfPassengers);
}
