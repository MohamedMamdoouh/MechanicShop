using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Dashboard.Dtos;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace MechanicShop.Application.Features.Dashboard.Queries;

public sealed class GetWorkOrderStatsQueryHandler(
    IAppDbContext context,
    ILogger<GetWorkOrderStatsQueryHandler> logger)
    : IRequestHandler<GetWorkOrderStatsQuery, Result<TodayWorkOrderStatsDto>>
{
    private const int RoundingScale = 2;

    public async Task<Result<TodayWorkOrderStatsDto>> Handle(
        GetWorkOrderStatsQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Calculating dashboard work order stats for date {Date}", request.Date);

        var start = new DateTimeOffset(request.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        var end = start.AddDays(1);

        var stats = await context.WorkOrders
        .AsNoTracking()
        .Where(x => x.CreatedAtUtc >= start && x.CreatedAtUtc < end)
        .GroupBy(_ => 1)
        .Select(g => new
        {
            Total = g.Count(),

            Scheduled = g.Count(x => x.Status == WorkOrderState.Scheduled),
            InProgress = g.Count(x => x.Status == WorkOrderState.InProgress),
            Completed = g.Count(x => x.Status == WorkOrderState.Completed),
            Cancelled = g.Count(x => x.Status == WorkOrderState.Cancelled),

            TotalRevenue = g.Sum(x => x.Invoice != null
                ? x.Invoice.LineItems.Sum(li => li.Quantity * li.UnitPrice) - x.Invoice.DiscountAmount + x.Invoice.TaxAmount
                : 0m),
            TotalPartsCost = g.Sum(x => x.RepairTasks.Sum(rt => rt.Parts.Sum(p => p.Cost * p.Quantity))),
            TotalLaborCost = g.Sum(x => x.RepairTasks.Sum(rt => rt.LaborCost)),

            UniqueVehicles = g.Select(x => x.VehicleId).Distinct().Count(),
            UniqueCustomers = g.Select(x => x.Vehicle.CustomerId).Distinct().Count()
        })
        .SingleOrDefaultAsync(cancellationToken);

        if (stats == null)
        {
            logger.LogInformation("No work orders found for date {Date}", request.Date);
            return new TodayWorkOrderStatsDto { Date = request.Date };
        }

        var totalRevenue = stats.TotalRevenue;
        var totalPartsCost = stats.TotalPartsCost;
        var totalLaborCost = stats.TotalLaborCost;

        var netProfit = totalRevenue - totalPartsCost - totalLaborCost;
        var profitMargin = totalRevenue > 0 ? decimal.Round(netProfit / totalRevenue * 100, RoundingScale) : 0;

        var completionRate = stats.Total > 0 ? decimal.Round(stats.Completed / (decimal)stats.Total * 100, RoundingScale) : 0;
        var averageRevenuePerOrder = stats.Total > 0 ? decimal.Round(totalRevenue / stats.Total, RoundingScale) : 0;
        var ordersPerVehicle = stats.UniqueVehicles > 0 ? decimal.Round(stats.Total / (decimal)stats.UniqueVehicles, RoundingScale) : 0;
        var partsCostRatio = totalRevenue > 0 ? decimal.Round(totalPartsCost / totalRevenue * 100, RoundingScale) : 0;
        var laborCostRatio = totalRevenue > 0 ? decimal.Round(totalLaborCost / totalRevenue * 100, RoundingScale) : 0;
        var cancellationRate = stats.Total > 0 ? decimal.Round(stats.Cancelled / (decimal)stats.Total * 100, RoundingScale) : 0;

        logger.LogInformation(
            "Dashboard stats calculated for {Date}: total={Total}, completed={Completed}, revenue={Revenue}",
            request.Date,
            stats.Total,
            stats.Completed,
            stats.TotalRevenue);

        return new TodayWorkOrderStatsDto
        {
            Date = request.Date,
            Total = stats.Total,
            Scheduled = stats.Scheduled,
            InProgress = stats.InProgress,
            Completed = stats.Completed,
            Cancelled = stats.Cancelled,
            TotalRevenue = stats.TotalRevenue,
            TotalPartsCost = stats.TotalPartsCost,
            TotalLaborCost = stats.TotalLaborCost,
            UniqueVehicles = stats.UniqueVehicles,
            UniqueCustomers = stats.UniqueCustomers,
            NetProfit = netProfit,
            ProfitMargin = profitMargin,
            CompletionRate = completionRate,
            AverageRevenuePerOrder = averageRevenuePerOrder,
            OrdersPerVehicle = ordersPerVehicle,
            PartsCostRatio = partsCostRatio,
            LaborCostRatio = laborCostRatio,
            CancellationRate = cancellationRate
        };
    }
}
