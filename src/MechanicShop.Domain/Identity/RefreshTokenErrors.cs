using MechanicShop.Domain.Common.Results;
namespace MechanicShop.Domain.Identity;

public static class RefreshTokenErrors
{
    public static Error TokenRequired
        => Error.Validation("Refresh token is required.", "RefreshToken.Token.Required");

    public static Error UserIdRequired
        => Error.Validation("User ID is required.", "RefreshToken.UserId.Required");

    public static Error ExpiryInvalid
        => Error.Validation("Refresh token expiry is invalid.", "RefreshToken.Expiry.Invalid");

    public static Error ServerFingerprintRequired
        => Error.Validation("Server fingerprint is required.", "RefreshToken.ServerFingerprint.Required");

    public static Error DeviceIdentifierRequired
        => Error.Validation("Device identifier is required.", "RefreshToken.DeviceIdentifier.Required");
}