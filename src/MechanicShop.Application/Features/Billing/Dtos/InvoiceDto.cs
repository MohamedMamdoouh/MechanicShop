using MechanicShop.Application.Features.Customer.Dtos;
namespace MechanicShop.Application.Features.Billing.Dtos;

public record InvoiceDto(
    Guid InvoiceId,
    Guid WorkOrderId,
    DateTimeOffset IssuedAt,
    CustomerDto Customer,
    VehicleDto Vehicle,
    decimal DiscountAmount,
    decimal SubtotalAmount,
    decimal TaxAmount,
    decimal TotalAmount,
    string PaymentStatus,
    List<InvoiceLineItemDto> Items);