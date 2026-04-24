namespace MechanicShop.Application.Common.Interfaces;

public interface IWorkOrderNotifier
{
    Task NotifyWorkOrderChangedAsync(Guid workOrderId, CancellationToken cancellationToken);
}