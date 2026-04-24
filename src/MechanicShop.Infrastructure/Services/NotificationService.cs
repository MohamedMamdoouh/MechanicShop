using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Common.Utilities;
using MechanicShop.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
namespace MechanicShop.Infrastructure.Services;

public sealed class NotificationService(
    ISendGridClient sendGridClient,
    IOptions<SendGridSettings> settings,
    ILogger<NotificationService> logger) : INotificationService
{
    public async Task SendEmailAsync(
        string to,
        string CustomerName,
        string VehicleModel,
        string PickupTime,
        CancellationToken cancellationToken)
    {
        var from = new EmailAddress(settings.Value.FromEmail, settings.Value.FromName);
        var recipient = new EmailAddress(to);

        var templateData = new
        {
            customerName = CustomerName,
            vehicleModel = VehicleModel,
            pickupTime = PickupTime,
            year = DateTime.UtcNow.Year
        };

        var msg = MailHelper.CreateSingleTemplateEmail(
            from, recipient, settings.Value.TemplateId, templateData);

        var response = await sendGridClient.SendEmailAsync(msg, cancellationToken);

        if (response.IsSuccessStatusCode)
            logger.LogInformation("Email sent to {Email}", UtilityService.MaskEmail(to));
        else
            logger.LogWarning(
                "Failed to send email to {Email}. StatusCode: {StatusCode}",
                UtilityService.MaskEmail(to),
                response.StatusCode);
    }

    public Task SendSmsAsync(string to, string CustomerName, string VehicleModel, string PickupTime, CancellationToken cancellationToken)
    {
        // Simulate sending SMS by logging the message.
        var message = $"Hello {CustomerName}, your {VehicleModel} is ready for pickup at {PickupTime}.";
        logger.LogInformation("Sending SMS to {PhoneNumber}: {Message}", UtilityService.MaskPhoneNumber(to), message);
        return Task.CompletedTask;
    }
}