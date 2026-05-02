using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.WorkOrders.Enum;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.Employees;
using MechanicShop.Tests.Common.RepairTasks;
using MediatR;
namespace MechanicShop.Application.SubcutaneousTests.Features.Dashboard;

public abstract class DashboardTestBase(WebAppFactory factory)
{
    protected IMediator Mediator { get; } = factory.CreateMediator();

    protected async Task SeedWorkOrderAsync()
    {
        var context = factory.CreateDbContext();
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

        var workOrder = WorkOrder.Create(
            savedVehicle.Id, startAt, startAt.AddHours(1),
            labor.Id, Spot.A, [repairTask]).Value;

        context.WorkOrders.Add(workOrder);
        await context.SaveChangesAsync(CancellationToken.None);
    }
}
