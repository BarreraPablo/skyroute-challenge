using SkyRoute.Core.Models.Validation;

namespace SkyRoute.Core.Features.Bookings.CreateBooking;

public interface ICreateBookingValidationService
{
    ValidationResultDto ValidateRequest(CreateBookingRequest request);
}
