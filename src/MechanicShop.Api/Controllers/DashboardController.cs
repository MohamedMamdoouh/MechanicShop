using Asp.Versioning;
using MechanicShop.Application.Features.Dashboard.Dtos;
using MechanicShop.Application.Features.Dashboard.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace MechanicShop.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class DashboardController(ISender sender) : ApiController
{
    [HttpGet("today-stats")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(TodayWorkOrderStatsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTodayStatsAsync(DateOnly date, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetWorkOrderStatsQuery(date), cancellationToken);
        return result.Match(Ok, Problem);
    }
}
