using MechanicShop.Domain.RepairTasks.Parts;
using MechanicShop.Tests.Common.RepairTasks;
using Xunit;
namespace MechanicShop.Domain.UnitTests.RepairTasks;

public class PartTests
{
    // --- Create ---

    [Fact]
    public void Create_ShouldSucceed_WithValidData()
    {
        var result = PartFactory.Create();
        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData("", 100.00, 1, "Part.Name.Required")]
    [InlineData("   ", 100.00, 1, "Part.Name.Required")]
    [InlineData("Part", 0, 1, "Part.Cost.Invalid")]
    [InlineData("Part", -10, 1, "Part.Cost.Invalid")]
    [InlineData("Part", 100, 0, "Part.Quantity.Invalid")]
    [InlineData("Part", 100, -1, "Part.Quantity.Invalid")]
    public void Create_ShouldFail_WithInvalidInput(
        string name, decimal cost, int quantity, string expectedCode)
    {
        var result = Part.Create(name, cost, quantity);
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == expectedCode);
    }

    [Fact]
    public void Create_ShouldReturnMultipleErrors_WhenAllFieldsInvalid()
    {
        var result = Part.Create("", 0, 0);
        Assert.False(result.IsSuccess);
        Assert.Equal(3, result.Errors.Count);
    }

    [Fact]
    public void Create_ShouldTrimName()
    {
        var result = Part.Create("  Oil Filter  ", 10m, 2);
        Assert.True(result.IsSuccess);
        Assert.Equal("Oil Filter", result.Value.Name);
    }

    // --- Update ---

    [Fact]
    public void Update_ShouldSucceed_WithValidData()
    {
        var part = PartFactory.Create().Value;
        var result = part.Update("New Name", 50m, 3);
        Assert.True(result.IsSuccess);
        Assert.Equal("New Name", part.Name);
        Assert.Equal(50m, part.Cost);
        Assert.Equal(3, part.Quantity);
    }

    [Theory]
    [InlineData("", 100.00, 1, "Part.Name.Required")]
    [InlineData("Part", 0, 1, "Part.Cost.Invalid")]
    [InlineData("Part", -5, 1, "Part.Cost.Invalid")]
    [InlineData("Part", 100, 0, "Part.Quantity.Invalid")]
    public void Update_ShouldFail_WithInvalidInput(
        string name, decimal cost, int quantity, string expectedCode)
    {
        var part = PartFactory.Create().Value;
        var result = part.Update(name, cost, quantity);
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == expectedCode);
    }

    [Fact]
    public void Update_ShouldNotMutate_WhenValidationFails()
    {
        var part = PartFactory.Create().Value;
        part.Update("", 0, 0);

        // Original values (provided by the factory) should remain unchanged
        Assert.Equal("Test Part", part.Name);
        Assert.Equal(100m, part.Cost);
        Assert.Equal(1, part.Quantity);
    }

    [Fact]
    public void Update_ShouldTrimName()
    {
        var part = PartFactory.Create().Value;
        part.Update("  Brake Pad  ", 20m, 4);
        Assert.Equal("Brake Pad", part.Name);
    }
}
