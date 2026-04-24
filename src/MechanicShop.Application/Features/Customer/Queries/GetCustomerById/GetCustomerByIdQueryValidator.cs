using FluentValidation;
namespace MechanicShop.Application.Features.Customer.Queries.GetCustomerById;

public sealed class GetCustomerByIdQueryValidator : AbstractValidator<GetCustomerByIdQuery>
{
    public GetCustomerByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Customer ID is required.").WithErrorCode("Customer.Id.Required");
    }
}