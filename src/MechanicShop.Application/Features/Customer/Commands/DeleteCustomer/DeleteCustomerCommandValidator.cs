using FluentValidation;
namespace MechanicShop.Application.Features.Customer.Commands.DeleteCustomer;

public sealed class DeleteCustomerCommandValidator : AbstractValidator<DeleteCustomerCommand>
{
    public DeleteCustomerCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
                .WithMessage("Customer ID is required.")
                .WithErrorCode("Customer.Id.Required")
            .Must(id => id != Guid.Empty)
                .WithMessage("Customer ID must be a valid GUID.")
                .WithErrorCode("Customer.Id.Invalid");
    }
}
