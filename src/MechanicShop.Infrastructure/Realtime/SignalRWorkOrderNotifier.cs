using MechanicShop.Application.Common.Interfaces;
using Microsoft.AspNetCore.SignalR;
namespace MechanicShop.Infrastructure.Realtime;

public sealed class SignalRWorkOrderNotifier(IHubContext<WorkOrderHub> hubContext) : IWorkOrderNotifier
{
    public async Task NotifyWorkOrderChangedAsync(Guid workOrderId, CancellationToken cancellationToken)
    {
        await hubContext.Clients.All.SendAsync(WorkOrderHub.WorkOrderChangedMethod, workOrderId, cancellationToken);
    }
}