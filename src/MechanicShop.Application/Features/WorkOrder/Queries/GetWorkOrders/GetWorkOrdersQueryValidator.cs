using FluentValidation;
namespace MechanicShop.Application.Features.WorkOrder.Queries.GetWorkOrders;

public sealed class GetWorkOrdersQueryValidator : AbstractValidator<GetWorkOrdersQuery>
{
    public GetWorkOrdersQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
                .WithMessage("Page number must be at least 1.")
                .WithErrorCode("WorkOrder.GetList.PageNumber.Invalid");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
                .WithMessage("Page size must be between 1 and 100.")
                .WithErrorCode("WorkOrder.GetList.PageSize.Invalid");
    }
}
