using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrder.Dtos;
using MechanicShop.Application.Features.WorkOrder.Mappers;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace MechanicShop.Application.Features.WorkOrder.Queries.GetWorkOrderById;

public sealed class GetWorkOrderByIdQueryHandler(
    IAppDbContext context,
    ILogger<GetWorkOrderByIdQueryHandler> logger)
    : IRequestHandler<GetWorkOrderByIdQuery, Result<WorkOrderDto>>
{
    public async Task<Result<WorkOrderDto>> Handle(GetWorkOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var workOrder = await context.WorkOrders.AsNoTracking()
            .Include(x => x.RepairTasks).ThenInclude(x => x.Parts)
            .Include(x => x.Vehicle).ThenInclude(x => x.Customer)
            .Include(x => x.Labor)
            .Include(x => x.Invoice)
            .FirstOrDefaultAsync(x => x.Id == request.WorkOrderId, cancellationToken);

        if (workOrder is null)
        {
            logger.LogWarning("Work order with id {WorkOrderId} not found", request.WorkOrderId);
            return ApplicationErrors.WorkOrderNotFound;
        }

        return workOrder.ToDto();
    }
}