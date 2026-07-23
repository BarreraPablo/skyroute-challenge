using SkyRoute.Core.ExternalServices.ArcticAir;
using SkyRoute.Core.ExternalServices.ArcticAir.Dtos;
using SkyRoute.Core.Models;

namespace SkyRoute.Infrastructure.Proxies.ArcticAir;

public class ArcticAirProxy : IArcticAirProxy
{
    public async Task<ArcticAirSearchResponse> SearchFlightsAsync(
        SearchFlightRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await Task.Delay(TimeSpan.FromMilliseconds(50), cancellationToken);

        var departureDate = request.DepartureDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var allFlights = new List<ArcticAirOffer>
        {
            CreateOffer(
                25001,
                request,
                departureDate,
                "Economy",
                199.50m,
                7,
                16,
                12),
            CreateOffer(
                25002,
                request,
                departureDate,
                "Business",
                749.00m,
                11,
                22,
                4)
        };

        var flights = allFlights
            .Where(flight => flight.ClassType.Equals(request.CabinClass, StringComparison.OrdinalIgnoreCase))
            .Where(flight => request.NumberOfPassengers <= flight.NumberOfPassengersAvailable)
            .Select(flight => new ArcticAirOffer
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

        return new ArcticAirSearchResponse
        {
            Flights = flights
        };
    }

    public async Task<ArcticAirOffer?> GetFlightByIdAsync(
        string flightId,
        SearchFlightRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await Task.Delay(TimeSpan.FromMilliseconds(50), cancellationToken);

        var departureDate = request.DepartureDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var allFlights = new List<ArcticAirOffer>
        {
            CreateOffer(
                25001,
                request,
                departureDate,
                "Economy",
                199.50m,
                7,
                16,
                12),
            CreateOffer(
                25002,
                request,
                departureDate,
                "Business",
                749.00m,
                11,
                22,
                4)
        };

        var offer = allFlights
            .FirstOrDefault(item => item.FlightGuid.ToString().Equals(flightId, StringComparison.OrdinalIgnoreCase));

        if (offer is null ||
            !offer.ClassType.Equals(request.CabinClass, StringComparison.OrdinalIgnoreCase) ||
            request.NumberOfPassengers > offer.NumberOfPassengersAvailable)
        {
            return null;
        }

        return new ArcticAirOffer
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

    private static ArcticAirOffer CreateOffer(
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
