using MechanicShop.Domain.Common.BaseEntities;
using MechanicShop.Domain.Common.Results;
namespace MechanicShop.Domain.Identity;

public sealed class RefreshToken : AuditableEntity
{
    public string Token { get; } = null!;
    public string UserId { get; } = null!;
    public DateTimeOffset ExpiresOnUtc { get; }
    public bool IsConsumed { get; private set; }
    public string ServerFingerprint { get; } = null!;
    public DeviceInfo Device { get; private set; } = null!;

    // Ef Core requires a parameterless constructor for materialization.
    private RefreshToken() { }

    private RefreshToken(
        string token,
        string userId,
        DateTimeOffset expiresOnUtc,
        DeviceInfo device,
        string serverFingerprint)
    {
        Token = token;
        UserId = userId;
        ExpiresOnUtc = expiresOnUtc;
        Device = device;
        ServerFingerprint = serverFingerprint;
    }

    public void MarkAsConsumed() => IsConsumed = true;

    public static Result<RefreshToken> Create(
        string token,
        string userId,
        DateTimeOffset expiresOnUtc,
        DeviceInfo device,
        string serverFingerprint,
        DateTimeOffset now)
    {
        var errors = new List<Error>();

        if (string.IsNullOrWhiteSpace(token))
            errors.Add(RefreshTokenErrors.TokenRequired);

        if (string.IsNullOrWhiteSpace(userId) ||
            (Guid.TryParse(userId, out var parsedUserId) && parsedUserId == Guid.Empty))
            errors.Add(RefreshTokenErrors.UserIdRequired);

        if (expiresOnUtc <= now)
            errors.Add(RefreshTokenErrors.ExpiryInvalid);

        if (string.IsNullOrWhiteSpace(device.Identifier))
            errors.Add(RefreshTokenErrors.DeviceIdentifierRequired);

        if (string.IsNullOrWhiteSpace(serverFingerprint))
            errors.Add(RefreshTokenErrors.ServerFingerprintRequired);

        if (errors.Count > 0)
            return errors;

        return new RefreshToken(
            token,
            userId,
            expiresOnUtc,
            device,
            serverFingerprint
        );
    }
}