using FluentValidation;
namespace MechanicShop.Application.Features.Billing.Queries.GetInvoiceById;

public sealed class GetInvoiceByIdQueryValidator : AbstractValidator<GetInvoiceByIdQuery>
{
    public GetInvoiceByIdQueryValidator()
    {
        RuleFor(q => q.InvoiceId)
            .NotEmpty().WithMessage("Invoice ID is required.").WithErrorCode("Invoice.Id.Required");
    }
}