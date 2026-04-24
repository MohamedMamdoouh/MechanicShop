using MechanicShop.Application.Features.Customer.Commands.CreateCustomer;
using MechanicShop.Application.SubcutaneousTests.Common;
using Xunit;
namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Commands;

[Collection(WebAppFactoryCollection.CollectionName)]
public class CreateCustomerTests(WebAppFactory factory) : CustomerTestBase(factory)
{
    [Fact]
    public async Task Handle_WhenValidRequest_ReturnsCustomerDto()
    {
        var id = Guid.NewGuid().ToString("N")[..8];
        var licensePlate = $"ABC{id[..6]}".ToUpperInvariant();
        var phoneNumber = CreateUniquePhoneNumber();

        var command = new CreateCustomerCommand(
            "Jane",
            "Smith",
            $"{id}@test.com",
            phoneNumber,
            [new CreateVehicleCommand("Toyota", "Camry", 2022, licensePlate)]);

        var result = await Mediator.Send(command);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
        Assert.Equal("Jane", result.Value.FirstName);
        Assert.Equal("Smith", result.Value.LastName);
        Assert.Equal($"{id}@test.com", result.Value.Email);
        Assert.Single(result.Value.Vehicles);
    }

    [Fact]
    public async Task Handle_WhenEmailAlreadyExists_ReturnsConflictError()
    {
        var customer = await SeedCustomerAsync();
        var id = Guid.NewGuid().ToString("N")[..8];
        var licensePlate = $"PLT{id[..6]}".ToUpperInvariant();
        var phoneNumber = CreateUniquePhoneNumber();

        var command = new CreateCustomerCommand(
            "Other",
            "User",
            customer.Email,
            phoneNumber,
            [new CreateVehicleCommand("Honda", "Civic", 2021, licensePlate)]);

        var result = await Mediator.Send(command);

        Assert.False(result.IsSuccess);
        Assert.Equal("ApplicationErrors.Customer.EmailAlreadyExists", result.TopError!.Value.Code);
    }

    private static string CreateUniquePhoneNumber()
    {
        var bytes = Guid.NewGuid().ToByteArray();
        var sequence = BitConverter.ToUInt32(bytes, 0) % 100_000_000;
        return $"+2015{sequence:D8}";
    }
}
