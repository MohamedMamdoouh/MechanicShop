namespace MechanicShop.Application.Features.Billing.Dtos;

public record InvoiceLineItemDto(
    Guid InvoiceId,
    int LineNumber,
    string Description,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal);