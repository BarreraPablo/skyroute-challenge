using Microsoft.AspNetCore.Mvc;
using SkyRoute.Contracts.Flights;
using SkyRoute.Contracts.Validation;
using SkyRoute.Core.Services;

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
