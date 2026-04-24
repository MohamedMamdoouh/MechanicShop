using MechanicShop.Application.Features.Customer.Dtos;
using MechanicShop.Application.Features.Labor.Dtos;
using MechanicShop.Application.Features.RepairTasks.Dtos;
using MechanicShop.Domain.WorkOrders.Enum;
namespace MechanicShop.Application.Features.WorkOrder.Dtos;

public sealed record WorkOrderDto
{
    public Guid WorkOrderId { get; init; }
    public Guid? InvoiceId { get; init; }
    public Spot Spot { get; init; }
    public VehicleDto Vehicle { get; init; } = default!;
    public DateTimeOffset StartAtUtc { get; init; }
    public DateTimeOffset EndAtUtc { get; init; }
    public List<RepairTaskDto> RepairTasks { get; init; } = [];
    public LaborDto Labor { get; init; } = default!;
    public WorkOrderState Status { get; init; }
    public decimal TotalPartsCost { get; init; }
    public decimal TotalLaborCost { get; init; }
    public decimal TotalCost { get; init; }
    public int TotalDurationInMinutes { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
}