using Microsoft.AspNetCore.Mvc;
using SkyRoute.Contracts.Bookings;
using SkyRoute.Core.Services;

namespace SkyRoute.Api.Controllers;

[ApiController]
[Route("api/bookings")]
public class BookingsController : ControllerBase
{
    private readonly ICreateBookingService _createBookingService;

    public BookingsController(ICreateBookingService createBookingService)
    {
        _createBookingService = createBookingService;
    }

    [HttpPost]
    public async Task<ActionResult<CreateBookingResponse>> ConfirmBooking(
        [FromBody] CreateBookingRequest request,
        CancellationToken cancellationToken)
    {
        var (validationResult, response) = await _createBookingService.CreateAsync(request, cancellationToken);

        if (validationResult.StatusCode == 200 && response is not null)
        {
            return Ok(response);
        }

        return StatusCode(validationResult.StatusCode, validationResult);
    }
}
