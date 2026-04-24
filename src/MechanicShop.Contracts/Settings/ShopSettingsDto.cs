namespace MechanicShop.Contracts.Settings;

public sealed record ShopSettingsDto(
    string ShopName,
    TimeOnly OpeningTime,
    TimeOnly ClosingTime,
    int MaxSpots,
    int MaxAppointmentDurationInMinutes);
