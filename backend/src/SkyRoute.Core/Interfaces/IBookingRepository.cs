using SkyRoute.Core.Entities;

namespace SkyRoute.Core.Interfaces;

public interface IBookingRepository
{
    Task AddAsync(Booking booking, CancellationToken cancellationToken);
}
