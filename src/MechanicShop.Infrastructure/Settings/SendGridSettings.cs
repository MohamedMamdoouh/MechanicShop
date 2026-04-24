using System.ComponentModel.DataAnnotations;
namespace MechanicShop.Infrastructure.Settings;

public sealed class SendGridSettings
{
    public const string SectionName = "SendGridSettings";

    [Required]
    public string ApiKey { get; init; } = default!;

    [Required]
    public string FromEmail { get; init; } = default!;

    [Required]
    public string FromName { get; init; } = default!;

    [Required]
    public string TemplateId { get; init; } = default!;
}
