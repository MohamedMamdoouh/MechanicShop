namespace MechanicShop.Application.Features.Identity;

public sealed record TokenResponse
{
    public string AccessToken { get; init; } = null!;
    public string RefreshToken { get; init; } = null!;
    public DateTimeOffset AccessTokenExpiresOnUtc { get; init; }
    public DateTimeOffset RefreshTokenExpiresOnUtc { get; init; }
}