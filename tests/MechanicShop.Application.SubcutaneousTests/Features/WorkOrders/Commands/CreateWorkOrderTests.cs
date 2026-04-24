using MechanicShop.Application.Features.WorkOrder.Commands.CreateWorkOrder;
using MechanicShop.Application.Features.WorkOrder.Dtos;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.WorkOrders.Enum;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.Employees;
using MechanicShop.Tests.Common.RepairTasks;
using Xunit;
namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands;

[Collection(WebAppFactoryCollection.CollectionName)]
public class CreateWorkOrderTests(WebAppFactory factory) : WorkOrderTestBase(factory)
{
    [Fact]
    public async Task CreateWorkOrder_WithValidData_ReturnsWorkOrderDto()
    {
        // Arrange
        var context = Factory.CreateDbContext();
        var id = Guid.NewGuid().ToString("N")[..8];

        var vehicle = VehicleFactory.CreateVehicle(licensePlate: $"plate-{id}").Value;
        var customer = CustomerFactory.CreateCustomer(email: $"{id}@test.com", vehicles: [vehicle]).Value;
        var labor = EmployeeFactory.CreateLabor().Value;
        var repairTask = RepairTaskFactory.Create(name: $"Task-{id}").Value;

        context.Customers.Add(customer);
        context.Employees.Add(labor);
        context.RepairTasks.Add(repairTask);
        await context.SaveChangesAsync(CancellationToken.None);

        // Tomorrow at 10:00 AM local → future and within business hours (09:00–17:00)
        var localOffset = DateTimeOffset.Now.Offset;
        var startAt = new DateTimeOffset(DateTime.Today.AddDays(1).AddHours(10), localOffset);

        var command = new CreateWorkOrderCommand(
            Spot: Spot.A,
            VehicleId: vehicle.Id,
            StartAt: startAt,
            RepairTaskIds: [repairTask.Id],
            LaborId: labor.Id);

        // Act
        var result = await Mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.IsType<WorkOrderDto>(result.Value);
        Assert.Equal(vehicle.Id, result.Value.Vehicle.Id);
    }

    [Fact]
    public async Task CreateWorkOrder_WithNonExistentRepairTask_ReturnsRepairTaskNotFoundError()
    {
        // Arrange
        var context = Factory.CreateDbContext();
        var id = Guid.NewGuid().ToString("N")[..8];

        var vehicle = VehicleFactory.CreateVehicle(licensePlate: $"plate-{id}").Value;
        var customer = CustomerFactory.CreateCustomer(email: $"{id}@test.com", vehicles: [vehicle]).Value;
        var labor = EmployeeFactory.CreateLabor().Value;

        context.Customers.Add(customer);
        context.Employees.Add(labor);
        await context.SaveChangesAsync(CancellationToken.None);

        var localOffset = DateTimeOffset.Now.Offset;
        var startAt = new DateTimeOffset(DateTime.Today.AddDays(1).AddHours(10), localOffset);

        var command = new CreateWorkOrderCommand(
            Spot: Spot.A,
            VehicleId: vehicle.Id,
            StartAt: startAt,
            RepairTaskIds: [Guid.NewGuid()],
            LaborId: labor.Id);

        // Act
        var result = await Mediator.Send(command);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "ApplicationErrors.RepairTask.NotFound");
    }
}
