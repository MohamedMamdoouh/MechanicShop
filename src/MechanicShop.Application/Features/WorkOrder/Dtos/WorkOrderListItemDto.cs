using MechanicShop.Application.Features.Customer.Dtos;
using MechanicShop.Domain.WorkOrders.Enum;
namespace MechanicShop.Application.Features.WorkOrder.Dtos;

public sealed record WorkOrderListItemDto
{
    public Guid WorkOrderId { get; init; }
    public Guid? InvoiceId { get; init; }
    public VehicleDto Vehicle { get; init; } = default!;
    public string CustomerName { get; init; } = default!;
    public string LaborName { get; init; } = default!;
    public WorkOrderState Status { get; init; }
    public Spot Spot { get; init; }
    public DateTimeOffset StartAtUtc { get; init; }
    public DateTimeOffset EndAtUtc { get; init; }
    public List<string> RepairTaskNames { get; init; } = [];
}