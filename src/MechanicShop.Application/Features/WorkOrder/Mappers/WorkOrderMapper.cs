using MechanicShop.Application.Features.Customer.Dtos;
using MechanicShop.Application.Features.Customer.Mappers;
using MechanicShop.Application.Features.Labor.Mappers;
using MechanicShop.Application.Features.RepairTasks.Mappers;
using MechanicShop.Application.Features.WorkOrder.Dtos;
namespace MechanicShop.Application.Features.WorkOrder.Mappers;

public static class WorkOrderMapper
{
    public static WorkOrderDto ToDto(this Domain.WorkOrders.WorkOrder workOrder)
    {
        ArgumentNullException.ThrowIfNull(workOrder);

        return new WorkOrderDto
        {
            WorkOrderId = workOrder.Id,
            InvoiceId = workOrder.Invoice?.Id,
            Spot = workOrder.Spot,
            Vehicle = workOrder.Vehicle.ToDto(),
            StartAtUtc = workOrder.StartAtUtc,
            EndAtUtc = workOrder.EndAtUtc,
            RepairTasks = workOrder.RepairTasks.ToList().ToDto(),
            Labor = workOrder.Labor.ToDto(),
            Status = workOrder.Status,
            TotalPartsCost = workOrder.TotalPartsCost,
            TotalLaborCost = workOrder.TotalLaborCost,
            TotalCost = workOrder.TotalCost,
            TotalDurationInMinutes = workOrder.RepairTasks.Sum(rt => (int)rt.EstimatedRepairDurationMinutes),
            CreatedAtUtc = workOrder.CreatedAtUtc
        };
    }

    public static List<WorkOrderDto> ToDto(this IEnumerable<Domain.WorkOrders.WorkOrder> workOrders)
    {
        ArgumentNullException.ThrowIfNull(workOrders);

        return [.. workOrders.Select(wo => wo.ToDto())];
    }

    public static WorkOrderListItemDto ToListItemDto(this Domain.WorkOrders.WorkOrder workOrder)
    {
        ArgumentNullException.ThrowIfNull(workOrder);

        return new WorkOrderListItemDto
        {
            WorkOrderId = workOrder.Id,
            InvoiceId = workOrder.Invoice?.Id ?? null,
            Vehicle = workOrder.Vehicle.ToDto(),
            CustomerName = workOrder.Vehicle.Customer.FullName,
            LaborName = workOrder.Labor.FullName,
            Status = workOrder.Status,
            Spot = workOrder.Spot,
            StartAtUtc = workOrder.StartAtUtc,
            EndAtUtc = workOrder.EndAtUtc,
            RepairTaskNames = [.. workOrder.RepairTasks.Select(rt => rt.Name)]
        };
    }

    // We cannot use IQueryable extension methods in the ToDto() method.
    // EF Core does not support projecting to complex types with nested collections.
    public static IQueryable<WorkOrderListItemDto> ToListItemDto(this IQueryable<Domain.WorkOrders.WorkOrder> workOrders)
    {
        ArgumentNullException.ThrowIfNull(workOrders);

        return workOrders.Select(wo => new WorkOrderListItemDto
        {
            WorkOrderId = wo.Id,
            InvoiceId = wo.Invoice == null ? null : wo.Invoice.Id,
            Vehicle = new VehicleDto(wo.Vehicle.Id, wo.Vehicle.Make, wo.Vehicle.Model, wo.Vehicle.Year, wo.Vehicle.LicensePlate),
            CustomerName = wo.Vehicle.Customer.FullName,
            LaborName = wo.Labor.FullName,
            Status = wo.Status,
            Spot = wo.Spot,
            StartAtUtc = wo.StartAtUtc,
            EndAtUtc = wo.EndAtUtc,
            RepairTaskNames = wo.RepairTasks.Select(rt => rt.Name).ToList()
        });
    }
}