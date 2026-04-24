using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Identity;
namespace MechanicShop.Tests.Common.Auth;

public static class RefreshTokenFactory
{
    public static Result<RefreshToken> Create(
        string? token = null,
        string? userId = null,
        DateTimeOffset? expiresOnUtc = null,
        DeviceInfo? device = null,
        string? serverFingerprint = null,
        DateTimeOffset? now = null)
    {
        return RefreshToken.Create(token ?? Guid.NewGuid().ToString(),
            userId ?? Guid.NewGuid().ToString(),
            expiresOnUtc ?? DateTimeOffset.UtcNow.AddDays(7),
            device ?? new DeviceInfo(Guid.NewGuid().ToString(), "Test Device", "1.1.1.1"),
            serverFingerprint ?? Guid.NewGuid().ToString(),
            now ?? DateTimeOffset.UtcNow);
    }
}