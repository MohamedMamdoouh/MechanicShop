using MechanicShop.Domain.Identity;
namespace MechanicShop.Application.Common.Interfaces;

public interface ITokenSessionService
{
    Task<RefreshToken?> GetValidTokenAsync(
        string rawToken,
        string userId,
        string deviceId,
        string? userAgent,
        CancellationToken ct);

    Task<(RefreshToken Token, string RawToken)> RotateAsync(
        RefreshToken current,
        DeviceInfo device,
        CancellationToken ct);

    Task ReplaceDeviceSessionAsync(
        string userId,
        DeviceInfo device,
        RefreshToken newToken,
        CancellationToken ct);

    Task RevokeAllAsync(
        Guid userId,
        CancellationToken ct);

    Task RevokeDeviceSessionAsync(
        string userId,
        string deviceId,
        CancellationToken ct);
}
