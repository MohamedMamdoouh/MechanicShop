using FluentValidation;
namespace MechanicShop.Application.Features.Customer.Commands.CreateCustomer;

public sealed class CreateVehicleCommandValidator : AbstractValidator<CreateVehicleCommand>
{
    public CreateVehicleCommandValidator()
    {
        RuleFor(x => x.Make)
            .NotEmpty()
                .WithMessage("Make is required.")
                .WithErrorCode("Vehicle.Make.Required")
            .MaximumLength(50)
                .WithMessage("Make cannot exceed 50 characters.")
                .WithErrorCode("Vehicle.Make.TooLong");

        RuleFor(x => x.Model)
            .NotEmpty()
                .WithMessage("Model is required.")
                .WithErrorCode("Vehicle.Model.Required")
            .MaximumLength(50)
                .WithMessage("Model cannot exceed 50 characters.")
                .WithErrorCode("Vehicle.Model.TooLong");

        RuleFor(x => x.Year)
            .NotEmpty()
                .WithMessage("Year is required.")
                .WithErrorCode("Vehicle.Year.Required")
            .InclusiveBetween(1886, DateTime.Now.Year + 1)
                .WithMessage($"Year must be between 1886 and {DateTime.Now.Year + 1}.")
                .WithErrorCode("Vehicle.Year.OutOfRange");

        RuleFor(x => x.LicensePlate)
            .NotEmpty()
                .WithMessage("License plate number is required.")
                .WithErrorCode("Vehicle.LicensePlate.Required")
            .MaximumLength(10)
                .WithMessage("License plate number cannot exceed 10 characters.")
                .WithErrorCode("Vehicle.LicensePlate.TooLong");
    }
}