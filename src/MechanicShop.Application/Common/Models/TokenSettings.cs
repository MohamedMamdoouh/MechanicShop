using System.ComponentModel.DataAnnotations;
namespace MechanicShop.Application.Common.Models;

public sealed class TokenSettings
{
    public const string SectionName = "TokenSettings";

    [Required(ErrorMessage =
    "TokenSettings:FingerprintSalt is required. Set environment variable: TOKENSETTINGS__FINGERPRINTSALT")]
    [MinLength(1, ErrorMessage = "TokenSettings:FingerprintSalt must not be empty.")]
    public string FingerprintSalt { get; init; } = default!;
}
