using MechanicShop.Domain.Common.Results;
namespace MechanicShop.Domain.RepairTasks.Parts;

public static class PartErrors
{
    public static Error NameRequired
        => Error.Validation("Part name is required.", "Part.Name.Required");

    public static Error CostInvalid
        => Error.Validation("Part cost must be greater than zero.", "Part.Cost.Invalid");

    public static Error QuantityInvalid
        => Error.Validation("Part quantity must be greater than zero.", "Part.Quantity.Invalid");
}