using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.WorkOrders.Events;
using MediatR;
namespace MechanicShop.Application.Features.WorkOrder.EventHandlers;

public sealed class WorkOrderCollectionModifiedEventHandler(IWorkOrderNotifier notifier)
    : INotificationHandler<WorkOrderCollectionModified>
{
    public async Task Handle(WorkOrderCollectionModified notification, CancellationToken cancellationToken)
    {
        await notifier.NotifyWorkOrderChangedAsync(notification.WorkOrderId, cancellationToken);
    }
}