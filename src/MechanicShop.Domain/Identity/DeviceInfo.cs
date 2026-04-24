namespace MechanicShop.Domain.Identity;

public sealed record DeviceInfo(
    string Identifier,
    string? UserAgent,
    string? IpAddress);
