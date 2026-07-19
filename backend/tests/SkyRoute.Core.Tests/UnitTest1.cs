using SkyRoute.Contracts.Bookings;
using SkyRoute.Core.Entities;
using SkyRoute.Core.ExternalServices;
using SkyRoute.Core.Interfaces;
using SkyRoute.Core.Models;
using SkyRoute.Core.Services;

namespace SkyRoute.Core.Tests;

public class CreateBookingServiceTests
{
    [Fact]
    public async Task CreateAsync_WhenSnapshotMatches_ReturnsSuccessAndPersistsBooking()
    {
        var repository = new FakeBookingRepository();
        var provider = new FakeProvider("BudgetWings", CreateMatchingFlight());
        var airportReferenceService = new AirportReferenceService();
        var sut = new CreateBookingService([provider], repository, airportReferenceService);

        var request = BuildValidRequest();

        var (validationResult, response) = await sut.CreateAsync(request, CancellationToken.None);

        Assert.Equal(200, validationResult.StatusCode);
        Assert.NotNull(response);
        Assert.Single(repository.StoredBookings);
        Assert.Equal("US", repository.StoredBookings[0].OriginCountry);
        Assert.Equal("ES", repository.StoredBookings[0].DestinationCountry);
        Assert.Equal(request.ExpectedPrice, repository.StoredBookings[0].TotalPrice);
    }

    [Fact]
    public async Task CreateAsync_WhenProviderDataDiffers_ReturnsConflict()
    {
        var repository = new FakeBookingRepository();
        var flight = CreateMatchingFlight();
        flight.TotalPrice += 10;

        var provider = new FakeProvider("BudgetWings", flight);
        var airportReferenceService = new AirportReferenceService();
        var sut = new CreateBookingService([provider], repository, airportReferenceService);

        var request = BuildValidRequest();

        var (validationResult, response) = await sut.CreateAsync(request, CancellationToken.None);

        Assert.Equal(409, validationResult.StatusCode);
        Assert.Null(response);
        Assert.Contains(validationResult.Conditions, item => item.Message.Contains("stale", StringComparison.OrdinalIgnoreCase));
        Assert.Empty(repository.StoredBookings);
    }

    [Fact]
    public async Task CreateAsync_WhenDomesticDocumentIsNotNationalId_ReturnsValidationError()
    {
        var repository = new FakeBookingRepository();
        var domesticFlight = CreateMatchingFlight();
        domesticFlight.Destination = "LAX";
        domesticFlight.DestinationCountry = "US";

        var provider = new FakeProvider("BudgetWings", domesticFlight);
        var airportReferenceService = new AirportReferenceService();
        var sut = new CreateBookingService([provider], repository, airportReferenceService);

        var request = BuildValidRequest() with
        {
            DestinationCode = "LAX",
            Passenger = new CreateBookingPassengerRequest("John Doe", "john.doe@email.com", "AB123456")
        };

        var (validationResult, response) = await sut.CreateAsync(request, CancellationToken.None);

        Assert.Equal(400, validationResult.StatusCode);
        Assert.Null(response);
        Assert.Contains(validationResult.Conditions, item => item.Message.Contains("National ID", StringComparison.OrdinalIgnoreCase));
        Assert.Empty(repository.StoredBookings);
    }

    [Fact]
    public async Task CreateAsync_WhenCabinClassIsInvalid_ReturnsValidationError()
    {
        var repository = new FakeBookingRepository();
        var provider = new FakeProvider("BudgetWings", CreateMatchingFlight());
        var airportReferenceService = new AirportReferenceService();
        var sut = new CreateBookingService([provider], repository, airportReferenceService);

        var request = BuildValidRequest() with
        {
            CabinClass = "Premium Economy"
        };

        var (validationResult, response) = await sut.CreateAsync(request, CancellationToken.None);

        Assert.Equal(400, validationResult.StatusCode);
        Assert.Null(response);
        Assert.Contains(validationResult.Conditions, item => item.Message.Contains("Cabin class must be one of", StringComparison.OrdinalIgnoreCase));
        Assert.Empty(repository.StoredBookings);
    }

    private static CreateBookingRequest BuildValidRequest() =>
        new(
            FlightId: "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
            Provider: "BudgetWings",
            OriginCode: "JFK",
            DestinationCode: "MAD",
            DepartureTimeUtc: new DateTime(2026, 8, 20, 18, 0, 0, DateTimeKind.Utc),
            ArrivalTimeUtc: new DateTime(2026, 8, 20, 18, 0, 0, DateTimeKind.Utc).AddHours(9),
            CabinClass: "Economy",
            PricePerPassenger: 262.26m,
            PassengerCount: 4,
            ExpectedPrice: 1049.04m,
            Passenger: new CreateBookingPassengerRequest("John Doe", "john.doe@email.com", "A1234567"));

    private static FlightResponse CreateMatchingFlight() =>
        new()
        {
            ProviderName = "BudgetWings",
            FlightId = "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
            Origin = "JFK",
            Destination = "MAD",
            OriginCountry = "US",
            DestinationCountry = "ES",
            DepartureTimeUtc = new DateTime(2026, 8, 20, 18, 0, 0, DateTimeKind.Utc),
            ArrivalTimeUtc = new DateTime(2026, 8, 20, 18, 0, 0, DateTimeKind.Utc).AddHours(9),
            CabinClass = "Economy",
            PricePerPassenger = 262.26m,
            TotalPrice = 1049.04m
        };

    private sealed class FakeBookingRepository : IBookingRepository
    {
        public List<Booking> StoredBookings { get; } = [];

        public Task AddAsync(Booking booking, CancellationToken cancellationToken)
        {
            StoredBookings.Add(booking);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeProvider : IFlightProviderExternalService
    {
        private readonly FlightResponse? _flight;

        public FakeProvider(string providerName, FlightResponse? flight)
        {
            ProviderName = providerName;
            _flight = flight;
        }

        public string ProviderName { get; }

        public Task<IReadOnlyList<FlightResponse>> SearchFlightsAsync(
            SearchFlightRequest request,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<FlightResponse>>(_flight is null ? [] : [_flight]);

        public Task<FlightResponse?> GetFlightByIdAsync(
            string flightId,
            SearchFlightRequest request,
            CancellationToken cancellationToken) =>
            Task.FromResult(_flight);
    }
}
