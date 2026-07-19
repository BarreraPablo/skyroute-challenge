using Microsoft.AspNetCore.Mvc;
using SkyRoute.Contracts.Flights;
using SkyRoute.Core.Services;

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
