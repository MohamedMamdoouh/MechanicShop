using FluentValidation;
namespace MechanicShop.Application.Features.WorkOrder.Commands.UpdateWorkOrderRepairTasks;

public sealed class UpdateWorkOrderRepairTasksCommandValidator : AbstractValidator<UpdateWorkOrderRepairTasksCommand>
{
    public UpdateWorkOrderRepairTasksCommandValidator()
    {
        RuleFor(x => x.WorkOrderId)
            .NotEmpty().WithMessage("Work order ID is required.")
            .WithErrorCode("WorkOrder.Id.Required");

        RuleFor(x => x.RepairTaskIds)
            .NotEmpty().WithMessage("Repair task IDs are required.")
            .WithErrorCode("WorkOrder.RepairTaskIds.Required")
            .Must(ids => ids.All(id => id != Guid.Empty))
            .WithMessage("All repair task IDs must be valid GUIDs.")
            .WithErrorCode("WorkOrder.RepairTaskIds.Invalid");
    }
}