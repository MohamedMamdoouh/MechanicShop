using MechanicShop.Application.Features.Billing.Dtos;
using MechanicShop.Application.Features.Customer.Mappers;
using MechanicShop.Domain.WorkOrders.Billing;
namespace MechanicShop.Application.Features.Billing.Mappers;

public static class InvoiceMapper
{
    public static InvoiceDto ToDto(this Invoice invoice)
        => new(
            InvoiceId: invoice.Id,
            WorkOrderId: invoice.WorkOrderId,
            IssuedAt: invoice.IssuedAt,
            Customer: invoice.WorkOrder.Vehicle.Customer.ToDto(),
            Vehicle: invoice.WorkOrder.Vehicle.ToDto(),
            DiscountAmount: invoice.DiscountAmount,
            SubtotalAmount: invoice.SubtotalAmount,
            TaxAmount: invoice.TaxAmount,
            TotalAmount: invoice.TotalAmount,
            PaymentStatus: invoice.PaymentStatus.ToString(),
            Items: [.. invoice.LineItems.Select(item => item.ToDto())]
        );

    public static List<InvoiceDto> ToDto(this IEnumerable<Invoice> invoices)
        => [.. invoices.Select(invoice => invoice.ToDto())];

    public static InvoiceLineItemDto ToDto(this InvoiceLineItem item)
        => new(
            InvoiceId: item.InvoiceId,
            LineNumber: item.LineNumber,
            Description: item.Description,
            UnitPrice: item.UnitPrice,
            Quantity: item.Quantity,
            LineTotal: item.LineTotal
        );

    public static List<InvoiceLineItemDto> ToDto(this IEnumerable<InvoiceLineItem> items)
    => [.. items.Select(item => item.ToDto())];
}