using MechanicShop.Domain.Common.BaseEntities;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Domain.Employees;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.WorkOrders.Billing;
using MechanicShop.Domain.WorkOrders.Enum;
using MechanicShop.Domain.WorkOrders.Events;
namespace MechanicShop.Domain.WorkOrders;

public class WorkOrder : AuditableEntity
{
    public Guid VehicleId { get; }
    public DateTimeOffset StartAtUtc { get; private set; }
    public DateTimeOffset EndAtUtc { get; private set; }
    public Guid LaborId { get; private set; }
    public Spot Spot { get; private set; }
    public WorkOrderState Status { get; private set; }

    public Vehicle Vehicle { get; } = null!;
    public Employee Labor { get; } = null!;
    public Invoice Invoice { get; } = null!;

    public decimal Discount { get; private set; }
    public decimal TaxPercentage { get; }

    public decimal Tax => (TotalPartsCost + TotalLaborCost - Discount) * TaxPercentage / 100m;
    public decimal TotalPartsCost => _repairTasks.Sum(rt => rt.TotalPartsCost);
    public decimal TotalLaborCost => _repairTasks.Sum(rt => rt.LaborCost);
    public decimal TotalCost => TotalPartsCost + TotalLaborCost - Discount + Tax;

    private readonly List<RepairTask> _repairTasks = [];
    public IReadOnlyCollection<RepairTask> RepairTasks => _repairTasks.AsReadOnly();

    private WorkOrder() { }

    private WorkOrder(
        Guid vehicleId,
        DateTimeOffset startAtUtc,
        DateTimeOffset endAtUtc,
        Guid laborId,
        Spot spot,
        List<RepairTask> repairTasks,
        decimal discount = 0,
        decimal taxPercentage = 0)
    {
        VehicleId = vehicleId;
        StartAtUtc = startAtUtc;
        EndAtUtc = endAtUtc;
        LaborId = laborId;
        Spot = spot;
        Status = WorkOrderState.Scheduled;
        _repairTasks = repairTasks ?? [];
        Discount = discount;
        TaxPercentage = taxPercentage;
    }

    public static Result<WorkOrder> Create(
        Guid vehicleId,
        DateTimeOffset startAtUtc,
        DateTimeOffset endAtUtc,
        Guid laborId,
        Spot spot,
        List<RepairTask> repairTasks,
        decimal discount = 0,
        decimal taxPercentage = 0)
    {
        var errors = new List<Error>();

        if (vehicleId == Guid.Empty)
        {
            errors.Add(WorkOrderErrors.VehicleIdRequired);
        }

        if (repairTasks == null || repairTasks.Count == 0)
        {
            errors.Add(WorkOrderErrors.RepairTasksRequired);
        }

        if (laborId == Guid.Empty)
        {
            errors.Add(WorkOrderErrors.EmployeeIdRequired);
        }

        if (startAtUtc >= endAtUtc)
        {
            errors.Add(WorkOrderErrors.InvalidStartEndTime);
        }

        if (!System.Enum.IsDefined(spot))
        {
            errors.Add(WorkOrderErrors.SpotInvalid);
        }

        if (discount < 0)
        {
            errors.Add(WorkOrderErrors.DiscountInvalid);
        }

        if (taxPercentage < 0 || taxPercentage > 100)
        {
            errors.Add(WorkOrderErrors.TaxInvalid);
        }

        if (repairTasks is not null)
        {
            var subtotal = repairTasks.Sum(rt => rt.TotalCost);
            if (discount > subtotal)
            {
                errors.Add(WorkOrderErrors.DiscountExceedsSubtotal);
            }
        }

        if (errors.Count > 0)
        {
            return errors;
        }

        return new WorkOrder(
            vehicleId,
            startAtUtc,
            endAtUtc,
            laborId,
            spot,
            repairTasks!,
            discount,
            taxPercentage);
    }

