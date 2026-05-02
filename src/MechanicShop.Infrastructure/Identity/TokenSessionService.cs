using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Identity;
using MechanicShop.Infrastructure.Identity.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
namespace MechanicShop.Infrastructure.Services;

public sealed class TokenSessionService(
    IAppDbContext context,
    IRefreshTokenFactory factory,
    IOptions<JwtSettings> jwtSettings,
    TimeProvider timeProvider) : ITokenSessionService
{
    public async Task<RefreshToken?> GetValidTokenAsync(
        string rawToken,
        string userId,
        string deviceId,
        string? userAgent,
        CancellationToken ct)
    {
        var tokens = await context.RefreshTokens
            .Where(t => t.UserId == userId
                     && t.Device.Identifier == deviceId
                     && t.ExpiresOnUtc > timeProvider.GetUtcNow())
            .ToListAsync(ct);

        var match = tokens.Find(t => RefreshTokenFactory.Verify(rawToken, t.Token));

        if (match is null)
        {
            return null;
        }

        if (match.IsConsumed)
        {
            if (Guid.TryParse(userId, out var parsedUserId))
            {
                await RevokeAllAsync(parsedUserId, ct);
            }

            return null;
        }

        if (match.ExpiresOnUtc <= timeProvider.GetUtcNow())
        {
            return null;
        }

        var expectedFingerprint = factory.BuildFingerprint(deviceId, userAgent);

        if (match.ServerFingerprint != expectedFingerprint)
        {
            return null;
        }

        return match;
    }

    public async Task<(RefreshToken Token, string RawToken)> RotateAsync(
        RefreshToken current,
        DeviceInfo device,
        CancellationToken ct)
    {
        if (!Guid.TryParse(current.UserId, out var userId))
        {
            throw new InvalidOperationException("Refresh token contains invalid user ID.");
        }

        current.MarkAsConsumed();

        var (newToken, raw) = factory.Create(
            userId,
            device,
            timeProvider.GetUtcNow().AddDays(jwtSettings.Value.RefreshTokenExpiryDays));

        context.RefreshTokens.Update(current);
        context.RefreshTokens.Add(newToken);
        await context.SaveChangesAsync(ct);

        return (newToken, raw);
    }

    public async Task ReplaceDeviceSessionAsync(
        string userId,
        DeviceInfo device,
        RefreshToken newToken,
        CancellationToken ct)
    {
        var existing = await context.RefreshTokens
            .Where(t => t.UserId == userId && t.Device.Identifier == device.Identifier)
            .ToListAsync(ct);

        context.RefreshTokens.RemoveRange(existing);
        context.RefreshTokens.Add(newToken);

        await context.SaveChangesAsync(ct);
    }

    public async Task RevokeAllAsync(Guid userId, CancellationToken ct)
    {
        var tokens = await context.RefreshTokens
            .Where(t => t.UserId == userId.ToString())
            .ToListAsync(ct);

        context.RefreshTokens.RemoveRange(tokens);
        await context.SaveChangesAsync(ct);
    }

    public async Task RevokeDeviceSessionAsync(string userId, string deviceId, CancellationToken ct)
    {
        var tokens = await context.RefreshTokens
            .Where(t => t.UserId == userId && t.Device.Identifier == deviceId)
            .ToListAsync(ct);

        context.RefreshTokens.RemoveRange(tokens);
        await context.SaveChangesAsync(ct);
    }
}