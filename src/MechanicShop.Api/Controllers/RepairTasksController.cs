using Asp.Versioning;
using MechanicShop.Api;
using MechanicShop.Application.Features.RepairTasks.Commands.CreateRepairTask;
using MechanicShop.Application.Features.RepairTasks.Commands.RemoveRepairTask;
using MechanicShop.Application.Features.RepairTasks.Commands.UpdateRepairTask;
using MechanicShop.Contracts.RepairTasks;
using MechanicShop.Application.Features.RepairTasks.Dtos;
using MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTaskById;
using MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTasks;
using MechanicShop.Domain.Identity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;
namespace MechanicShop.Api.Controllers;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public sealed class RepairTasksController(ISender sender) : ApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(List<RepairTaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Get all repair tasks")]
    [EndpointDescription("Retrieves a list of all repair tasks defined in the shop catalogue.")]
    [EndpointName("GetRepairTasks")]
    [MapToApiVersion("1.0")]
    [OutputCache(PolicyName = CachePolicies.AuthUser, Duration = 60)]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var query = new GetRepairTasksQuery();
        var result = await sender.Send(query, ct);
        return result.Match(Ok, Problem);
    }

    [HttpGet("{id:guid}", Name = "GetRepairTaskById")]
    [ProducesResponseType(typeof(RepairTaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Get repair task by ID")]
    [EndpointDescription("Retrieves a single repair task by its unique identifier.")]
    [EndpointName("GetRepairTaskById")]
    [MapToApiVersion("1.0")]
    [OutputCache(PolicyName = CachePolicies.AuthUser, Duration = 60)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var query = new GetRepairTaskByIdQuery(id);
        var result = await sender.Send(query, ct);
        return result.Match(Ok, Problem);
    }

    [HttpPost]
    [EnableRateLimiting(RateLimitPolicies.Writes)]
    [Authorize(Roles = nameof(Role.Manager))]
    [ProducesResponseType(typeof(RepairTaskDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Create repair task")]
    [EndpointDescription("Creates a new repair task in the shop catalogue.")]
    [EndpointName("CreateRepairTask")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Create([FromBody] CreateRepairTaskRequest request, CancellationToken ct)
    {
        var command = new CreateRepairTaskCommand(
            request.Name,
            request.LaborCost,
            request.RepairDurationMinutes,
            request.Parts.Select(p => new CreateRepairTaskPartCommand(p.Name, p.Cost, p.Quantity)).ToList());

        var result = await sender.Send(command, ct);
        return result.Match(
            repairTask => CreatedAtRoute("GetRepairTaskById", new { version = "1.0", id = repairTask.RepairTaskId }, repairTask),
            Problem);
    }

    [HttpPut]
    [EnableRateLimiting(RateLimitPolicies.Writes)]
    [Authorize(Roles = nameof(Role.Manager))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Update repair task")]
    [EndpointDescription("Updates an existing repair task including its parts.")]
    [EndpointName("UpdateRepairTask")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Update([FromBody] UpdateRepairTaskRequest request, CancellationToken ct)
    {
        var command = new UpdateRepairTaskCommand(
            request.RepairTaskId,
            request.Name,
            request.LaborCost,
            request.RepairDurationMinutes,
            request.Parts.Select(p => new UpdateRepairTaskPartCommand(p.PartId, p.Name, p.Cost, p.Quantity)).ToList());

        var result = await sender.Send(command, ct);
        return result.Match(_ => NoContent(), Problem);
    }

    [HttpDelete("{id:guid}")]
    [EnableRateLimiting(RateLimitPolicies.Writes)]
    [Authorize(Roles = nameof(Role.Manager))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Delete repair task")]
    [EndpointDescription("Deletes a repair task from the catalogue. Blocked if the task is used in any work order.")]
    [EndpointName("DeleteRepairTask")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var command = new RemoveRepairTaskCommand(id);
        var result = await sender.Send(command, ct);
        return result.Match(_ => NoContent(), Problem);
    }
}
