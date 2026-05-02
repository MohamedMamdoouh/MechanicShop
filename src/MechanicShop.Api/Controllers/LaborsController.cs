using Asp.Versioning;
using MechanicShop.Application.Features.Labor.Dtos;
using MechanicShop.Application.Features.Labor.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
namespace MechanicShop.Api.Controllers;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public sealed class LaborsController(ISender sender) : ApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(List<LaborDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Get all labors")]
    [EndpointDescription("Retrieves a list of all labor employees available in the shop.")]
    [EndpointName("GetLabors")]
    [MapToApiVersion("1.0")]
    [OutputCache(PolicyName = CachePolicies.AuthUser, Duration = 60)]
    public async Task<IActionResult> GetAsync(CancellationToken ct)
    {
        var query = new GetLaborsQuery();
        var result = await sender.Send(query, ct);
        return result.Match(Ok, Problem);
    }
}
