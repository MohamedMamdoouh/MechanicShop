using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Enum;
using MechanicShop.Tests.Common.RepairTasks;
using MechanicShop.Tests.Common.WorkOrders;
using Xunit;
namespace MechanicShop.Domain.UnitTests.WorkOrders;

public class WorkOrderTests
{
    private static readonly DateTimeOffset PastStart = DateTimeOffset.UtcNow.AddHours(-2);
    private static readonly DateTimeOffset FutureEnd = DateTimeOffset.UtcNow.AddHours(2);

    private static WorkOrder CreateScheduled() =>
        WorkOrderFactory.Create(startAtUtc: PastStart, endAtUtc: FutureEnd).Value;

    private static WorkOrder CreateInProgress()
    {
        var wo = CreateScheduled();
        wo.UpdateStatus(WorkOrderState.InProgress, DateTimeOffset.UtcNow);
        return wo;
    }

    private static WorkOrder CreateCompleted()
    {
        var wo = CreateInProgress();
        wo.UpdateStatus(WorkOrderState.Completed, DateTimeOffset.UtcNow);
        return wo;
    }

    private static WorkOrder CreateCancelled()
    {
        var wo = CreateScheduled();
        wo.UpdateStatus(WorkOrderState.Cancelled, DateTimeOffset.UtcNow);
        return wo;
    }

    // --- Create ---

    [Fact]
    public void Create_ShouldSucceed_WithValidData()
    {
        var result = WorkOrderFactory.Create();
        Assert.True(result.IsSuccess);
        Assert.Equal(WorkOrderState.Scheduled, result.Value.Status);
        Assert.Single(result.Value.RepairTasks);
    }

