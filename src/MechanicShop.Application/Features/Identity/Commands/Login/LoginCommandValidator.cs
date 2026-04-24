using FluentValidation;
namespace MechanicShop.Application.Features.Identity.Commands.Login;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
                .WithMessage("Email is required.")
                .WithErrorCode("Authentication.Email.Required")
            .EmailAddress()
                .WithMessage("Invalid email format.")
                .WithErrorCode("Authentication.Email.InvalidFormat");

        RuleFor(x => x.Password)
            .NotEmpty()
                .WithMessage("Password is required.")
                .WithErrorCode("Authentication.Password.Required");

        RuleFor(x => x.DeviceIdentifier)
            .NotEmpty()
                .WithMessage("Device identifier is required.")
                .WithErrorCode("Authentication.DeviceIdentifier.Required")
            .MaximumLength(100)
                .WithMessage("Device identifier cannot exceed 100 characters.")
                .WithErrorCode("Authentication.DeviceIdentifier.TooLong");

        RuleFor(x => x.UserAgent)
            .MaximumLength(500)
                .WithMessage("User-Agent cannot exceed 500 characters.")
                .WithErrorCode("Authentication.UserAgent.TooLong")
            .When(x => !string.IsNullOrWhiteSpace(x.UserAgent));
    }
}