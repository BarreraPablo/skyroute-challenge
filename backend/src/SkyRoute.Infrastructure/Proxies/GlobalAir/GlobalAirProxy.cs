using SkyRoute.Core.ExternalServices.GlobalAir;
using SkyRoute.Core.ExternalServices.GlobalAir.Dtos;
using SkyRoute.Core.Models;

namespace SkyRoute.Infrastructure.Proxies.GlobalAir;

public class GlobalAirProxy : IGlobalAirProxy
{
    public async Task<GlobalAirAvailabilityResponse> SearchFlightsAsync(
        SearchFlightRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await Task.Delay(TimeSpan.FromMilliseconds(50), cancellationToken);

        var departureDate = request.DepartureDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var allResults = new List<GlobalAirLeg>
        {
            CreateLeg("100234", request, departureDate, "Economy", 349.99m, 9),
            CreateLeg("100235", request, departureDate, "Business", 899.50m, 4)
        };

        var results = allResults
            .Where(leg => leg.Fare.Cabin.Equals(request.CabinClass, StringComparison.OrdinalIgnoreCase))
            .Where(leg => request.NumberOfPassengers <= leg.Inventory.NumberOfPassengersAvailable)
            .Select(leg => new GlobalAirLeg
            {
                Id = leg.Id,
                Departure = leg.Departure,
                Arrival = leg.Arrival,
                Schedule = leg.Schedule,
                Fare = new GlobalAirFare
                {
                    Cabin = leg.Fare.Cabin,
                    PricePerPax = ApplyGroupPricing(leg.Fare.PricePerPax, request.NumberOfPassengers)
                },
                Inventory = new GlobalAirInventory
                {
                    NumberOfPassengersAvailable = leg.Inventory.NumberOfPassengersAvailable
                }
            })
            .ToList();

        return new GlobalAirAvailabilityResponse
        {
            Results = results
        };
    }

    public async Task<GlobalAirLeg?> GetFlightByIdAsync(
        string flightId,
        SearchFlightRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await Task.Delay(TimeSpan.FromMilliseconds(50), cancellationToken);

        var departureDate = request.DepartureDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var allResults = new List<GlobalAirLeg>
        {
            CreateLeg("100234", request, departureDate, "Economy", 349.99m, 9),
            CreateLeg("100235", request, departureDate, "Business", 899.50m, 4)
        };

        var leg = allResults
            .FirstOrDefault(item => item.Id.Equals(flightId, StringComparison.OrdinalIgnoreCase));

        if (leg is null ||
            !leg.Fare.Cabin.Equals(request.CabinClass, StringComparison.OrdinalIgnoreCase) ||
            request.NumberOfPassengers > leg.Inventory.NumberOfPassengersAvailable)
        {
            return null;
        }

        return new GlobalAirLeg
        {
            Id = leg.Id,
            Departure = leg.Departure,
            Arrival = leg.Arrival,
            Schedule = leg.Schedule,
            Fare = new GlobalAirFare
            {
                Cabin = leg.Fare.Cabin,
                PricePerPax = ApplyGroupPricing(leg.Fare.PricePerPax, request.NumberOfPassengers)
            },
            Inventory = new GlobalAirInventory
            {
                NumberOfPassengersAvailable = leg.Inventory.NumberOfPassengersAvailable
            }
        };
    }

    private static GlobalAirLeg CreateLeg(
        string id,
        SearchFlightRequest request,
        DateTime departureDate,
        string cabin,
        decimal pricePerPax,
        int numberOfPassengersAvailable)
    {
        var departOffset = cabin.Equals("Business", StringComparison.OrdinalIgnoreCase) ? 14 : 8;
        var arriveOffset = cabin.Equals("Business", StringComparison.OrdinalIgnoreCase) ? 26 : 20;

        return new GlobalAirLeg
        {
            Id = id,
            Departure = new GlobalAirLocation
            {
                Airport = request.Origin,
                Country = "US"
            },
            Arrival = new GlobalAirLocation
            {
                Airport = request.Destination,
                Country = "ES"
            },
            Schedule = new GlobalAirSchedule
            {
                DepartUtc = departureDate.AddHours(departOffset),
                ArriveUtc = departureDate.AddHours(arriveOffset)
            },
            Fare = new GlobalAirFare
            {
                Cabin = cabin,
                PricePerPax = pricePerPax
            },
            Inventory = new GlobalAirInventory
            {
                NumberOfPassengersAvailable = numberOfPassengersAvailable
            }
        };
    }

    private static decimal ApplyGroupPricing(decimal basePrice, int numberOfPassengers)
    {
        if (numberOfPassengers <= 1)
        {
            return basePrice;
        }

        var discount = Math.Min(0.10m, 0.02m * (numberOfPassengers - 1));
        return Math.Round(basePrice * (1 - discount), 2);
    }
}
