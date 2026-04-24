using FluentValidation;
using MechanicShop.Domain.Customers;
namespace MechanicShop.Application.Features.Customer.Commands.CreateCustomer;

public sealed class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator(IPhoneValidator phoneValidator)
    {
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
                .WithErrorCode("Customer.Email.InvalidFormat")
                .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
                .WithMessage("Phone number is required.")
                .WithErrorCode("Customer.PhoneNumber.Required")
            .Must(phoneValidator.IsValid)
                .WithMessage("Phone number is invalid.")
                .WithErrorCode("Customer.PhoneNumber.Invalid")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.Vehicles)
            .NotNull()
                .WithMessage("Vehicles information is required.")
                .WithErrorCode("Customer.Vehicles.Required")
            .Must(vehicles => vehicles.Count > 0)
                .WithMessage("At least one vehicle must be provided.")
                .WithErrorCode("Customer.Vehicle.AtLeastOneRequired");

        RuleForEach(x => x.Vehicles)
            .SetValidator(new CreateVehicleCommandValidator());
    }
}