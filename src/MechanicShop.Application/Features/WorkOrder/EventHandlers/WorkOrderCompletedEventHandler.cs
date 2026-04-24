using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.WorkOrders.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace MechanicShop.Application.Features.WorkOrder.EventHandlers;

public sealed class WorkOrderCompletedEventHandler(
    IAppDbContext context,
    ILogger<WorkOrderCompletedEventHandler> logger,
    INotificationService notificationService)
    : INotificationHandler<WorkOrderCompleted>
{
    public async Task Handle(WorkOrderCompleted notification, CancellationToken cancellationToken)
    {
        var workOrder = await context.WorkOrders.AsNoTracking()
        .Include(x => x.Vehicle).ThenInclude(x => x.Customer)
        .FirstOrDefaultAsync(x => x.Id == notification.WorkOrderId, cancellationToken);

        if (workOrder == null)
        {
            logger.LogWarning("Work order with ID {WorkOrderId} not found for completion event", notification.WorkOrderId);
            return;
        }

        var customerName = workOrder.Vehicle.Customer.FullName;
        var vehicleModel = workOrder.Vehicle.VehicleInfo;
        var pickupTime = workOrder.EndAtUtc.ToString("g");

        await notificationService.SendEmailAsync(
            workOrder.Vehicle.Customer.Email,
            customerName,
            vehicleModel,
            pickupTime,
            cancellationToken);

        await notificationService.SendSmsAsync(
            workOrder.Vehicle.Customer.PhoneNumber,
            customerName,
            vehicleModel,
            pickupTime,
            cancellationToken);
    }
}