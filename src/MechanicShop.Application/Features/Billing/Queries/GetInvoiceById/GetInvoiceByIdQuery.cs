using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Billing.Dtos;
namespace MechanicShop.Application.Features.Billing.Queries.GetInvoiceById;

public sealed record GetInvoiceByIdQuery(Guid InvoiceId)
    : ICachedQuery<InvoiceDto>
{
    public string CacheKey => CacheKeys.InvoiceById(InvoiceId);

    public string[] CacheTag => [CacheTags.Invoices];

    public TimeSpan CacheDuration => CacheDurations.InvoiceById;
}