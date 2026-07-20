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
                13331,
                request,
                departureDate,
                "Economy",
                279.00m,
                9,
                18,
                9),
            CreateOffer(
                14421,
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

    public async Task<BudgetWingsOffer?> GetFlightByIdAsync(
        string flightId,
        SearchFlightRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await Task.Delay(TimeSpan.FromMilliseconds(50), cancellationToken);

        var departureDate = request.DepartureDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var allFlights = new List<BudgetWingsOffer>
        {
            CreateOffer(
                13331,
                request,
                departureDate,
                "Economy",
                279.00m,
                9,
                18,
                9),
            CreateOffer(
                14421,
                request,
                departureDate,
                "First Class",
                1249.99m,
                19,
                31,
                2)
        };

        var offer = allFlights
            .FirstOrDefault(item => item.FlightGuid.ToString().Equals(flightId, StringComparison.OrdinalIgnoreCase));

        if (offer is null ||
            !offer.ClassType.Equals(request.CabinClass, StringComparison.OrdinalIgnoreCase) ||
            request.NumberOfPassengers > offer.NumberOfPassengersAvailable)
        {
            return null;
        }

        return new BudgetWingsOffer
        {
            FlightGuid = offer.FlightGuid,
            FromCode = offer.FromCode,
            FromCountry = offer.FromCountry,
            ToCode = offer.ToCode,
            ToCountry = offer.ToCountry,
            DepartureTime = offer.DepartureTime,
            ArrivalTime = offer.ArrivalTime,
            ClassType = offer.ClassType,
            UnitPrice = ApplyGroupPricing(offer.UnitPrice, request.NumberOfPassengers),
            NumberOfPassengersAvailable = offer.NumberOfPassengersAvailable
        };
    }

    private static BudgetWingsOffer CreateOffer(
        int flightGuid,
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
            FromCountry = "US",
            ToCode = request.Destination,
            ToCountry = "ES",
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
