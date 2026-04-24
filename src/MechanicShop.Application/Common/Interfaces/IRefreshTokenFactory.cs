using MechanicShop.Domain.Identity;
namespace MechanicShop.Application.Common.Interfaces;

public interface IRefreshTokenFactory
{
    (RefreshToken Token, string RawToken) Create(
        Guid userId,
        DeviceInfo device,
        DateTimeOffset expiresOnUtc);

    string BuildFingerprint(
        string deviceId,
        string? userAgent);
}
