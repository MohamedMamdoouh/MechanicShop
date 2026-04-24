using MechanicShop.Application.Features.Billing.Dtos;
using MechanicShop.Domain.WorkOrders.Billing;
namespace MechanicShop.Application.Common.Interfaces;

public interface IInvoicePdfGenerator
{
    InvoicePdfDto Generate(Invoice invoice);
}