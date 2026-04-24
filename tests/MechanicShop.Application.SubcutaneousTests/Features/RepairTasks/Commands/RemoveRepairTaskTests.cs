using MechanicShop.Application.Features.RepairTasks.Commands.RemoveRepairTask;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.WorkOrders.Enum;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.Employees;
using MechanicShop.Tests.Common.RepairTasks;
using Xunit;
namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Commands;

[Collection(WebAppFactoryCollection.CollectionName)]
public class RemoveRepairTaskTests(WebAppFactory factory) : RepairTaskTestBase(factory)
{
    [Fact]
    public async Task Handle_WhenRepairTaskExists_ReturnsSuccess()
    {
        var repairTask = await SeedRepairTaskAsync();

        var result = await Mediator.Send(new RemoveRepairTaskCommand(repairTask.Id));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_WhenRepairTaskNotFound_ReturnsNotFoundError()
    {
        var result = await Mediator.Send(new RemoveRepairTaskCommand(Guid.NewGuid()));

        Assert.False(result.IsSuccess);
        Assert.Equal("ApplicationErrors.RepairTask.NotFound", result.TopError!.Value.Code);
    }

    [Fact]
    public async Task Handle_WhenRepairTaskInUseByWorkOrder_ReturnsConflictError()
    {
        var context = Factory.CreateDbContext();
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
        var startAt = DateTimeOffset.UtcNow.AddHours(-2);
        var workOrder = Domain.WorkOrders.WorkOrder.Create(
            savedVehicle.Id, startAt, startAt.AddHours(1),
            labor.Id, Spot.A, [repairTask]).Value;

        context.WorkOrders.Add(workOrder);
        await context.SaveChangesAsync(CancellationToken.None);

        var result = await Mediator.Send(new RemoveRepairTaskCommand(repairTask.Id));

        Assert.False(result.IsSuccess);
        Assert.Equal("ApplicationErrors.RepairTask.InUse", result.TopError!.Value.Code);
    }
}
