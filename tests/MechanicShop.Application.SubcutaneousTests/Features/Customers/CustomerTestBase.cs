using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.Customers;
using MechanicShop.Tests.Common.Customers;
using MediatR;
namespace MechanicShop.Application.SubcutaneousTests.Features.Customers;

public abstract class CustomerTestBase(WebAppFactory factory)
{
    protected IMediator Mediator { get; } = factory.CreateMediator();

    protected async Task<Customer> SeedCustomerAsync(string? email = null)
    {
        var context = factory.CreateDbContext();
        var id = Guid.NewGuid().ToString("N")[..8];

        var vehicle = VehicleFactory.CreateVehicle(licensePlate: $"Plate-{id}").Value;
        var customer = CustomerFactory.CreateCustomer(
            email: email ?? $"{id}@test.com",
            vehicles: [vehicle]).Value;

        context.Customers.Add(customer);
        await context.SaveChangesAsync(CancellationToken.None);

        return customer;
    }
}
