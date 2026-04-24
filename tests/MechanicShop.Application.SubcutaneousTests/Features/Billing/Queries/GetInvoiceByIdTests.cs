using MechanicShop.Application.Features.Billing.Commands.IssueInvoice;
using MechanicShop.Application.Features.Billing.Queries.GetInvoiceById;
using MechanicShop.Application.SubcutaneousTests.Common;
using Xunit;
namespace MechanicShop.Application.SubcutaneousTests.Features.Billing.Queries;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GetInvoiceByIdTests(WebAppFactory factory) : BillingTestBase(factory)
{
    [Fact]
    public async Task Handle_WhenInvoiceExists_ReturnsInvoiceDto()
    {
        var workOrder = await SeedCompletedWorkOrderAsync();
        var issued = await Mediator.Send(new IssueInvoiceCommand(workOrder.Id));

        var result = await Mediator.Send(new GetInvoiceByIdQuery(issued.Value.InvoiceId));

        Assert.True(result.IsSuccess);
        Assert.Equal(issued.Value.InvoiceId, result.Value.InvoiceId);
        Assert.Equal(workOrder.Id, result.Value.WorkOrderId);
        Assert.NotEmpty(result.Value.Items);
    }

    [Fact]
    public async Task Handle_WhenInvoiceNotFound_ReturnsNotFoundError()
    {
        var result = await Mediator.Send(new GetInvoiceByIdQuery(Guid.NewGuid()));

        Assert.False(result.IsSuccess);
        Assert.Equal("ApplicationErrors.Invoice.NotFound", result.TopError!.Value.Code);
    }
}
