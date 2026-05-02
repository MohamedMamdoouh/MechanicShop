using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using MechanicShop.Api;
using MechanicShop.Application.Common.Models;
using MechanicShop.Application.Features.WorkOrder.Commands.AssignLabor;
using MechanicShop.Application.Features.WorkOrder.Commands.CreateWorkOrder;
using MechanicShop.Application.Features.WorkOrder.Commands.DeleteWorkOrder;
using MechanicShop.Application.Features.WorkOrder.Commands.RelocateWorkOrder;
using MechanicShop.Application.Features.WorkOrder.Commands.UpdateOrderState;
using MechanicShop.Application.Features.WorkOrder.Commands.UpdateWorkOrderRepairTasks;
using MechanicShop.Application.Features.WorkOrder.Dtos;
using MechanicShop.Application.Features.WorkOrder.Queries.GetWorkOrderById;
using MechanicShop.Application.Features.WorkOrder.Queries.GetWorkOrders;
using MechanicShop.Contracts.WorkOrders;
using MechanicShop.Domain.Identity;
using MechanicShop.Domain.WorkOrders.Enum;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;
namespace MechanicShop.Api.Controllers;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public sealed class WorkOrdersController(ISender sender) : ApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<WorkOrderListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Get work orders")]
    [EndpointDescription("Returns a paginated, filtered, and sorted list of work orders.")]
    [EndpointName("GetWorkOrders")]
    [MapToApiVersion("1.0")]
    [OutputCache(PolicyName = CachePolicies.AuthUser, Duration = 60)]
    public async Task<IActionResult> Get(
        [FromQuery][Range(1, int.MaxValue)] int pageNumber = 1,
        [FromQuery][Range(1, 100)] int pageSize = 20,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string sortBy = "createdAt",
        [FromQuery] bool sortDescending = false,
        [FromQuery] WorkOrderState? status = null,
        [FromQuery] Guid? vehicleId = null,
        [FromQuery] Guid? laborId = null,
        [FromQuery] DateTime? startDateFrom = null,
        [FromQuery] DateTime? startDateTo = null,
        [FromQuery] DateTime? endDateFrom = null,
        [FromQuery] DateTime? endDateTo = null,
        [FromQuery] Spot? spot = null,
        CancellationToken ct = default)
    {
        var query = new GetWorkOrdersQuery(
            pageNumber, pageSize, searchTerm, sortBy, sortDescending,
            status, vehicleId, laborId,
            startDateFrom, startDateTo, endDateFrom, endDateTo, spot);

        var result = await sender.Send(query, ct);
        return result.Match(Ok, Problem);
    }

    [HttpGet("{id:guid}", Name = "GetWorkOrderById")]
    [ProducesResponseType(typeof(WorkOrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Get work order by ID")]
    [EndpointDescription("Retrieves the full details of a work order by its identifier.")]
    [EndpointName("GetWorkOrderById")]
    [MapToApiVersion("1.0")]
    [OutputCache(PolicyName = CachePolicies.AuthUser, Duration = 60)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var query = new GetWorkOrderByIdQuery(id);
        var result = await sender.Send(query, ct);
        return result.Match(Ok, Problem);
    }

    [HttpPost]
    [EnableRateLimiting(RateLimitPolicies.Writes)]
    [Authorize(Roles = nameof(Role.Manager))]
    [ProducesResponseType(typeof(WorkOrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Create work order")]
    [EndpointDescription("Schedules a new work order for a vehicle.")]
    [EndpointName("CreateWorkOrder")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Create([FromBody] CreateWorkOrderRequest request, CancellationToken ct)
    {
        var command = new CreateWorkOrderCommand(
            request.Spot,
            request.VehicleId,
            request.StartAt,
            request.RepairTaskIds,
            request.LaborId);

        var result = await sender.Send(command, ct);
        return result.Match(
            wo => CreatedAtRoute("GetWorkOrderById", new { version = "1.0", id = wo.WorkOrderId }, wo),
            Problem);
    }

    [HttpDelete("{id:guid}")]
    [EnableRateLimiting(RateLimitPolicies.Writes)]
    [Authorize(Roles = nameof(Role.Manager))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Delete work order")]
    [EndpointDescription("Deletes a scheduled work order. Blocked if the order is in progress or completed.")]
    [EndpointName("DeleteWorkOrder")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var command = new DeleteWorkOrderCommand(id);
        var result = await sender.Send(command, ct);
        return result.Match(_ => NoContent(), Problem);
    }

    [HttpPut("{id:guid}/repair-tasks")]
    [EnableRateLimiting(RateLimitPolicies.Writes)]
    [Authorize(Roles = nameof(Role.Manager))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Update work order repair tasks")]
    [EndpointDescription("Replaces the repair tasks on a work order and recalculates end time.")]
    [EndpointName("UpdateWorkOrderRepairTasks")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> UpdateRepairTasks(Guid id, [FromBody] UpdateWorkOrderRepairTasksRequest request, CancellationToken ct)
    {
        var command = new UpdateWorkOrderRepairTasksCommand(id, request.RepairTaskIds);
        var result = await sender.Send(command, ct);
        return result.Match(_ => NoContent(), Problem);
    }

    [HttpPatch("{id:guid}/assign-labor")]
    [EnableRateLimiting(RateLimitPolicies.Writes)]
    [Authorize(Roles = nameof(Role.Manager))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Assign labor to work order")]
    [EndpointDescription("Assigns a labor employee to a work order, replacing any existing assignment.")]
    [EndpointName("AssignLabor")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> AssignLabor(Guid id, [FromBody] AssignLaborRequest request, CancellationToken ct)
    {
        var command = new AssignLaborCommand(id, request.LaborId);
        var result = await sender.Send(command, ct);
        return result.Match(_ => NoContent(), Problem);
    }

    [HttpPatch("{id:guid}/relocate")]
    [EnableRateLimiting(RateLimitPolicies.Writes)]
    [Authorize(Roles = nameof(Role.Manager))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Relocate work order")]
    [EndpointDescription("Changes the start time and/or spot of a work order, rechecking all scheduling conflicts.")]
    [EndpointName("RelocateWorkOrder")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Relocate(Guid id, [FromBody] RelocateWorkOrderRequest request, CancellationToken ct)
    {
        var command = new RelocateWorkOrderCommand(id, request.NewStartAt, request.Spot);
        var result = await sender.Send(command, ct);
        return result.Match(_ => NoContent(), Problem);
    }

    [HttpPatch("{id:guid}/state")]
    [Authorize(Roles = nameof(Role.Manager))]
    [EnableRateLimiting(RateLimitPolicies.Writes)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Update work order state")]
    [EndpointDescription("Transitions a work order to a new state (e.g. Scheduled → InProgress → Completed).")]
    [EndpointName("UpdateWorkOrderState")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> UpdateState(Guid id, [FromBody] UpdateWorkOrderStateRequest request, CancellationToken ct)
    {
        var command = new UpdateWorkOrderStateCommand(id, request.NewState);
        var result = await sender.Send(command, ct);
        return result.Match(_ => NoContent(), Problem);
    }
}
