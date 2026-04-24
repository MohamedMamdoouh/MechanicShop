using FluentValidation;
namespace MechanicShop.Application.Features.WorkOrder.Commands.AssignLabor;

public sealed class AssignLaborCommandValidator : AbstractValidator<AssignLaborCommand>
{
    public AssignLaborCommandValidator()
    {
        RuleFor(x => x.WorkOrderId)
            .NotEmpty().WithMessage("Work order ID is required.").WithErrorCode("WorkOrder.Id.Required");

        RuleFor(x => x.LaborId)
            .NotEmpty().WithMessage("Labor ID is required.").WithErrorCode("Labor.Id.Required");
    }
}