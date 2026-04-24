using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Billing;
using MechanicShop.Tests.Common.WorkOrders;
namespace MechanicShop.Tests.Common.Billing;

public static class InvoiceFactory
{
    public static Result<Invoice> Create()
    {
        return Invoice.CreateFromWorkOrder(WorkOrderFactory.Create().Value);
    }
}