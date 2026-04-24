using FluentValidation;
namespace MechanicShop.Application.Features.Identity.Commands.Logout;

public sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
                .WithMessage("User ID is required.")
                .WithErrorCode("Authentication.UserId.Required")
            .Must(id => Guid.TryParse(id, out _))
                .WithMessage("User ID must be a valid GUID.")
                .WithErrorCode("Authentication.UserId.Invalid");

        RuleFor(x => x.DeviceIdentifier)
            .NotEmpty()
                .WithMessage("Device identifier is required.")
                .WithErrorCode("Authentication.DeviceIdentifier.Required")
            .MaximumLength(100)
                .WithMessage("Device identifier cannot exceed 100 characters.")
                .WithErrorCode("Authentication.DeviceIdentifier.TooLong");
    }
}
