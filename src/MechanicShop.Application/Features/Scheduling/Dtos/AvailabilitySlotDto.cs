using MechanicShop.Application.Features.Customer.Dtos;
using MechanicShop.Application.Features.Labor.Dtos;
using MechanicShop.Application.Features.RepairTasks.Dtos;
using MechanicShop.Domain.WorkOrders.Enum;
namespace MechanicShop.Application.Features.Scheduling.Dtos;

public sealed record AvailabilitySlotDto
{
    public Guid? WorkOrderId { get; init; }
    public Spot Spot { get; init; }
    public DateTimeOffset StartAtUtc { get; init; }
    public DateTimeOffset EndAtUtc { get; init; }
    public VehicleDto? Vehicle { get; init; }
    public LaborDto? Labor { get; init; }
    public bool IsOccupied => WorkOrderId.HasValue;
    public bool IsBookable { get; init; }
    public bool WorkOrderLocked { get; init; }
    public WorkOrderState? WorkOrderState { get; init; }
    public List<RepairTaskDto>? RepairTasks { get; init; }
}