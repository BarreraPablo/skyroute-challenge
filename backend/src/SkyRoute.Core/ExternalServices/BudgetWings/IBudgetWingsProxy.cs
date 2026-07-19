using SkyRoute.Core.ExternalServices.BudgetWings.Dtos;
using SkyRoute.Core.Models;

namespace SkyRoute.Core.ExternalServices.BudgetWings;

public interface IBudgetWingsProxy
{
    Task<BudgetWingsSearchResponse> SearchFlightsAsync(
        SearchFlightRequest request,
        CancellationToken cancellationToken);

    Task<BudgetWingsOffer?> GetFlightByIdAsync(
        string flightId,
        SearchFlightRequest request,
        CancellationToken cancellationToken);
}
