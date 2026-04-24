using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Billing;
namespace MechanicShop.Tests.Common.Billing;

public static class InvoiceLineItemFactory
{
    public static Result<InvoiceLineItem> Create(
        Guid? invoiceId = null,
        int? lineNumber = null,
        string? description = null,
        int? quantity = null,
        decimal? unitPrice = null)
    {
        return InvoiceLineItem.Create(
            invoiceId ?? Guid.NewGuid(),
            lineNumber ?? 1,
            description ?? "Test Line Item",
            quantity ?? 1,
            unitPrice ?? 10.00m);
    }
}