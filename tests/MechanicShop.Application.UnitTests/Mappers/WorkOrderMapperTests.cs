using System.Reflection;
using MechanicShop.Application.Features.WorkOrder.Mappers;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.Employees;
using MechanicShop.Tests.Common.RepairTasks;
using MechanicShop.Tests.Common.WorkOrders;
using Xunit;
namespace MechanicShop.Application.UnitTests.Mappers;

public class WorkOrderMapperTests
{
    private static void SetField<T>(T entity, string fieldName, object value) where T : class
    {
        var type = entity.GetType();
        while (type is not null)
        {
            var field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field is not null)
            {
                field.SetValue(entity, value);
                return;
            }
            type = type.BaseType;
        }
        throw new InvalidOperationException($"Field '{fieldName}' not found on type '{entity.GetType().Name}'.");
    }

    private static WorkOrder CreateHydratedWorkOrder()
    {
        var repairTask = RepairTaskFactory.Create().Value;
        var customer = CustomerFactory.CreateCustomer(vehicles: []).Value;
        var vehicle = VehicleFactory.CreateVehicle().Value;
        var labor = EmployeeFactory.CreateEmployee().Value;

        SetField(vehicle, "<Customer>k__BackingField", customer);

        var workOrder = WorkOrderFactory.Create(
            startAtUtc: DateTimeOffset.UtcNow.AddHours(-2),
            endAtUtc: DateTimeOffset.UtcNow.AddHours(2),
            repairTasks: [repairTask]).Value;

        SetField(workOrder, "<Vehicle>k__BackingField", vehicle);
        SetField(workOrder, "<Labor>k__BackingField", labor);

        return workOrder;
    }

    [Fact]
    public void ToDto_ShouldMapAllFieldsCorrectly()
    {
        var workOrder = CreateHydratedWorkOrder();
        var repairTask = workOrder.RepairTasks.First();
        var vehicle = workOrder.Vehicle;
        var labor = workOrder.Labor;

        var dto = workOrder.ToDto();

        Assert.Equal(workOrder.Id, dto.WorkOrderId);
        Assert.Null(dto.InvoiceId);
        Assert.Equal(workOrder.Spot, dto.Spot);
        Assert.Equal(workOrder.Status, dto.Status);
        Assert.Equal(workOrder.StartAtUtc, dto.StartAtUtc);
        Assert.Equal(workOrder.EndAtUtc, dto.EndAtUtc);
        Assert.Equal(workOrder.TotalPartsCost, dto.TotalPartsCost);
        Assert.Equal(workOrder.TotalLaborCost, dto.TotalLaborCost);
        Assert.Equal(workOrder.TotalCost, dto.TotalCost);
        Assert.Equal((int)repairTask.EstimatedRepairDurationMinutes, dto.TotalDurationInMinutes);
        Assert.Equal(workOrder.CreatedAtUtc, dto.CreatedAtUtc);
        Assert.Equal(vehicle.Id, dto.Vehicle.Id);
        Assert.Equal(vehicle.Make, dto.Vehicle.Make);
        Assert.Equal(vehicle.Model, dto.Vehicle.Model);
        Assert.Equal(vehicle.Year, dto.Vehicle.Year);
        Assert.Equal(vehicle.LicensePlate, dto.Vehicle.LicensePlate);
        Assert.Equal(labor.Id, dto.Labor.Id);
        Assert.Equal(labor.FullName, dto.Labor.Name);
        Assert.Single(dto.RepairTasks);
        Assert.Equal(repairTask.Id, dto.RepairTasks[0].RepairTaskId);
    }

    [Fact]
    public void ToDto_ShouldThrow_WhenWorkOrderIsNull()
    {
        WorkOrder? workOrder = null;
        Assert.Throws<ArgumentNullException>(() => workOrder!.ToDto());
    }

    [Fact]
    public void ToDtoList_ShouldMapAllWorkOrdersCorrectly()
    {
        var wo1 = CreateHydratedWorkOrder();
        var wo2 = CreateHydratedWorkOrder();

        var dtos = new List<WorkOrder> { wo1, wo2 }.ToDto();

        Assert.Equal(2, dtos.Count);
        Assert.Equal(wo1.Id, dtos[0].WorkOrderId);
        Assert.Equal(wo2.Id, dtos[1].WorkOrderId);
    }

    [Fact]
    public void ToDtoList_ShouldThrow_WhenWorkOrdersIsNull()
    {
        IEnumerable<WorkOrder>? workOrders = null;
        Assert.Throws<ArgumentNullException>(() => workOrders!.ToDto());
    }

    [Fact]
    public void ToListItemDto_ShouldMapAllFieldsCorrectly()
    {
        var workOrder = CreateHydratedWorkOrder();
        var vehicle = workOrder.Vehicle;
        var labor = workOrder.Labor;
        var repairTask = workOrder.RepairTasks.First();

        var dto = workOrder.ToListItemDto();

        Assert.Equal(workOrder.Id, dto.WorkOrderId);
        Assert.Null(dto.InvoiceId);
        Assert.Equal(workOrder.Status, dto.Status);
        Assert.Equal(workOrder.Spot, dto.Spot);
        Assert.Equal(workOrder.StartAtUtc, dto.StartAtUtc);
        Assert.Equal(workOrder.EndAtUtc, dto.EndAtUtc);
        Assert.Equal(vehicle.Customer.FullName, dto.CustomerName);
        Assert.Equal(labor.FullName, dto.LaborName);
        Assert.Single(dto.RepairTaskNames);
        Assert.Equal(repairTask.Name, dto.RepairTaskNames[0]);
        Assert.Equal(vehicle.Id, dto.Vehicle.Id);
    }

    [Fact]
    public void ToListItemDto_ShouldThrow_WhenWorkOrderIsNull()
    {
        WorkOrder? workOrder = null;
        Assert.Throws<ArgumentNullException>(() => workOrder!.ToListItemDto());
    }
}