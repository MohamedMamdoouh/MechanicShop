using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrder.Dtos;
namespace MechanicShop.Application.Features.WorkOrder.Queries.GetWorkOrderById;

public sealed record GetWorkOrderByIdQuery(Guid WorkOrderId) : ICachedQuery<WorkOrderDto>
{
    public string CacheKey => CacheKeys.WorkOrderById(WorkOrderId);

    public string[] CacheTag => [CacheTags.WorkOrders, CacheTags.WorkOrderById(WorkOrderId)];

    public TimeSpan CacheDuration => CacheDurations.WorkOrderById;
}