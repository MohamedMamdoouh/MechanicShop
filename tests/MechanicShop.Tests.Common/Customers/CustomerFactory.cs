using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Customers.Vehicles;
namespace MechanicShop.Tests.Common.Customers;

public static class CustomerFactory
{
    private static int _phoneSequence = 1234567;

    public static Result<Customer> CreateCustomer(
        string? firstName = null,
        string? lastName = null,
        string? email = null,
        string? phoneNumber = null,
        List<Vehicle>? vehicles = null)
    {
        return Customer.Create(
            firstName ?? "John",
            lastName ?? "Doe",
            email ?? "john.doe@example.com",
            phoneNumber ?? CreateUniquePhoneNumber(),
            vehicles ?? [VehicleFactory.CreateVehicle().Value]);
    }

    private static string CreateUniquePhoneNumber()
    {
        var sequence = Interlocked.Increment(ref _phoneSequence);
        return $"+2010{sequence:00000000}";
    }
}