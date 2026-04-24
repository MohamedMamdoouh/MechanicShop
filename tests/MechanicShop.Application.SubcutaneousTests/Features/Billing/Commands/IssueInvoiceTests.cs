using MechanicShop.Application.Features.Billing.Commands.IssueInvoice;
using MechanicShop.Application.SubcutaneousTests.Common;
using Xunit;
namespace MechanicShop.Application.SubcutaneousTests.Features.Billing.Commands;

[Collection(WebAppFactoryCollection.CollectionName)]
public class IssueInvoiceTests(WebAppFactory factory) : BillingTestBase(factory)
{
    [Fact]
    public async Task Handle_WhenWorkOrderIsCompleted_ReturnsInvoiceDto()
    {
        var workOrder = await SeedCompletedWorkOrderAsync();

        var result = await Mediator.Send(new IssueInvoiceCommand(workOrder.Id));

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.InvoiceId);
        Assert.Equal(workOrder.Id, result.Value.WorkOrderId);
        Assert.Equal("Unpaid", result.Value.PaymentStatus);
        Assert.NotEmpty(result.Value.Items);
    }

    [Fact]
    public async Task Handle_WhenWorkOrderNotFound_ReturnsNotFoundError()
    {
        var result = await Mediator.Send(new IssueInvoiceCommand(Guid.NewGuid()));

        Assert.False(result.IsSuccess);
        Assert.Equal("ApplicationErrors.WorkOrder.NotFound", result.TopError!.Value.Code);
    }

    [Fact]
    public async Task Handle_WhenWorkOrderNotCompleted_ReturnsError()
    {
        var workOrder = await SeedScheduledWorkOrderAsync();

        var result = await Mediator.Send(new IssueInvoiceCommand(workOrder.Id));

        Assert.False(result.IsSuccess);
        Assert.Equal("WorkOrder.NotCompleted", result.TopError!.Value.Code);
    }

    [Fact]
    public async Task Handle_WhenAlreadyInvoiced_ReturnsConflictError()
    {
        var workOrder = await SeedCompletedWorkOrderAsync();
        await Mediator.Send(new IssueInvoiceCommand(workOrder.Id));

        var result = await Mediator.Send(new IssueInvoiceCommand(workOrder.Id));

        Assert.False(result.IsSuccess);
        Assert.Equal("WorkOrder.Invoice.AlreadyExists", result.TopError!.Value.Code);
    }
}
