using SkyRoute.Core.Entities;
using SkyRoute.Core.Interfaces;

namespace SkyRoute.Infrastructure.Persistence.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly SkyRouteDbContext _dbContext;

    public BookingRepository(SkyRouteDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Booking booking, CancellationToken cancellationToken)
    {
        _dbContext.Bookings.Add(booking);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
