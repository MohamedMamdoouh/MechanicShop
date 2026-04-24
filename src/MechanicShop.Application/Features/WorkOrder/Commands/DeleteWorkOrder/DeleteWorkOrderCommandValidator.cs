using FluentValidation;
namespace MechanicShop.Application.Features.WorkOrder.Commands.DeleteWorkOrder;

public sealed class DeleteWorkOrderCommandValidator : AbstractValidator<DeleteWorkOrderCommand>
{
    public DeleteWorkOrderCommandValidator()
    {
        RuleFor(x => x.WorkOrderId)
            .NotEmpty().WithMessage("Work order ID is required.")
            .WithErrorCode("WorkOrder.Id.Required");
    }
}