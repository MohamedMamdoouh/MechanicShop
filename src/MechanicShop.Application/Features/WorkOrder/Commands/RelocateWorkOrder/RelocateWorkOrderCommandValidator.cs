using FluentValidation;
namespace MechanicShop.Application.Features.WorkOrder.Commands.RelocateWorkOrder;

public sealed class RelocateWorkOrderCommandValidator : AbstractValidator<RelocateWorkOrderCommand>
{
    public RelocateWorkOrderCommandValidator()
    {
        RuleFor(x => x.WorkOrderId)
            .NotEmpty().WithMessage("Work order ID is required.")
            .WithErrorCode("WorkOrder.Id.Required");

        RuleFor(x => x.NewStartAt)
            .GreaterThan(DateTimeOffset.UtcNow).WithMessage("New start time must be in the future.")
            .WithErrorCode("WorkOrder.NewStartAt.Invalid");

        RuleFor(x => x.Spot)
            .IsInEnum().WithMessage("Invalid spot value.")
            .WithErrorCode("WorkOrder.Spot.Invalid");
    }
}