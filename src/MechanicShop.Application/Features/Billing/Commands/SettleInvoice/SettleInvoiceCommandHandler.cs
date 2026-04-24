using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
namespace MechanicShop.Application.Features.Billing.Commands.SettleInvoice;

public sealed class SettleInvoiceCommandHandler(
    IAppDbContext context,
    ILogger<SettleInvoiceCommandHandler> logger,
    HybridCache cache)
    : IRequestHandler<SettleInvoiceCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(SettleInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await context.Invoices.FirstOrDefaultAsync(
            i => i.Id == request.InvoiceId, cancellationToken);

        if (invoice is null)
        {
            logger.LogWarning("Invoice with ID {InvoiceId} not found.", request.InvoiceId);
            return ApplicationErrors.InvoiceNotFound;
        }

        var payInvoiceResult = invoice.MarkAsPaid();

        if (!payInvoiceResult.IsSuccess)
        {
            logger.LogWarning(
                "Failed to settle invoice with ID {InvoiceId}. Errors: {Errors}",
                request.InvoiceId,
                payInvoiceResult.Errors);

            return payInvoiceResult.Errors.ToList();
        }

        context.Invoices.Update(invoice);
        await context.SaveChangesAsync(cancellationToken);

        await cache.RemoveByTagAsync(CacheTags.Invoices, cancellationToken);
        logger.LogInformation("Invoice with ID {InvoiceId} has been settled.", request.InvoiceId);

        return Result.Success;
    }
}