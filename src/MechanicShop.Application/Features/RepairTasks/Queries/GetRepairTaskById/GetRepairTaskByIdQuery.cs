using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.RepairTasks.Dtos;
namespace MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTaskById;

public sealed record GetRepairTaskByIdQuery(Guid Id) : ICachedQuery<RepairTaskDto>
{
    public string CacheKey => CacheKeys.RepairTaskById(Id);

    public string[] CacheTag => [CacheTags.RepairTasks];

    public TimeSpan CacheDuration => CacheDurations.RepairTaskById;
}