using MechanicShop.Domain.RepairTasks.Enum;
using MechanicShop.Tests.Common.RepairTasks;
using Xunit;
namespace MechanicShop.Domain.UnitTests.RepairTasks;

public class RepairTaskTests
{
    [Fact]
    public void Create_ShouldSucceed_WithValidData()
    {
        var result = RepairTaskFactory.Create();
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value.Name);
    }

    [Fact]
    public void Update_ShouldSucceed_WithValidData()
    {
        var task = RepairTaskFactory.Create().Value;
        var updateResult = task.Update("Updated Task", 150.00m, RepairDurationMinutes.Min60);
        Assert.True(updateResult.IsSuccess);
    }

    [Fact]
    public void UpsertParts_ShouldSucceed_WithValidParts()
    {
        var task = RepairTaskFactory.Create().Value;
        var part = PartFactory.Create(name: "New Part").Value;
        var result = task.UpsertParts([part]);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Update_ShouldReturnError_WithInvalidDuration()
    {
        var task = RepairTaskFactory.Create().Value;
        var updateResult = task.Update("Updated Task", 150.00m, (RepairDurationMinutes)999);
        Assert.False(updateResult.IsSuccess);
        Assert.Contains(updateResult.Errors, e => e.Code == "RepairTask.Duration.Invalid");
    }

    [Theory]
    [InlineData("", 100.00, RepairDurationMinutes.Min60, false)]
    [InlineData("Valid Name", -50.00, RepairDurationMinutes.Min60, false)]
    [InlineData("Valid Name", 0, RepairDurationMinutes.Min120, false)]
    public void Update_ShouldReturnError_WithInvalidNameOrCost(
        string name,
        decimal cost,
        RepairDurationMinutes duration,
        bool expected)
    {
        var task = RepairTaskFactory.Create().Value;
        var updateResult = task.Update(name, cost, duration);
        Assert.Equal(expected, updateResult.IsSuccess);
    }
}

