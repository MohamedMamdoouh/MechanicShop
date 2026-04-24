using System.ComponentModel.DataAnnotations;
namespace MechanicShop.Infrastructure.Identity.Models;

public sealed class JwtSettings
{
    public const string SectionName = "JwtSettings";

    [Required]
    public string SecretKey { get; set; } = default!;

    [Required]
    public string Issuer { get; set; } = default!;

    [Required]
    public string Audience { get; set; } = default!;

    [Range(1, int.MaxValue)]
    public int AccessTokenExpiryMinutes { get; set; }

    [Range(1, int.MaxValue)]
    public int RefreshTokenExpiryDays { get; set; }
}