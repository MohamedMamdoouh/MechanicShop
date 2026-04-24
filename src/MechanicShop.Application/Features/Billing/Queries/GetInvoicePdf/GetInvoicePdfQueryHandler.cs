using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Billing.Dtos;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace MechanicShop.Application.Features.Billing.Queries.GetInvoicePdf;

public sealed class GetInvoicePdfQueryHandler(
    IAppDbContext context,
    ILogger<GetInvoicePdfQueryHandler> logger,
    IInvoicePdfGenerator pdfGenerator)
    : IRequestHandler<GetInvoicePdfQuery, Result<InvoicePdfDto>>
{
    public async Task<Result<InvoicePdfDto>> Handle(GetInvoicePdfQuery request, CancellationToken cancellationToken)
    {
        var invoice = await context.Invoices.AsNoTracking()
            .Include(x => x.LineItems)
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, cancellationToken);

        if (invoice is null)
        {
            logger.LogWarning("Invoice with ID {InvoiceId} not found.", request.InvoiceId);
            return ApplicationErrors.InvoiceNotFound;
        }

        try
        {
            return pdfGenerator.Generate(invoice);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to generate PDF for Invoice ID {InvoiceId}.", request.InvoiceId);
            return ApplicationErrors.InvoicePdfGenerationFailed;
        }
    }
}