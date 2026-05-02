using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.Employees;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Enum;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.Employees;
using MechanicShop.Tests.Common.RepairTasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders;

public abstract class WorkOrderTestBase(WebAppFactory factory)
{
    protected WebAppFactory Factory { get; } = factory;
    protected IMediator Mediator { get; } = factory.CreateMediator();

    protected async Task<WorkOrder> SeedScheduledWorkOrderAsync(Spot spot = Spot.A, DateTimeOffset? startAt = null)
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

        var scheduledStartAt = startAt ?? DateTimeOffset.UtcNow.AddHours(-2);
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

    protected async Task<WorkOrder> SeedInProgressWorkOrderAsync()
    {
        var seeded = await SeedScheduledWorkOrderAsync(Spot.B);

        var context = Factory.CreateDbContext();
        var workOrder = await context.WorkOrders
            .Include(wo => wo.RepairTasks)
            .FirstAsync(wo => wo.Id == seeded.Id, CancellationToken.None);

        var transitionResult = workOrder.UpdateStatus(WorkOrderState.InProgress, DateTimeOffset.UtcNow);
        if (!transitionResult.IsSuccess)
        {
            throw new InvalidOperationException($"Failed to seed InProgress work order: {transitionResult.TopError?.Code}");
        }

        await context.SaveChangesAsync(CancellationToken.None);

        return workOrder;
    }

    protected async Task<WorkOrder> SeedCompletedWorkOrderAsync()
    {
        var seeded = await SeedScheduledWorkOrderAsync(Spot.C);

        var context = Factory.CreateDbContext();
        var workOrder = await context.WorkOrders
            .Include(wo => wo.RepairTasks)
            .FirstAsync(wo => wo.Id == seeded.Id, CancellationToken.None);

        var toInProgress = workOrder.UpdateStatus(WorkOrderState.InProgress, DateTimeOffset.UtcNow);
        if (!toInProgress.IsSuccess)
        {
            throw new InvalidOperationException($"Failed to transition work order to InProgress: {toInProgress.TopError?.Code}");
        }

        var toCompleted = workOrder.UpdateStatus(WorkOrderState.Completed, DateTimeOffset.UtcNow);
        if (!toCompleted.IsSuccess)
        {
            throw new InvalidOperationException($"Failed to transition work order to Completed: {toCompleted.TopError?.Code}");
        }

        await context.SaveChangesAsync(CancellationToken.None);

        return workOrder;
    }

    protected async Task<Employee> SeedLaborAsync()
    {
        var context = Factory.CreateDbContext();
        var labor = EmployeeFactory.CreateLabor().Value;
        context.Employees.Add(labor);
        await context.SaveChangesAsync(CancellationToken.None);
        return labor;
    }

    protected async Task<RepairTask> SeedRepairTaskAsync()
    {
        var context = Factory.CreateDbContext();
        var id = Guid.NewGuid().ToString("N")[..8];
        var repairTask = RepairTaskFactory.Create(name: $"Task-{id}").Value;
        context.RepairTasks.Add(repairTask);
        await context.SaveChangesAsync(CancellationToken.None);
        return repairTask;
    }

    // Seeds a Scheduled work order whose start time is yesterday at 10 AM local time.
    protected async Task<(WorkOrder workOrder, RepairTask repairTask)> SeedScheduledWorkOrderBusinessHoursAsync()
    {
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

        // Yesterday at 10 AM in the shop's timezone (Africa/Cairo) → always in the past and within operating hours (09:00–17:00)
        var shopTz = TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");
        var shopNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, shopTz);
        var yesterdayAt10 = shopNow.Date.AddDays(-1).AddHours(10);
        var startAt = new DateTimeOffset(yesterdayAt10, shopTz.GetUtcOffset(yesterdayAt10));

        var workOrder = WorkOrder.Create(
            vehicle.Id,
            startAt,
            startAt.AddHours(1),
            labor.Id,
            Spot.D,
            [repairTask]).Value;

        context.WorkOrders.Add(workOrder);
        await context.SaveChangesAsync(CancellationToken.None);

        return (workOrder, repairTask);
    }
}
