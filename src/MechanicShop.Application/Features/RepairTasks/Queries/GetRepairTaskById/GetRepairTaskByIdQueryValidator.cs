using FluentValidation;
namespace MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTaskById;

public sealed class GetRepairTaskByIdQueryValidator : AbstractValidator<GetRepairTaskByIdQuery>
{
    public GetRepairTaskByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.").WithErrorCode("RepairTask.Id.Required");
    }
}