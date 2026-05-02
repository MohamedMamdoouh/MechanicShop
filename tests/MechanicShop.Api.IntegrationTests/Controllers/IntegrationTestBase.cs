using MechanicShop.Api.IntegrationTests.Common;
using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Employees;
using MechanicShop.Domain.Identity;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Enum;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.Employees;
using MechanicShop.Tests.Common.RepairTasks;
namespace MechanicShop.Api.IntegrationTests.Controllers;

public abstract class IntegrationTestBase(WebFactory factory)
{
    protected WebFactory Factory { get; } = factory;

    protected async Task<AppHttpClient> CreateAuthenticatedClientAsync(Role role = Role.Manager)
    {
        var (email, password) = await Factory.SeedUserAsync(role);
        var client = Factory.CreateAppHttpClient();
        await client.AuthenticateAsync(email, password);
        return client;
    }

    protected AppHttpClient CreateUnauthenticatedClient() => Factory.CreateAppHttpClient();

    protected async Task<Customer> SeedCustomerAsync(string? email = null)
    {
        var context = Factory.CreateDbContext();
        var id = Guid.NewGuid().ToString("N")[..8];

        var vehicle = VehicleFactory.CreateVehicle(licensePlate: $"PLT-{id}").Value;
        var customer = CustomerFactory.CreateCustomer(
            email: email ?? $"{id}@test.com",
            vehicles: [vehicle]).Value;

        context.Customers.Add(customer);
        await context.SaveChangesAsync(CancellationToken.None);
        return customer;
    }

    protected async Task<RepairTask> SeedRepairTaskAsync(string? name = null)
    {
        var context = Factory.CreateDbContext();
        var id = Guid.NewGuid().ToString("N")[..8];

        var repairTask = RepairTaskFactory.Create(name: name ?? $"Task-{id}").Value;
        context.RepairTasks.Add(repairTask);
        await context.SaveChangesAsync(CancellationToken.None);
        return repairTask;
    }

    protected async Task<Employee> SeedLaborAsync()
    {
        var context = Factory.CreateDbContext();
        var labor = EmployeeFactory.CreateLabor().Value;
        context.Employees.Add(labor);
        await context.SaveChangesAsync(CancellationToken.None);
        return labor;
    }

    protected async Task<WorkOrder> SeedWorkOrderAsync(Spot spot = Spot.A)
    {
        var context = Factory.CreateDbContext();
        var id = Guid.NewGuid().ToString("N")[..8];

        var vehicle = VehicleFactory.CreateVehicle(licensePlate: $"WO-{id}").Value;
        var customer = CustomerFactory.CreateCustomer(email: $"wo-{id}@test.com", vehicles: [vehicle]).Value;
        var labor = EmployeeFactory.CreateLabor().Value;
        var repairTask = RepairTaskFactory.Create(name: $"WOTask-{id}").Value;

        context.Customers.Add(customer);
        context.Employees.Add(labor);
        context.RepairTasks.Add(repairTask);
        await context.SaveChangesAsync(CancellationToken.None);

        var scheduledStartAt = DateTimeOffset.UtcNow.AddHours(-2);
        var workOrder = WorkOrder.Create(
            vehicle.Id,
            scheduledStartAt,
            scheduledStartAt.AddHours(1),
            labor.Id,
            spot,
            [repairTask]).Value;

        context.WorkOrders.Add(workOrder);
        await context.SaveChangesAsync(CancellationToken.None);
        return workOrder;
    }
}
