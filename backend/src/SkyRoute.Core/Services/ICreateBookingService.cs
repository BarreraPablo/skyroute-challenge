using SkyRoute.Contracts.Bookings;
using SkyRoute.Contracts.Validation;

namespace SkyRoute.Core.Services;

public interface ICreateBookingService
{
    Task<(ValidationResultDto ValidationResult, CreateBookingResponse? Response)> CreateAsync(
        CreateBookingRequest request,
        CancellationToken cancellationToken);
}
