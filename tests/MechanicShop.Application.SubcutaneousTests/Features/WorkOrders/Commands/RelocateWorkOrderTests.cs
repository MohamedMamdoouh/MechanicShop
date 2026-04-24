using MechanicShop.Application.Features.WorkOrder.Commands.RelocateWorkOrder;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.WorkOrders.Enum;
using Xunit;
namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands;

[Collection(WebAppFactoryCollection.CollectionName)]
public class RelocateWorkOrderTests(WebAppFactory factory) : WorkOrderTestBase(factory)
{
    [Fact]
    public async Task RelocateWorkOrder_ToAvailableSpotAndTime_Succeeds()
    {
        // Arrange
        var workOrder = await SeedScheduledWorkOrderAsync(Spot.A);

        // New start time: far in the future, no other work orders at that time
        var newStartAt = DateTimeOffset.UtcNow.AddDays(30);
        var command = new RelocateWorkOrderCommand(workOrder.Id, newStartAt, Spot.B);

        // Act
        var result = await Mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task RelocateWorkOrder_WithNonExistentId_ReturnsNotFoundError()
    {
        // Arrange
        var command = new RelocateWorkOrderCommand(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddDays(30),
            Spot.A);

        // Act
        var result = await Mediator.Send(command);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "ApplicationErrors.WorkOrder.NotFound");
    }

    [Fact]
    public async Task RelocateWorkOrder_ToOccupiedSpot_ReturnsSpotNotAvailableError()
    {
        // Arrange
        var futureStartAt = DateTimeOffset.UtcNow.AddDays(30);
        var workOrder1 = await SeedScheduledWorkOrderAsync(Spot.A, futureStartAt);
        var workOrder2 = await SeedScheduledWorkOrderAsync(Spot.B, futureStartAt);

        // Relocate workOrder2 to Spot.A at the same time as workOrder1
        var command = new RelocateWorkOrderCommand(
            workOrder2.Id,
            workOrder1.StartAtUtc,
            Spot.A);

        // Act
        var result = await Mediator.Send(command);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "ApplicationErrors.WorkOrder.SpotNotAvailable");
    }
}
