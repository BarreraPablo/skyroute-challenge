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

    [HttpGet]
    public ActionResult<IReadOnlyList<AirportResponse>> GetAirports() =>
        Ok(_airportReferenceService.GetAirports());
}
