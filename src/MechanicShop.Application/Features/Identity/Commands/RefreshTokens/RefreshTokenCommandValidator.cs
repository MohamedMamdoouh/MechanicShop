using FluentValidation;
namespace MechanicShop.Application.Features.Identity.Commands.RefreshTokens;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
                .WithMessage("Refresh token is required.")
                .WithErrorCode("Authentication.RefreshToken.Required");

        RuleFor(x => x.ExpiredAccessToken)
            .NotEmpty()
                .WithMessage("Expired access token is required.")
                .WithErrorCode("Authentication.AccessToken.Required");

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
