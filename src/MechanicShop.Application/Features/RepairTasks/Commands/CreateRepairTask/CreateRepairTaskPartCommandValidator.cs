using FluentValidation;
namespace MechanicShop.Application.Features.RepairTasks.Commands.CreateRepairTask;

public sealed class CreateRepairTaskPartCommandValidator : AbstractValidator<CreateRepairTaskPartCommand>
{
    public CreateRepairTaskPartCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Part name is required.").WithErrorCode("RepairTask.Part.Name.Required")
            .MaximumLength(100)
            .WithMessage("Part name must not exceed 100 characters.").WithErrorCode("RepairTask.Part.Name.TooLong");

        RuleFor(x => x.Cost)
            .GreaterThan(0)
            .WithMessage("Part cost must be greater than 0.").WithErrorCode("RepairTask.Part.Cost.Invalid");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Part quantity must be greater than 0.").WithErrorCode("RepairTask.Part.Quantity.Invalid");
    }
}