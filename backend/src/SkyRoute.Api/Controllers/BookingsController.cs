using Microsoft.AspNetCore.Mvc;
using SkyRoute.Core.Features.Bookings.CreateBooking;
using SkyRoute.Core.Models.Validation;

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

    /// <summary>
    /// Creates a booking for the supplied itinerary and passenger details.
    /// </summary>
    /// <param name="request">The booking request payload.</param>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    /// <returns>A <see cref="CreateBookingResponse" /> on success or a <see cref="ValidationResultDto" /> on failure.</returns>
    [ProducesResponseType(typeof(CreateBookingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationResultDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ValidationResultDto), StatusCodes.Status409Conflict)]
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
