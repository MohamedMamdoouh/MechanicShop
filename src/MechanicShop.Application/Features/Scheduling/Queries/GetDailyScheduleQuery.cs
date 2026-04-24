using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Scheduling.Dtos;
namespace MechanicShop.Application.Features.Scheduling.Queries;

public sealed record GetDailyScheduleQuery(
    DateOnly ScheduledDate,
    TimeZoneInfo TimeZone,
    Guid? LaborerId = null) :
    ICachedQuery<ScheduleDto>
{
    public string CacheKey => CacheKeys.Schedule(ScheduledDate, LaborerId);

    public string[] CacheTag => [CacheTags.Schedules];

    public TimeSpan CacheDuration => CacheDurations.DailySchedule;
}