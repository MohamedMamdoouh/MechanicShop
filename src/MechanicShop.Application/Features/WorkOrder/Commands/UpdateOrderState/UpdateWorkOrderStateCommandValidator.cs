using FluentValidation;
namespace MechanicShop.Application.Features.WorkOrder.Commands.UpdateOrderState;

public sealed class UpdateWorkOrderStateCommandValidator : AbstractValidator<UpdateWorkOrderStateCommand>
{
    public UpdateWorkOrderStateCommandValidator()
    {
        RuleFor(x => x.WorkOrderId)
            .NotEmpty().WithMessage("Work order ID is required.")
            .WithErrorCode("WorkOrder.Id.Required");

        RuleFor(x => x.NewState)
            .IsInEnum().WithMessage("Invalid work order state.")
            .WithErrorCode("WorkOrder.State.Invalid");
    }
}