using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Dashboard.Dtos;
namespace MechanicShop.Application.Features.Dashboard.Queries;

public sealed record GetWorkOrderStatsQuery(DateOnly Date)
    : ICachedQuery<TodayWorkOrderStatsDto>
{
    public string CacheKey => CacheKeys.DashboardStatsByDate(Date);

    public string[] CacheTag => [CacheTags.Dashboard];

    public TimeSpan CacheDuration => CacheDurations.DashboardStats;
}