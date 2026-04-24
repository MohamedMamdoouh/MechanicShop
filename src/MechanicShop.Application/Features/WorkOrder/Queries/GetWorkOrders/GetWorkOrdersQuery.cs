using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Common.Models;
using MechanicShop.Application.Features.WorkOrder.Dtos;
using MechanicShop.Domain.WorkOrders.Enum;
namespace MechanicShop.Application.Features.WorkOrder.Queries.GetWorkOrders;

public sealed record GetWorkOrdersQuery(
    int PageNumber,
    int PageSize,
    string? SearchTerm = null,
    string SortBy = "createdAt",
    bool SortDescending = false,
    WorkOrderState? Status = null,
    Guid? VehicleId = null,
    Guid? LaborId = null,
    DateTime? StartDateFrom = null,
    DateTime? StartDateTo = null,
    DateTime? EndDateFrom = null,
    DateTime? EndDateTo = null,
    Spot? Spot = null
)
    : ICachedQuery<PaginatedList<WorkOrderListItemDto>>
{
    public string CacheKey => CacheKeys.WorkOrderListItemPaginated(new WorkOrderListFilter(
        PageNumber,
        PageSize,
        SearchTerm,
        SortBy,
        SortDescending,
        VehicleId,
        LaborId,
        StartDateFrom,
        StartDateTo,
        EndDateFrom,
        EndDateTo,
        Spot,
        Status));

    public string[] CacheTag => [CacheTags.WorkOrders];

    public TimeSpan CacheDuration => CacheDurations.WorkOrderPaginatedList;
}