namespace MechanicShop.Contracts.Customers;

public sealed record CreateCustomerRequest(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    List<CreateVehicleRequest> Vehicles);

public sealed record CreateVehicleRequest(
    string Make,
    string Model,
    int Year,
    string LicensePlate);

public sealed record UpdateCustomerRequest(
    Guid CustomerId,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber);
