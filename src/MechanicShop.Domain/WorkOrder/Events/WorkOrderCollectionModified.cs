using MechanicShop.Domain.Common.BaseEntities;
namespace MechanicShop.Domain.WorkOrders.Events;

public sealed class WorkOrderCollectionModified(Guid workOrderId) : DomainEvent
{
    public Guid WorkOrderId { get; init; } = workOrderId;
}