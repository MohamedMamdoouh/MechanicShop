using FluentValidation;
namespace MechanicShop.Application.Features.Billing.Queries.GetInvoicePdf;

public sealed class GetInvoicePdfQueryValidator : AbstractValidator<GetInvoicePdfQuery>
{
    public GetInvoicePdfQueryValidator()
    {
        RuleFor(q => q.InvoiceId)
             .NotEmpty().WithMessage("Invoice ID is required.").WithErrorCode("Invoice.Id.Required");
    }
}