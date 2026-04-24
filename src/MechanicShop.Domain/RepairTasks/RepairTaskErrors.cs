using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.RepairTasks;

public static class RepairTaskErrors
{
    public static Error NameRequired
        => Error.Validation("Repair Task name is required.", "RepairTask.Name.Required");

    public static Error LaborCostInvalid
        => Error.Validation("Labor cost must be a non-negative value.", "RepairTask.LaborCost.Invalid");

    public static Error DurationInvalid
        => Error.Validation("Duration must be a non-negative value.", "RepairTask.Duration.Invalid");

    public static Error PartsRequired
        => Error.Validation("At least one part is required.", "RepairTask.Parts.Required");

    public static Error AtLeastOneRepairTaskIsRequired
        => Error.Validation("At least one repair task is required.", "RepairTask.AtLeastOneRequired");

    public static Error InUse
        => Error.Conflict("Repair task is currently in use and cannot be modified or deleted.", "RepairTask.InUse");

    public static Error DuplicateName
        => Error.Conflict("Repair task with the same name already exists.", "RepairTask.Name.Duplicate");

    public static Error DuplicatePartName
        => Error.Conflict("A part with the same name already exists in this repair task.", "RepairTask.Part.Name.Duplicate");
}