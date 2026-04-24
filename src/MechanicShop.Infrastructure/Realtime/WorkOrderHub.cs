using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
namespace MechanicShop.Infrastructure.Realtime;

[Authorize]
public sealed class WorkOrderHub : Hub
{
    public const string WorkOrderChangedMethod = "WorkOrderChanged";
    public const string HubUrl = "/hubs/workorders";
}