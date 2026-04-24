using FluentValidation;
using MechanicShop.Domain.Customers;
namespace MechanicShop.Application.Features.Customer.Commands.UpdateCustomer;

public sealed class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator(IPhoneValidator phoneValidator)
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
                .WithMessage("Customer ID is required.")
                .WithErrorCode("Customer.Id.Required")
            .Must(id => id != Guid.Empty)
                .WithMessage("Customer ID must be a valid GUID.")
                .WithErrorCode("Customer.Id.Invalid");

        RuleFor(x => x.FirstName)
            .NotEmpty()
                .WithMessage("First name is required.")
                .WithErrorCode("Customer.FirstName.Required")
            .MaximumLength(50)
                .WithMessage("First name cannot exceed 50 characters.")
                .WithErrorCode("Customer.FirstName.TooLong");

        RuleFor(x => x.LastName)
            .NotEmpty()
                .WithMessage("Last name is required.")
                .WithErrorCode("Customer.LastName.Required")
            .MaximumLength(50)
                .WithMessage("Last name cannot exceed 50 characters.")
                .WithErrorCode("Customer.LastName.TooLong");

        RuleFor(x => x.Email)
            .NotEmpty()
                .WithMessage("Email is required.")
                .WithErrorCode("Customer.Email.Required")
            .EmailAddress()
                .WithMessage("Invalid email format.")
                .WithErrorCode("Customer.Email.InvalidFormat");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
                .WithMessage("Phone number is required.")
                .WithErrorCode("Customer.PhoneNumber.Required")
            .Must(phoneValidator.IsValid)
                .WithMessage("Phone number is invalid.")
                .WithErrorCode("Customer.PhoneNumber.Invalid")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }
}
