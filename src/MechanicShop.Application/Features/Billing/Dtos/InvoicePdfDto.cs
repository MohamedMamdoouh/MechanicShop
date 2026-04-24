namespace MechanicShop.Application.Features.Billing.Dtos;

public sealed class InvoicePdfDto
{
    public string FileName { get; init; } = "invoice.pdf";
    public byte[] PdfContent { get; init; } = [];
    public string ContentType { get; } = "application/pdf";
}