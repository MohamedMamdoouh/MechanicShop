using FluentValidation;
namespace MechanicShop.Application.Features.Billing.Commands.SettleInvoice;

public sealed class SettleInvoiceCommandValidator : AbstractValidator<SettleInvoiceCommand>
{
    public SettleInvoiceCommandValidator()
    {
        RuleFor(c => c.InvoiceId)
            .NotEmpty().WithMessage("Invoice ID is required.").WithErrorCode("Invoice.Id.Required");
    }
}