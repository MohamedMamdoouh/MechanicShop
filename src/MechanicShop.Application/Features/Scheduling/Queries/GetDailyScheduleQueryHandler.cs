using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Customer.Mappers;
using MechanicShop.Application.Features.Labor.Mappers;
using MechanicShop.Application.Features.RepairTasks.Mappers;
using MechanicShop.Application.Features.Scheduling.Dtos;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace MechanicShop.Application.Features.Scheduling.Queries;

public sealed class GetDailyScheduleQueryHandler(IAppDbContext context)
    : IRequestHandler<GetDailyScheduleQuery, Result<ScheduleDto>>
{
    public async Task<Result<ScheduleDto>> Handle(GetDailyScheduleQuery request, CancellationToken cancellationToken)
    {
        var localStart = request.ScheduledDate.ToDateTime(TimeOnly.MinValue);
        var localEnd = localStart.AddDays(1);

        var utcStart = TimeZoneInfo.ConvertTimeToUtc(localStart, request.TimeZone);
        var utcEnd = TimeZoneInfo.ConvertTimeToUtc(localEnd, request.TimeZone);

        var workOrders = await context.WorkOrders
            .AsNoTracking()
            .Include(wo => wo.Vehicle)
            .Include(wo => wo.Labor)
            .Include(wo => wo.RepairTasks).ThenInclude(rt => rt.Parts)

            .Where(wo =>
                wo.StartAtUtc < utcEnd &&
                (wo.EndAtUtc > utcStart) &&
                wo.Status != WorkOrderState.Cancelled &&
                (request.LaborerId == null || wo.LaborId == request.LaborerId))

            .OrderBy(wo => wo.StartAtUtc)
            .ToListAsync(cancellationToken);

        var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, request.TimeZone);

        var result = new ScheduleDto
        {
            Date = request.ScheduledDate,
            IsDayInPast = nowLocal > localEnd
        };

        foreach (var spot in Enum.GetValues<Spot>())
        {
            var workOrdersBySpot = workOrders.Where(wo => wo.Spot == spot).ToList();

            result.Slots.Add(new SpotDto
            {
                Spot = spot,
                AvailabilitySlots = BuildSlotsForSpot(spot, workOrdersBySpot, localStart, localEnd, nowLocal, request.TimeZone)
            });
        }

        return result;
    }

    private static List<AvailabilitySlotDto> BuildSlotsForSpot(
        Spot spot,
        List<Domain.WorkOrders.WorkOrder> workOrdersBySpot,
        DateTime localStart,
        DateTime localEnd,
        DateTime nowLocal,
        TimeZoneInfo timeZone)
    {
        var current = localStart;
        var slots = new List<AvailabilitySlotDto>();

        while (current < localEnd)
        {
            var next = current.AddMinutes(15);
            var startUtc = TimeZoneInfo.ConvertTimeToUtc(current, timeZone);
            var endUtc = TimeZoneInfo.ConvertTimeToUtc(next, timeZone);

            var workOrder = workOrdersBySpot.Find(wo =>
                wo.StartAtUtc < endUtc && (wo.EndAtUtc > startUtc));

            if (workOrder != null)
            {
                // This means we have a work order that overlaps with the current slot,
                // so we create an availability slot for it
                slots.Add(new AvailabilitySlotDto
                {
                    WorkOrderId = workOrder.Id,
                    Spot = spot,
                    StartAtUtc = workOrder.StartAtUtc,
                    EndAtUtc = workOrder.EndAtUtc,
                    Vehicle = workOrder.Vehicle.ToDto(),
                    Labor = workOrder.Labor.ToDto(),
                    WorkOrderLocked = !workOrder.IsEditable,
                    WorkOrderState = workOrder.Status,
                    RepairTasks = workOrder.RepairTasks.ToList().ToDto()
                });

                // We need to move the current time forward to the end of this work order,
                // or the next slot if the work order ends after the next slot
                var workOrderLocalEnd = TimeZoneInfo.ConvertTimeFromUtc(workOrder.EndAtUtc.UtcDateTime, timeZone);
                current = workOrderLocalEnd > next ? workOrderLocalEnd : next;
            }
            else
            {
                // This means we have no work order for this slot, so we create an available slot
                slots.Add(new AvailabilitySlotDto
                {
                    Spot = spot,
                    StartAtUtc = startUtc,
                    EndAtUtc = endUtc,
                    WorkOrderLocked = false,
                    IsBookable = current >= nowLocal
                });

                current = next;
            }
        }

        return slots;
    }
}