using FluentValidation;
namespace MechanicShop.Application.Features.WorkOrder.Queries.GetWorkOrderById;

public sealed class GetWorkOrderByIdQueryValidator : AbstractValidator<GetWorkOrderByIdQuery>
{
    public GetWorkOrderByIdQueryValidator()
    {
        RuleFor(x => x.WorkOrderId)
            .NotEmpty().WithMessage("Id is required.").WithErrorCode("WorkOrder.Id.Required");
    }
}