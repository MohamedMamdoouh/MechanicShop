namespace MechanicShop.Contracts.Identity;

public sealed record LoginRequest(
    string Email,
    string Password,
    string DeviceIdentifier);

public sealed record RefreshTokensRequest(
    string RefreshToken,
    string ExpiredAccessToken,
    string DeviceIdentifier);

public sealed record LogoutRequest(
    string DeviceIdentifier);
