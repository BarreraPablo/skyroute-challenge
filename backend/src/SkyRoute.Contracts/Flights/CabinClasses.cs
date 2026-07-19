namespace SkyRoute.Contracts.Flights;

public static class CabinClasses
{
    public const string Economy = "Economy";
    public const string Business = "Business";
    public const string FirstClass = "First Class";

    public static readonly IReadOnlyList<string> All = [Economy, Business, FirstClass];
}
