using System.ComponentModel.DataAnnotations;
namespace MechanicShop.Infrastructure.Settings;

public sealed class AppSettings
{
    public const string SectionName = "AppSettings";

    public TimeOnly OpeningTime { get; set; }
    public TimeOnly ClosingTime { get; set; }

    [Range(1, int.MaxValue)]
    public int MaxSpots { get; set; }

    [Range(1, 1440)]
    public int MaxAppointmentDurationInMinutes { get; set; }

    [Range(1, int.MaxValue)]
    public int LocalCacheExpirationInMinutes { get; set; }

    [Range(1, int.MaxValue)]
    public int DistributedCacheExpirationInMinutes { get; set; }

    [Range(1, int.MaxValue)]
    public int DefaultPageNumber { get; set; }

    [Range(1, int.MaxValue)]
    public int DefaultPageSize { get; set; }

    [Range(1, int.MaxValue)]
    public int BookingCancellationThresholdInMinutes { get; set; }

    [Range(1, int.MaxValue)]
    public int OverdueBookingCleanupFrequencyInMinutes { get; set; }

    [Required]
    public string CorsPolicyName { get; set; } = default!;

    [Required, MinLength(1)]
    public string[] CorsAllowedOrigins { get; set; } = default!;

    [Required]
    public string ShopName { get; init; } = default!;

    [Required]
    public string PhoneValidationRegion { get; set; } = default!;

    [Required]
    public string ShopTimeZone { get; set; } = default!;
}