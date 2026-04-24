using FluentValidation;
namespace MechanicShop.Application.Features.Identity.Queries;

public sealed class GetUserInfoQueryValidator : AbstractValidator<GetUserInfoQuery>
{
    public GetUserInfoQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
                .WithMessage("User ID is required.")
                .WithErrorCode("Authentication.UserId.Required")
            .Must(id => Guid.TryParse(id, out _))
                .WithMessage("User ID must be a valid GUID.")
                .WithErrorCode("Authentication.UserId.InvalidFormat");
    }
}