    public Result<Updated> AddRepairTask(RepairTask repairTask)
    {
        var errors = new List<Error>();

        if (!IsEditable)
        {
            errors.Add(WorkOrderErrors.ReadOnly);
        }

        if (repairTask == null)
        {
            errors.Add(WorkOrderErrors.RepairTaskRequired);
        }

        if (_repairTasks.Exists(rt => rt.Id == repairTask?.Id))
        {
            errors.Add(WorkOrderErrors.RepairTaskAlreadyAdded);
        }

        if (errors.Count > 0)
        {
            return errors;
        }

        _repairTasks.Add(repairTask!);
        AddDomainEvent(new WorkOrderCollectionModified(Id));

        return Result.Updated;
    }

    public Result<Updated> UpdateTiming(DateTimeOffset startAtUtc, DateTimeOffset endAtUtc)
    {
        var errors = new List<Error>();

        if (!IsEditable)
        {
            errors.Add(WorkOrderErrors.ReadOnly);
        }

        if (startAtUtc >= endAtUtc)
        {
            errors.Add(WorkOrderErrors.InvalidStartEndTime);
        }

        if (errors.Count > 0)
        {
            return errors;
        }

        StartAtUtc = startAtUtc;
        EndAtUtc = endAtUtc;
        return Result.Updated;
    }

    public Result<Updated> UpdateSpot(Spot spot)
    {
        var errors = new List<Error>();

        if (!IsEditable)
        {
            errors.Add(WorkOrderErrors.ReadOnly);
        }

        if (!System.Enum.IsDefined(spot))
        {
            errors.Add(WorkOrderErrors.SpotInvalid);
        }

        if (errors.Count > 0)
        {
            return errors;
        }

        Spot = spot;
        AddDomainEvent(new WorkOrderCollectionModified(Id));

        return Result.Updated;
    }

    public Result<Updated> UpdateLabor(Guid laborId)
    {
        var errors = new List<Error>();

        if (!IsEditable)
        {
            errors.Add(WorkOrderErrors.ReadOnly);
        }

        if (laborId == Guid.Empty)
        {
            errors.Add(WorkOrderErrors.EmployeeIdRequired);
        }

        if (errors.Count > 0)
        {
            return errors;
        }

        LaborId = laborId;
        AddDomainEvent(new WorkOrderCollectionModified(Id));

        return Result.Updated;
    }

    public Result<Updated> UpdateStatus(WorkOrderState newState, DateTimeOffset now)
    {
        var errors = new List<Error>();

        if (StartAtUtc > now)
        {
            errors.Add(WorkOrderErrors.CannotUpdateStatusBeforeStartTime);
        }

        if (!CanTransitionTo(newState))
        {
            errors.Add(WorkOrderErrors.InvalidStatusTransition);
        }

        if (errors.Count > 0)
        {
            return errors;
        }

        Status = newState;

        if (newState is WorkOrderState.Completed or WorkOrderState.Cancelled)
        {
            EndAtUtc = now;
        }

        if (newState is WorkOrderState.Completed)
        {
            AddDomainEvent(new WorkOrderCompleted { WorkOrderId = Id });
        }

        return Result.Updated;
    }

    public Result<Updated> Cancel(DateTimeOffset now)
    {
        return UpdateStatus(WorkOrderState.Cancelled, now);
    }

    public Result<Updated> ClearRepairTasks()
    {
        if (!IsEditable)
        {
            return WorkOrderErrors.ReadOnly;
        }

        _repairTasks.Clear();
        Discount = 0;

        AddDomainEvent(new WorkOrderCollectionModified(Id));

        return Result.Updated;
    }

    public Result<Success> EnsureCanBeInvoiced()
    {
        var errors = new List<Error>();

        if (Status != WorkOrderState.Completed)
        {
            errors.Add(WorkOrderErrors.WorkOrderNotCompleted);
        }

        if (Invoice is not null)
        {
            errors.Add(WorkOrderErrors.InvoiceAlreadyExists);
        }

        if (errors.Count > 0)
        {
            return errors;
        }

        return Result.Success;
    }

    public bool IsEditable => Status is WorkOrderState.Scheduled or WorkOrderState.InProgress;

    public bool CanTransitionTo(WorkOrderState newState)
    {
        return Status switch
        {
            WorkOrderState.Scheduled => newState is WorkOrderState.InProgress or WorkOrderState.Cancelled,
            WorkOrderState.InProgress => newState is WorkOrderState.Completed or WorkOrderState.Cancelled,
            _ => false
        };
    }
}