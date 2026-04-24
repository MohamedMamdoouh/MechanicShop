using MechanicShop.Application.Features.Billing.Commands.IssueInvoice;
using MechanicShop.Application.Features.Billing.Commands.SettleInvoice;
using MechanicShop.Application.SubcutaneousTests.Common;
using Xunit;
namespace MechanicShop.Application.SubcutaneousTests.Features.Billing.Commands;

[Collection(WebAppFactoryCollection.CollectionName)]
public class SettleInvoiceTests(WebAppFactory factory) : BillingTestBase(factory)
{
    [Fact]
    public async Task Handle_WhenInvoiceIsUnpaid_ReturnsSuccess()
    {
        var workOrder = await SeedCompletedWorkOrderAsync();
        var issued = await Mediator.Send(new IssueInvoiceCommand(workOrder.Id));

        var result = await Mediator.Send(new SettleInvoiceCommand(issued.Value.InvoiceId));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_WhenInvoiceNotFound_ReturnsNotFoundError()
    {
        var result = await Mediator.Send(new SettleInvoiceCommand(Guid.NewGuid()));

        Assert.False(result.IsSuccess);
        Assert.Equal("ApplicationErrors.Invoice.NotFound", result.TopError!.Value.Code);
    }

    [Fact]
    public async Task Handle_WhenAlreadyPaid_ReturnsError()
    {
        var workOrder = await SeedCompletedWorkOrderAsync();
        var issued = await Mediator.Send(new IssueInvoiceCommand(workOrder.Id));
        var invoiceId = issued.Value.InvoiceId;
        await Mediator.Send(new SettleInvoiceCommand(invoiceId));

        var result = await Mediator.Send(new SettleInvoiceCommand(invoiceId));

        Assert.False(result.IsSuccess);
        Assert.Equal("Invoice.AlreadyPaid", result.TopError!.Value.Code);
    }
}
