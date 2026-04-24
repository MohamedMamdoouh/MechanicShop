namespace MechanicShop.Application.Features.Customer.Dtos;

public sealed record CustomerDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    List<VehicleDto> Vehicles);
