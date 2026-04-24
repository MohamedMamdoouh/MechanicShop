using System.Globalization;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Billing.Dtos;
using MechanicShop.Domain.WorkOrders.Billing;
using MechanicShop.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
namespace MechanicShop.Infrastructure.Services;

public sealed class InvoicePdfGenerator(IOptions<AppSettings> appSettings) : IInvoicePdfGenerator
{
    private const string PrimaryColor = "#1a1a2e";
    private const string SecondaryTextColor = "#555555";
    private static readonly CultureInfo UsCulture = CultureInfo.GetCultureInfo("en-US");

    public InvoicePdfDto Generate(Invoice invoice)
    {
        var shopName = appSettings.Value.ShopName;

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                page.Header().Element(c => ComposeHeader(c, invoice, shopName));
                page.Content().Element(c => ComposeContent(c, invoice));
                page.Footer().Element(c => ComposeFooter(c, shopName));
            });
        });

        return new InvoicePdfDto
        {
            PdfContent = pdf.GeneratePdf(),
            FileName = $"Invoice_{invoice.Id}.pdf"
        };
    }

    private static void ComposeHeader(IContainer container, Invoice invoice, string shopName)
    {
        container.Column(col =>
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("INVOICE").FontSize(28).Bold().FontColor(PrimaryColor);
                    c.Item().Text(shopName).FontSize(13).FontColor(SecondaryTextColor);
                });

                row.ConstantItem(160).AlignRight().Column(c =>
                {
                    c.Item().Text("Invoice #:").Bold();
                    c.Item().Text(invoice.Id.ToString()[..8]).FontSize(9).FontColor(SecondaryTextColor);
                    c.Item().PaddingTop(4)
                        .Text($"Issued: {invoice.IssuedAt:dd MMM yyyy HH:mm}")
                        .FontColor(SecondaryTextColor);
                });
            });

            col.Item().PaddingTop(8).LineHorizontal(1).LineColor(PrimaryColor);
        });
    }

    private static void ComposeContent(IContainer container, Invoice invoice)
    {
        container.PaddingVertical(20).Column(col =>
        {
            // Payment status
            col.Item().PaddingBottom(20).Row(row =>
            {
                row.AutoItem().Text("Payment Status: ").Bold();
                row.AutoItem().Text(invoice.PaymentStatus.ToString())
                    .Bold()
                    .FontColor(invoice.PaymentStatus == PaymentStatus.Paid ? "#2ecc71" : "#e74c3c");
            });

            // Line items table
            col.Item().Text("Line Items").FontSize(13).Bold().FontColor(PrimaryColor);
            col.Item().PaddingTop(8).Table(table =>
            {
                table.ColumnsDefinition(cols =>
                {
                    cols.ConstantColumn(36);   // #
                    cols.RelativeColumn(4);    // Description (wide for wrapping)
                    cols.ConstantColumn(40);   // Qty
                    cols.RelativeColumn(1.5f); // Unit Price
                    cols.RelativeColumn(1.5f); // Total
                });

                static IContainer HeaderCell(IContainer c) =>
                    c.Background(PrimaryColor).Padding(6);

                table.Header(header =>
                {
                    header.Cell().Element(HeaderCell).Text("#").FontColor(Colors.White).Bold();
                    header.Cell().Element(HeaderCell).Text("Description").FontColor(Colors.White).Bold();
                    header.Cell().Element(HeaderCell).Text("Qty").FontColor(Colors.White).Bold();
                    header.Cell().Element(HeaderCell).Text("Unit Price").FontColor(Colors.White).Bold();
                    header.Cell().Element(HeaderCell).Text("Total").FontColor(Colors.White).Bold();
                });

                var items = invoice.LineItems.OrderBy(li => li.LineNumber).ToList();
                for (var i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    var bg = i % 2 == 0 ? "#f9f9f9" : "#ffffff";

                    static IContainer DataCell(IContainer c, string bg) =>
                        c.Background(bg).Padding(6).AlignMiddle();

                    table.Cell().Element(c => DataCell(c, bg))
                        .Text(item.LineNumber.ToString());

                    // Text wrapping for long descriptions
                    table.Cell().Element(c => DataCell(c, bg))
                        .Text(item.Description);

                    table.Cell().Element(c => DataCell(c, bg))
                        .AlignCenter().Text(item.Quantity.ToString());

                    table.Cell().Element(c => DataCell(c, bg))
                        .AlignRight().Text(item.UnitPrice.ToString("C", UsCulture));

                    table.Cell().Element(c => DataCell(c, bg))
                        .AlignRight().Text(item.LineTotal.ToString("C", UsCulture));
                }
            });

            // Totals
            col.Item().PaddingTop(20).AlignRight().Width(220).Column(totals =>
            {
                totals.Item().Row(r =>
                {
                    r.RelativeItem().Text("Subtotal:").Bold();
                    r.ConstantItem(100).AlignRight()
                        .Text(invoice.SubtotalAmount.ToString("C", UsCulture));
                });
                totals.Item().Row(r =>
                {
                    r.RelativeItem().Text("Discount:").Bold();
                    r.ConstantItem(100).AlignRight()
                        .Text($"-{invoice.DiscountAmount.ToString("C", UsCulture)}").FontColor("#e74c3c");
                });
                totals.Item().Row(r =>
                {
                    r.RelativeItem().Text("Tax:").Bold();
                    r.ConstantItem(100).AlignRight()
                        .Text($"+{invoice.TaxAmount.ToString("C", UsCulture)}");
                });
                totals.Item().PaddingTop(4).LineHorizontal(1).LineColor(PrimaryColor);
                totals.Item().PaddingTop(4).Row(r =>
                {
                    r.RelativeItem().Text("Total:").FontSize(13).Bold();
                    r.ConstantItem(100).AlignRight()
                        .Text(invoice.TotalAmount.ToString("C", UsCulture))
                        .FontSize(13).Bold().FontColor(PrimaryColor);
                });

                if (invoice.PaidAt.HasValue)
                    totals.Item().PaddingTop(6)
                        .Text($"Paid on {invoice.PaidAt.Value:dd MMM yyyy}")
                        .FontSize(9).FontColor("#2ecc71");
            });
        });
    }

    private static void ComposeFooter(IContainer container, string shopName)
    {
        container.Column(col =>
        {
            col.Item().LineHorizontal(1).LineColor("#dddddd");
            col.Item().PaddingTop(8).AlignCenter()
                .Text($"Thank you for choosing {shopName}.")
                .FontSize(10).FontColor("#aaaaaa");
        });
    }
}