using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.RepairTasks.Dtos;
namespace MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTasks;

public sealed record GetRepairTasksQuery : ICachedQuery<List<RepairTaskDto>>
{
    public string CacheKey => CacheKeys.RepairTaskList();

    public string[] CacheTag => [CacheTags.RepairTasks];

    public TimeSpan CacheDuration => CacheDurations.RepairTaskList;
}