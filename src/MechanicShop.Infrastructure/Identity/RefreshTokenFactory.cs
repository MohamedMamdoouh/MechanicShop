using System.Security.Cryptography;
using System.Text;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Common.Models;
using MechanicShop.Domain.Identity;
using Microsoft.Extensions.Options;
namespace MechanicShop.Infrastructure.Services;

public sealed class RefreshTokenFactory(IOptions<TokenSettings> settings, TimeProvider timeProvider) : IRefreshTokenFactory
{
    private readonly string _salt = settings.Value.FingerprintSalt;

    public (RefreshToken Token, string RawToken) Create(
        Guid userId,
        DeviceInfo device,
        DateTimeOffset expiresOnUtc)
    {
        var rawToken = GenerateSecureToken();
        var hashedToken = Hash(rawToken);

        var fingerprint = BuildFingerprint(device.Identifier, device.UserAgent);

        var token = RefreshToken.Create(
            hashedToken,
            userId.ToString(),
            expiresOnUtc,
            device,
            fingerprint,
            timeProvider.GetUtcNow()
        ).Value;

        return (token, rawToken);
    }

    public static bool Verify(string rawToken, string storedHash)
        => Hash(rawToken) == storedHash;

    public string BuildFingerprint(string deviceId, string? userAgent)
    {
        var payload = $"{deviceId}|{userAgent ?? ""}|{_salt}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(bytes);
    }

    private static string Hash(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }

    private static string GenerateSecureToken()
    {
        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }
}