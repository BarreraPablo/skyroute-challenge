using Microsoft.AspNetCore.Mvc;
using SkyRoute.Core.Services;
using SkyRoute.Core.Services.Airports;

namespace SkyRoute.Api.Controllers;

[ApiController]
[Route("api/airports")]
public class AirportsController : ControllerBase
{
    private readonly IAirportReferenceService _airportReferenceService;

    public AirportsController(IAirportReferenceService airportReferenceService)
    {
        _airportReferenceService = airportReferenceService;
    }

    /// <summary>
    /// Gets the list of available airports.
    /// </summary>
    /// <returns>A read-only list of <see cref="AirportResponse" /> values.</returns>
    [ProducesResponseType(typeof(IReadOnlyList<AirportResponse>), StatusCodes.Status200OK)]
    [HttpGet]
    public ActionResult<IReadOnlyList<AirportResponse>> GetAirports() =>
        Ok(_airportReferenceService.GetAirports());
}
