using Microsoft.AspNetCore.Mvc;
using SkyRoute.Core.Features.Flights.SearchFlights;
using SkyRoute.Core.Models.Validation;

namespace SkyRoute.Api.Controllers;

[ApiController]
[Route("api/flights")]
public class FlightsController : ControllerBase
{
    private readonly ISearchFlightService _searchFlightService;

    public FlightsController(ISearchFlightService searchFlightService)
    {
        _searchFlightService = searchFlightService;
    }

    /// <summary>
    /// Searches for flights that match the supplied query parameters.
    /// </summary>
    /// <param name="request">The flight search criteria.</param>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    /// <returns>A <see cref="SearchFlightsResponse" /> on success or a <see cref="ValidationResultDto" /> on validation failure.</returns>
    [ProducesResponseType(typeof(SearchFlightsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationResultDto), StatusCodes.Status400BadRequest)]
    [HttpGet("search")]
    public async Task<ActionResult<SearchFlightsResponse>> Search(
        [FromQuery] SearchFlightsRequest request,
        CancellationToken cancellationToken)
    {
        var (validationResult, response) = await _searchFlightService.SearchAsync(request, cancellationToken);

        if (validationResult.Conditions.Any(condition =>
            condition.Severity == ValidationSeverity.Error))
        {
            return BadRequest(validationResult);
        }

        return Ok(response);
    }
}