    [Fact]
    public void Create_ShouldFail_WithEmptyVehicleId()
    {
        var result = WorkOrderFactory.Create(vehicleId: Guid.Empty);
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "WorkOrder.VehicleId.Required");
    }

    [Fact]
    public void Create_ShouldFail_WithEmptyLaborId()
    {
        var result = WorkOrderFactory.Create(laborId: Guid.Empty);
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "WorkOrder.EmployeeId.Required");
    }

    [Fact]
    public void Create_ShouldFail_WithNoRepairTasks()
    {
        var result = WorkOrderFactory.Create(repairTasks: []);
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "WorkOrder.RepairTasks.Required");
    }

    [Fact]
    public void Create_ShouldFail_WithStartAtOrAfterEnd()
    {
        var now = DateTimeOffset.UtcNow;
        var result = WorkOrderFactory.Create(startAtUtc: now, endAtUtc: now);
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "WorkOrder.Schedule.Invalid");
    }

    [Fact]
    public void Create_ShouldFail_WithStartAfterEnd()
    {
        var result = WorkOrderFactory.Create(
            startAtUtc: DateTimeOffset.UtcNow.AddHours(2),
            endAtUtc: DateTimeOffset.UtcNow.AddHours(1));
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "WorkOrder.Schedule.Invalid");
    }

    [Fact]
    public void Create_ShouldFail_WithInvalidSpot()
    {
        var result = WorkOrderFactory.Create(spot: (Spot)999);
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "WorkOrder.Spot.Invalid");
    }

    [Fact]
    public void Create_ShouldFail_WithNegativeDiscount()
    {
        var result = WorkOrderFactory.Create(discount: -1m);
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "WorkOrder.Discount.Invalid");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Create_ShouldFail_WithInvalidTaxPercentage(decimal tax)
    {
        var result = WorkOrderFactory.Create(taxPercentage: tax);
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "WorkOrder.Tax.Invalid");
    }

    [Fact]
    public void Create_ShouldFail_WithDiscountExceedingSubtotal()
    {
        // Default task: labor=100, part cost=100*1=100 → TotalCost=200
        var result = WorkOrderFactory.Create(discount: 201m);
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "WorkOrder.Discount.ExceedsSubtotal");
    }

    [Fact]
    public void Create_ShouldSucceed_WithDiscountEqualToSubtotal()
    {
        // subtotal = 200, discount = 200 is allowed (not strictly greater)
        var result = WorkOrderFactory.Create(discount: 200m);
        Assert.True(result.IsSuccess);
    }

    // --- TotalCost ---

    [Fact]
    public void TotalCost_ShouldCalculateCorrectly()
    {
        // labor=100, part cost=100*1=100 → TotalParts=100, TotalLabor=100
        // discount=10, tax=10% → Tax=(100+100-10)*10/100=19, TotalCost=100+100-10+19=209
        var wo = WorkOrderFactory.Create(discount: 10m, taxPercentage: 10m).Value;
        Assert.Equal(100m, wo.TotalPartsCost);
        Assert.Equal(100m, wo.TotalLaborCost);
        Assert.Equal(19m, wo.Tax);
        Assert.Equal(209m, wo.TotalCost);
    }

    // --- AddRepairTask ---

    [Fact]
    public void AddRepairTask_ShouldSucceed()
    {
        var wo = CreateScheduled();
        var task = RepairTaskFactory.Create().Value;
        var result = wo.AddRepairTask(task);
        Assert.True(result.IsSuccess);
        Assert.Equal(2, wo.RepairTasks.Count);
    }

    [Fact]
    public void AddRepairTask_ShouldFail_WhenTaskAlreadyAdded()
    {
        var wo = CreateScheduled();
        var existingTask = wo.RepairTasks.First();
        var result = wo.AddRepairTask(existingTask);
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "WorkOrder.RepairTask.AlreadyAdded");
    }

    [Theory]
    [MemberData(nameof(NonEditableWorkOrders))]
    public void AddRepairTask_ShouldFail_WhenNotEditable(WorkOrder wo)
    {
        var task = RepairTaskFactory.Create().Value;
        var result = wo.AddRepairTask(task);
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "WorkOrder.ReadOnly");
    }

    // --- UpdateTiming ---

    [Fact]
    public void UpdateTiming_ShouldSucceed()
    {
        var wo = CreateScheduled();
        var newStart = DateTimeOffset.UtcNow.AddHours(-1);
        var newEnd = DateTimeOffset.UtcNow.AddHours(3);
        var result = wo.UpdateTiming(newStart, newEnd);
        Assert.True(result.IsSuccess);
        Assert.Equal(newStart, wo.StartAtUtc);
        Assert.Equal(newEnd, wo.EndAtUtc);
    }

    [Fact]
    public void UpdateTiming_ShouldFail_WhenStartAtOrAfterEnd()
    {
        var wo = CreateScheduled();
        var now = DateTimeOffset.UtcNow;
        var result = wo.UpdateTiming(now, now);
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "WorkOrder.Schedule.Invalid");
    }

    [Theory]
    [MemberData(nameof(NonEditableWorkOrders))]
    public void UpdateTiming_ShouldFail_WhenNotEditable(WorkOrder wo)
    {
        var result = wo.UpdateTiming(PastStart, FutureEnd);
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "WorkOrder.ReadOnly");
    }

    // --- UpdateSpot ---

    [Fact]
    public void UpdateSpot_ShouldSucceed()
    {
        var wo = CreateScheduled();
        var result = wo.UpdateSpot(Spot.C);
        Assert.True(result.IsSuccess);
        Assert.Equal(Spot.C, wo.Spot);
    }

    [Fact]
    public void UpdateSpot_ShouldFail_WithInvalidSpot()
    {
        var wo = CreateScheduled();
        var result = wo.UpdateSpot((Spot)999);
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "WorkOrder.Spot.Invalid");
    }

    [Theory]
    [MemberData(nameof(NonEditableWorkOrders))]
    public void UpdateSpot_ShouldFail_WhenNotEditable(WorkOrder wo)
    {
        var result = wo.UpdateSpot(Spot.B);
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "WorkOrder.ReadOnly");
    }

    // --- UpdateLabor ---

    [Fact]
    public void UpdateLabor_ShouldSucceed()
    {
        var wo = CreateScheduled();
        var newLaborId = Guid.NewGuid();
        var result = wo.UpdateLabor(newLaborId);
        Assert.True(result.IsSuccess);
        Assert.Equal(newLaborId, wo.LaborId);
    }

    [Fact]
    public void UpdateLabor_ShouldFail_WithEmptyId()
    {
        var wo = CreateScheduled();
        var result = wo.UpdateLabor(Guid.Empty);
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "WorkOrder.EmployeeId.Required");
    }

    [Theory]
    [MemberData(nameof(NonEditableWorkOrders))]
    public void UpdateLabor_ShouldFail_WhenNotEditable(WorkOrder wo)
    {
        var result = wo.UpdateLabor(Guid.NewGuid());
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "WorkOrder.ReadOnly");
    }

    // --- UpdateStatus ---

    [Theory]
    [InlineData(WorkOrderState.InProgress)]
    [InlineData(WorkOrderState.Cancelled)]
    public void UpdateStatus_ShouldSucceed_FromScheduled(WorkOrderState target)
    {
        var wo = CreateScheduled();
        var result = wo.UpdateStatus(target, DateTimeOffset.UtcNow);
        Assert.True(result.IsSuccess);
        Assert.Equal(target, wo.Status);
    }

    [Theory]
    [InlineData(WorkOrderState.Completed)]
    [InlineData(WorkOrderState.Cancelled)]
    public void UpdateStatus_ShouldSucceed_FromInProgress(WorkOrderState target)
    {
        var wo = CreateInProgress();
        var result = wo.UpdateStatus(target, DateTimeOffset.UtcNow);
        Assert.True(result.IsSuccess);
        Assert.Equal(target, wo.Status);
    }

    [Fact]
    public void UpdateStatus_ShouldFail_WithInvalidTransition()
    {
        var wo = CreateCompleted();
        var result = wo.UpdateStatus(WorkOrderState.InProgress, DateTimeOffset.UtcNow);
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "WorkOrder.Status.InvalidTransition");
    }

    [Fact]
    public void UpdateStatus_ShouldFail_WhenBeforeStartTime()
    {
        var wo = WorkOrderFactory.Create(
            startAtUtc: DateTimeOffset.UtcNow.AddHours(2),
            endAtUtc: DateTimeOffset.UtcNow.AddHours(4)).Value;
        var result = wo.UpdateStatus(WorkOrderState.InProgress, DateTimeOffset.UtcNow);
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "WorkOrder.Status.CannotUpdateBeforeStartTime");
    }

    [Theory]
    [InlineData(WorkOrderState.Completed)]
    [InlineData(WorkOrderState.Cancelled)]
    public void UpdateStatus_ShouldSetEndAtUtc_WhenTransitioningToTerminalState(WorkOrderState target)
    {
        var wo = CreateInProgress();
        var now = DateTimeOffset.UtcNow;
        wo.UpdateStatus(target, now);
        Assert.Equal(now, wo.EndAtUtc);
    }

    // --- Cancel ---

    [Fact]
    public void Cancel_ShouldSucceed_WhenScheduled()
    {
        var wo = CreateScheduled();
        var result = wo.Cancel(DateTimeOffset.UtcNow);
        Assert.True(result.IsSuccess);
        Assert.Equal(WorkOrderState.Cancelled, wo.Status);
    }

    [Fact]
    public void Cancel_ShouldSucceed_WhenInProgress()
    {
        var wo = CreateInProgress();
        var result = wo.Cancel(DateTimeOffset.UtcNow);
        Assert.True(result.IsSuccess);
        Assert.Equal(WorkOrderState.Cancelled, wo.Status);
    }

    // --- ClearRepairTasks ---

    [Fact]
    public void ClearRepairTasks_ShouldSucceed()
    {
        var wo = CreateScheduled();
        var result = wo.ClearRepairTasks();
        Assert.True(result.IsSuccess);
        Assert.Empty(wo.RepairTasks);
        Assert.Equal(0m, wo.Discount);
    }

    [Theory]
    [MemberData(nameof(NonEditableWorkOrders))]
    public void ClearRepairTasks_ShouldFail_WhenNotEditable(WorkOrder wo)
    {
        var result = wo.ClearRepairTasks();
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "WorkOrder.ReadOnly");
    }

    // --- IsEditable ---

    [Fact]
    public void IsEditable_ShouldBeTrue_WhenScheduled()
    {
        var wo = CreateScheduled();
        Assert.True(wo.IsEditable);
    }

    [Fact]
    public void IsEditable_ShouldBeTrue_WhenInProgress()
    {
        var wo = CreateInProgress();
        Assert.True(wo.IsEditable);
    }

    [Fact]
    public void IsEditable_ShouldBeFalse_WhenCompleted()
    {
        var wo = CreateCompleted();
        Assert.False(wo.IsEditable);
    }

    [Fact]
    public void IsEditable_ShouldBeFalse_WhenCancelled()
    {
        var wo = CreateCancelled();
        Assert.False(wo.IsEditable);
    }

    // --- EnsureCanBeInvoiced ---

    [Fact]
    public void EnsureCanBeInvoiced_ShouldSucceed_WhenCompleted()
    {
        var wo = CreateCompleted();
        var result = wo.EnsureCanBeInvoiced();
        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData(WorkOrderState.Scheduled)]
    [InlineData(WorkOrderState.InProgress)]
    [InlineData(WorkOrderState.Cancelled)]
    public void EnsureCanBeInvoiced_ShouldFail_WhenNotCompleted(WorkOrderState state)
    {
        WorkOrder wo = state switch
        {
            WorkOrderState.InProgress => CreateInProgress(),
            WorkOrderState.Cancelled => CreateCancelled(),
            _ => CreateScheduled()
        };
        var result = wo.EnsureCanBeInvoiced();
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "WorkOrder.NotCompleted");
    }

    // --- CanTransitionTo ---

    [Theory]
    [InlineData(WorkOrderState.Scheduled, WorkOrderState.InProgress, true)]
    [InlineData(WorkOrderState.Scheduled, WorkOrderState.Cancelled, true)]
    [InlineData(WorkOrderState.Scheduled, WorkOrderState.Completed, false)]
    [InlineData(WorkOrderState.InProgress, WorkOrderState.Completed, true)]
    [InlineData(WorkOrderState.InProgress, WorkOrderState.Cancelled, true)]
    [InlineData(WorkOrderState.InProgress, WorkOrderState.Scheduled, false)]
    public void CanTransitionTo_ShouldReturnExpectedResult(
        WorkOrderState from, WorkOrderState to, bool expected)
    {
        var wo = from == WorkOrderState.InProgress ? CreateInProgress() : CreateScheduled();
        Assert.Equal(expected, wo.CanTransitionTo(to));
    }

    [Theory]
    [InlineData(WorkOrderState.Completed)]
    [InlineData(WorkOrderState.Cancelled)]
    public void CanTransitionTo_ShouldReturnFalse_FromTerminalState(WorkOrderState terminal)
    {
        WorkOrder wo = terminal == WorkOrderState.Completed ? CreateCompleted() : CreateCancelled();
        Assert.False(wo.CanTransitionTo(WorkOrderState.Scheduled));
        Assert.False(wo.CanTransitionTo(WorkOrderState.InProgress));
    }

    // --- MemberData ---

    public static IEnumerable<object[]> NonEditableWorkOrders()
    {
        yield return [CreateCompleted()];
        yield return [CreateCancelled()];
    }
}
