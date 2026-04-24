namespace MechanicShop.Application.Common.Interfaces;

public interface INotificationService
{
    // Body of the email & SMS is predefined, only recipient is required
    Task SendEmailAsync(
        string to,
        string CustomerName,
        string VehicleModel,
        string PickupTime,
        CancellationToken cancellationToken);

    Task SendSmsAsync(
        string to,
        string CustomerName,
        string VehicleModel,
        string PickupTime,
        CancellationToken cancellationToken);
}