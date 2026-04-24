using FluentValidation;
namespace MechanicShop.Application.Features.RepairTasks.Commands.UpdateRepairTask;

public sealed class UpdateRepairTaskCommandValidator : AbstractValidator<UpdateRepairTaskCommand>
{
    public UpdateRepairTaskCommandValidator()
    {
        RuleFor(x => x.RepairTaskId)
            .NotEmpty().WithMessage("Repair task ID is required.").WithErrorCode("RepairTask.Id.Required");
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Repair task name is required.").WithErrorCode("RepairTask.Name.Required")
            .MaximumLength(100).WithMessage("Repair task name must not exceed 100 characters.")
            .WithErrorCode("RepairTask.Name.TooLong");

        RuleFor(x => x.LaborCost)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Labor cost must be greater than or equal to 0.").WithErrorCode("RepairTask.LaborCost.Invalid");

        RuleFor(x => x.RepairDurationMinutes)
               .IsInEnum()
               .WithMessage("Repair duration is required.").WithErrorCode("RepairTask.Duration.Required");

        RuleFor(x => x.Parts)
            .Must(parts => parts.All(p => p != null))
            .WithMessage("Parts list must not contain null elements.")
            .WithErrorCode("RepairTask.Parts.ContainsNull");

        RuleFor(x => x.Parts)
           .Must(parts =>
        {
            var normalized = parts.Select(p => p.Name.Trim().Replace(" ", "").ToLowerInvariant());
            return normalized.Distinct().Count() == normalized.Count();
        })
        .WithMessage("Part names must be unique within a repair task.")
        .WithErrorCode("RepairTask.Part.Name.Duplicate");

        RuleForEach(x => x.Parts)
            .SetValidator(new UpdateRepairTaskPartCommandValidator());
    }
}