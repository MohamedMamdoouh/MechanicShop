using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Billing.Dtos;
using MechanicShop.Application.Features.Billing.Mappers;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Billing;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
namespace MechanicShop.Application.Features.Billing.Commands.IssueInvoice;

public sealed class IssueInvoiceCommandHandler(
    IAppDbContext context,
    ILogger<IssueInvoiceCommandHandler> logger,
    HybridCache cache)
    : IRequestHandler<IssueInvoiceCommand, Result<InvoiceDto>>
{
    public async Task<Result<InvoiceDto>> Handle(IssueInvoiceCommand request, CancellationToken cancellationToken)
    {
        var workOrder = await context.WorkOrders
        .Include(v => v.Vehicle).ThenInclude(c => c.Customer)
        .Include(i => i.Invoice)
        .Include(rt => rt.RepairTasks).ThenInclude(p => p.Parts)
        .FirstOrDefaultAsync(w => w.Id == request.WorkOrderId, cancellationToken);

        if (workOrder is null)
        {
            logger.LogWarning("Work order with ID {WorkOrderId} not found.", request.WorkOrderId);
            return ApplicationErrors.WorkOrderNotFound;
        }

        var invoiceResult = Invoice.CreateFromWorkOrder(workOrder);

        if (!invoiceResult.IsSuccess)
        {
            logger.LogWarning(
                "Failed to create invoice for work order ID {WorkOrderId}. Errors: {Errors}",
                request.WorkOrderId,
                invoiceResult.Errors);

            return invoiceResult.Errors.ToList();
        }

        var invoice = invoiceResult.Value;
        await context.Invoices.AddAsync(invoice, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        await cache.RemoveByTagAsync(CacheTags.Invoices, cancellationToken);

        logger.LogInformation(
            "Invoice with ID {InvoiceId} created for work order ID {WorkOrderId}.",
            invoice.Id,
            request.WorkOrderId);

        return invoice.ToDto();
    }
}