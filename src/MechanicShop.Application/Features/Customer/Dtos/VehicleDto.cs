namespace MechanicShop.Application.Features.Customer.Dtos;

public sealed record VehicleDto(
Guid Id,
string Make,
string Model,
int Year,
string LicensePlate);