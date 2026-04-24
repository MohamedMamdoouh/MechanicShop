using MechanicShop.Application.Features.Customer.Commands.DeleteCustomer;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Enum;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.Employees;
using MechanicShop.Tests.Common.RepairTasks;
using Xunit;
namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Commands;

[Collection(WebAppFactoryCollection.CollectionName)]
public class DeleteCustomerTests(WebAppFactory factory) : CustomerTestBase(factory)
{
    private readonly WebAppFactory _factory = factory;

    [Fact]
    public async Task Handle_WhenCustomerExists_ReturnsSuccess()
    {
        var customer = await SeedCustomerAsync();

        var result = await Mediator.Send(new DeleteCustomerCommand(customer.Id));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_WhenCustomerNotFound_ReturnsNotFoundError()
    {
        var result = await Mediator.Send(new DeleteCustomerCommand(Guid.NewGuid()));

        Assert.False(result.IsSuccess);
        Assert.Equal("ApplicationErrors.Customer.NotFound", result.TopError!.Value.Code);
    }

    [Fact]
    public async Task Handle_WhenCustomerHasActiveWorkOrders_ReturnsConflictError()
    {
        var context = _factory.CreateDbContext();
        var id = Guid.NewGuid().ToString("N")[..8];

        var vehicle = VehicleFactory.CreateVehicle(licensePlate: $"Plate-{id}").Value;
        var customer = CustomerFactory.CreateCustomer(email: $"{id}@test.com", vehicles: [vehicle]).Value;
        var labor = EmployeeFactory.CreateLabor().Value;
        var repairTask = RepairTaskFactory.Create(name: $"Task-{id}").Value;

        context.Customers.Add(customer);
        context.Employees.Add(labor);
        context.RepairTasks.Add(repairTask);
        await context.SaveChangesAsync(CancellationToken.None);

        var savedVehicle = customer.Vehicles.First();
        var startAt = DateTimeOffset.UtcNow.AddHours(-1);
        var workOrder = WorkOrder.Create(
            savedVehicle.Id, startAt, startAt.AddHours(2),
            labor.Id, Spot.A, [repairTask]).Value;

        context.WorkOrders.Add(workOrder);
        await context.SaveChangesAsync(CancellationToken.None);

        var result = await Mediator.Send(new DeleteCustomerCommand(customer.Id));

        Assert.False(result.IsSuccess);
        Assert.Equal("ApplicationErrors.Customer.HasActiveWorkOrders", result.TopError!.Value.Code);
    }
}
