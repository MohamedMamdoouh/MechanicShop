using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Common.Models;
using MechanicShop.Application.Features.WorkOrder.Dtos;
using MechanicShop.Application.Features.WorkOrder.Mappers;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace MechanicShop.Application.Features.WorkOrder.Queries.GetWorkOrders;

public sealed class GetWorkOrdersQueryHandler(IAppDbContext context)
    : IRequestHandler<GetWorkOrdersQuery, Result<PaginatedList<WorkOrderListItemDto>>>
{
    public async Task<Result<PaginatedList<WorkOrderListItemDto>>> Handle(
        GetWorkOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var workOrdersQuery = context.WorkOrders.AsNoTracking()
        .Include(x => x.RepairTasks).ThenInclude(x => x.Parts)
        .Include(x => x.Vehicle).ThenInclude(x => x.Customer)
        .Include(x => x.Labor)
        .Include(x => x.Invoice)
        .AsQueryable();

        workOrdersQuery = ApplyFilters(workOrdersQuery, request);
        workOrdersQuery = ApplySearchTermFilter(workOrdersQuery, request.SearchTerm);
        workOrdersQuery = ApplySorting(workOrdersQuery, request.SortBy, request.SortDescending);

        var count = await workOrdersQuery.CountAsync(cancellationToken);

        var items = await workOrdersQuery
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListItemDto()
            .ToListAsync(cancellationToken);

        return new PaginatedList<WorkOrderListItemDto>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = count
        };
    }

    private static IQueryable<Domain.WorkOrders.WorkOrder> ApplySearchTermFilter(
        IQueryable<Domain.WorkOrders.WorkOrder> query, string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return query;

        var term = searchTerm.Trim();

        return query.Where(wo =>
            (wo.Vehicle != null &&
                (
                    EF.Functions.Like(wo.Vehicle.Make, $"%{term}%") ||
                    EF.Functions.Like(wo.Vehicle.Model, $"%{term}%") ||
                    EF.Functions.Like(wo.Vehicle.LicensePlate, $"%{term}%")
                )
            )

            ||

            (wo.Labor != null &&
                EF.Functions.Like(wo.Labor.FullName, $"%{term}%")
            )

            ||

            wo.RepairTasks.Any(rt =>
                EF.Functions.Like(rt.Name, $"%{term}%")
            )
        );
    }

    private static IQueryable<Domain.WorkOrders.WorkOrder> ApplyFilters(
        IQueryable<Domain.WorkOrders.WorkOrder> query,
        GetWorkOrdersQuery request)
    {
        if (request.Status.HasValue)
        {
            query = query.Where(wo => wo.Status == request.Status.Value);
        }

        if (request.VehicleId.HasValue)
        {
            query = query.Where(wo => wo.VehicleId == request.VehicleId.Value);
        }

        if (request.LaborId.HasValue)
        {
            query = query.Where(wo => wo.LaborId == request.LaborId.Value);
        }

        if (request.StartDateFrom.HasValue)
        {
            query = query.Where(wo => wo.StartAtUtc >= request.StartDateFrom.Value);
        }

        if (request.StartDateTo.HasValue)
        {
            query = query.Where(wo => wo.StartAtUtc <= request.StartDateTo.Value);
        }

        if (request.EndDateFrom.HasValue)
        {
            query = query.Where(wo => wo.EndAtUtc >= request.EndDateFrom.Value);
        }

        if (request.EndDateTo.HasValue)
        {
            query = query.Where(wo => wo.EndAtUtc <= request.EndDateTo.Value);
        }

        if (request.Spot.HasValue)
        {
            query = query.Where(wo => wo.Spot == request.Spot.Value);
        }

        return query;
    }

    private static IQueryable<Domain.WorkOrders.WorkOrder> ApplySorting(
        IQueryable<Domain.WorkOrders.WorkOrder> query,
        string sortBy,
        bool sortDescending)
    {
        return (sortBy, sortDescending) switch
        {
            ("createdat", false) => query.OrderBy(wo => wo.CreatedAtUtc),
            ("createdat", true) => query.OrderByDescending(wo => wo.CreatedAtUtc),

            ("startat", false) => query.OrderBy(wo => wo.StartAtUtc),
            ("startat", true) => query.OrderByDescending(wo => wo.StartAtUtc),

            ("endat", false) => query.OrderBy(wo => wo.EndAtUtc),
            ("endat", true) => query.OrderByDescending(wo => wo.EndAtUtc),

            ("status", false) => query.OrderBy(wo => wo.Status).ThenByDescending(wo => wo.CreatedAtUtc),
            ("status", true) => query.OrderByDescending(wo => wo.Status).ThenByDescending(wo => wo.CreatedAtUtc),

            ("spot", false) => query.OrderBy(wo => wo.Spot).ThenByDescending(wo => wo.CreatedAtUtc),
            ("spot", true) => query.OrderByDescending(wo => wo.Spot).ThenByDescending(wo => wo.CreatedAtUtc),

            ("totalamount", false) => query.OrderBy(wo => wo.Invoice != null ? wo.Invoice.TotalAmount : 0),
            ("totalamount", true) => query.OrderByDescending(wo => wo.Invoice != null ? wo.Invoice.TotalAmount : 0),

            _ => query.OrderByDescending(wo => wo.CreatedAtUtc)
        };
    }

}