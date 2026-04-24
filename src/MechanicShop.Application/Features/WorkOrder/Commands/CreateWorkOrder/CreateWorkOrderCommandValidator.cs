using FluentValidation;
namespace MechanicShop.Application.Features.WorkOrder.Commands.CreateWorkOrder;

public sealed class CreateWorkOrderCommandValidator : AbstractValidator<CreateWorkOrderCommand>
{
    public CreateWorkOrderCommandValidator()
    {
        RuleFor(x => x.Spot)
            .IsInEnum()
            .WithMessage("Invalid spot value.")
            .WithErrorCode("WorkOrder.Spot.Invalid");

        RuleFor(x => x.VehicleId)
            .NotEmpty()
            .WithMessage("VehicleId is required.")
            .WithErrorCode("WorkOrder.VehicleId.Required");

        RuleFor(x => x.StartAt)
            .GreaterThan(DateTimeOffset.UtcNow)
            .WithMessage("StartAt must be in the future.")
            .WithErrorCode("WorkOrder.StartAt.Invalid");

        RuleFor(x => x.RepairTaskIds)
            .NotEmpty()
            .WithMessage("At least one RepairTaskId is required.")
            .WithErrorCode("WorkOrder.RepairTasksIds.Required");

        RuleForEach(x => x.RepairTaskIds)
            .NotEmpty()
            .WithMessage("RepairTaskId cannot be empty.")
            .WithErrorCode("WorkOrder.RepairTaskId.Invalid");

        RuleFor(x => x.LaborId)
            .NotEmpty()
            .WithMessage("LaborId is required.")
            .WithErrorCode("WorkOrder.LaborId.Required");
    }
}