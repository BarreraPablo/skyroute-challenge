using SkyRoute.Core.ExternalServices.BudgetWings;
using SkyRoute.Core.ExternalServices.BudgetWings.Dtos;
using SkyRoute.Core.Models;

namespace SkyRoute.Infrastructure.Proxies.BudgetWings;

public class BudgetWingsProxy : IBudgetWingsProxy
{
    public async Task<BudgetWingsSearchResponse> SearchFlightsAsync(
        SearchFlightRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await Task.Delay(TimeSpan.FromMilliseconds(50), cancellationToken);

        var departureDate = request.DepartureDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var allFlights = new List<BudgetWingsOffer>
        {
            CreateOffer(
                Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                request,
                departureDate,
                "Economy",
                279.00m,
                6,
                18,
                9),
            CreateOffer(
                Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901"),
                request,
                departureDate,
                "First Class",
                1249.99m,
                19,
                31,
                2)
        };

        var flights = allFlights
            .Where(flight => flight.ClassType.Equals(request.CabinClass, StringComparison.OrdinalIgnoreCase))
            .Where(flight => request.NumberOfPassengers <= flight.NumberOfPassengersAvailable)
            .Select(flight => new BudgetWingsOffer
            {
                FlightGuid = flight.FlightGuid,
                FromCode = flight.FromCode,
                FromCountry = flight.FromCountry,
                ToCode = flight.ToCode,
                ToCountry = flight.ToCountry,
                DepartureTime = flight.DepartureTime,
                ArrivalTime = flight.ArrivalTime,
                ClassType = flight.ClassType,
                UnitPrice = ApplyGroupPricing(flight.UnitPrice, request.NumberOfPassengers),
                NumberOfPassengersAvailable = flight.NumberOfPassengersAvailable
            })
            .ToList();

        return new BudgetWingsSearchResponse
        {
            Flights = flights
        };
    }

    private static BudgetWingsOffer CreateOffer(
        Guid flightGuid,
        SearchFlightRequest request,
        DateTime departureDate,
        string classType,
        decimal unitPrice,
        int departureHourOffset,
        int arrivalHourOffset,
        int numberOfPassengersAvailable) =>
        new()
        {
            FlightGuid = flightGuid,
            FromCode = request.Origin,
            FromCountry = "United States",
            ToCode = request.Destination,
            ToCountry = "Spain",
            DepartureTime = departureDate.AddHours(departureHourOffset),
            ArrivalTime = departureDate.AddHours(arrivalHourOffset),
            ClassType = classType,
            UnitPrice = unitPrice,
            NumberOfPassengersAvailable = numberOfPassengersAvailable
        };

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
