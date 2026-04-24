using MechanicShop.Application.Features.RepairTasks.Mappers;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.RepairTasks.Parts;
using MechanicShop.Tests.Common.RepairTasks;
using Xunit;

namespace MechanicShop.Application.UnitTests.Mappers;

public class RepairTaskMapperTests
{
    // --- Part.ToDto ---

    [Fact]
    public void PartToDto_ShouldMapAllFieldsCorrectly()
    {
        var part = PartFactory.Create(name: "Oil Filter", cost: 25.50m, quantity: 2).Value;

        var dto = part.ToDto();

        Assert.Equal(part.Id, dto.PartId);
        Assert.Equal("Oil Filter", dto.Name);
        Assert.Equal(25.50m, dto.Cost);
        Assert.Equal(2, dto.Quantity);
    }

    [Fact]
    public void PartListToDto_ShouldMapAllPartsCorrectly()
    {
        var p1 = PartFactory.Create(name: "Brake Pad").Value;
        var p2 = PartFactory.Create(name: "Air Filter").Value;

        var dtos = new List<Part> { p1, p2 }.ToDto();

        Assert.Equal(2, dtos.Count);
        Assert.Equal(p1.Id, dtos[0].PartId);
        Assert.Equal(p2.Id, dtos[1].PartId);
    }

    [Fact]
    public void PartListToDto_ShouldReturnEmpty_WhenSourceIsEmpty()
    {
        var dtos = new List<Part>().ToDto();
        Assert.Empty(dtos);
    }

    // --- RepairTask.ToDto ---

    [Fact]
    public void RepairTaskToDto_ShouldMapAllFieldsCorrectly()
    {
        var part = PartFactory.Create(name: "Spark Plug", cost: 10m, quantity: 4).Value;
        var repairTask = RepairTaskFactory.Create(
            name: "Tune Up",
            laborCost: 80m,
            parts: [part]).Value;

        var dto = repairTask.ToDto();

        Assert.Equal(repairTask.Id, dto.RepairTaskId);
        Assert.Equal("Tune Up", dto.Name);
        Assert.Equal(80m, dto.LaborCost);
        Assert.Equal(repairTask.EstimatedRepairDurationMinutes, dto.RepairDurationMinutes);
        Assert.Equal(repairTask.TotalCost, dto.TotalCost);
        Assert.Single(dto.Parts);
        Assert.Equal(part.Id, dto.Parts[0].PartId);
    }

    [Fact]
    public void RepairTaskListToDto_ShouldMapAllRepairTasksCorrectly()
    {
        var rt1 = RepairTaskFactory.Create(name: "Oil Change").Value;
        var rt2 = RepairTaskFactory.Create(name: "Tire Rotation").Value;

        var dtos = new List<RepairTask> { rt1, rt2 }.ToDto();

        Assert.Equal(2, dtos.Count);
        Assert.Equal(rt1.Id, dtos[0].RepairTaskId);
        Assert.Equal(rt2.Id, dtos[1].RepairTaskId);
    }

    [Fact]
    public void RepairTaskListToDto_ShouldReturnEmpty_WhenSourceIsEmpty()
    {
        var dtos = new List<RepairTask>().ToDto();
        Assert.Empty(dtos);
    }
}