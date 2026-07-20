using SkyRoute.Core.Models.Validation;

namespace SkyRoute.Core.Features.Bookings.CreateBooking;

public interface ICreateBookingService
{
    Task<(ValidationResultDto ValidationResult, CreateBookingResponse? Response)> CreateAsync(
        CreateBookingRequest request,
        CancellationToken cancellationToken);
}
