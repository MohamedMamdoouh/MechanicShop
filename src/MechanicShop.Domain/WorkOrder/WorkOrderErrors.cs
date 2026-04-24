using MechanicShop.Domain.Common.Results;
namespace MechanicShop.Domain.WorkOrders;

public static class WorkOrderErrors
{
   public static Error VehicleIdRequired
      => Error.Validation("Vehicle ID is required.", "WorkOrder.VehicleId.Required");

   public static Error RepairTasksRequired
      => Error.Validation("At least one repair task is required.", "WorkOrder.RepairTasks.Required");

   public static Error EmployeeIdRequired
      => Error.Validation("Employee ID is required.", "WorkOrder.EmployeeId.Required");

   public static Error InvalidStartEndTime
      => Error.Validation("Start time must be before end time.", "WorkOrder.Schedule.Invalid");

   public static Error SpotInvalid
      => Error.Validation("The specified spot is invalid.", "WorkOrder.Spot.Invalid");

   public static Error ReadOnly
      => Error.Validation("The work order is read-only and cannot be modified.", "WorkOrder.ReadOnly");

   public static Error RepairTaskRequired
      => Error.Validation("A repair task is required.", "WorkOrder.RepairTask.Required");

   public static Error RepairTaskAlreadyAdded
      => Error.Validation("The repair task has already been added.", "WorkOrder.RepairTask.AlreadyAdded");

   public static Error InvalidStatusTransition
      => Error.Validation("The status transition is invalid.", "WorkOrder.Status.InvalidTransition");

   public static Error DiscountInvalid
      => Error.Validation("Discount must be a non-negative value.", "WorkOrder.Discount.Invalid");

   public static Error TaxInvalid
      => Error.Validation("Tax must be a non-negative value.", "WorkOrder.Tax.Invalid");

   public static Error DiscountExceedsSubtotal
      => Error.Validation("Discount cannot exceed the subtotal of parts and labor.", "WorkOrder.Discount.ExceedsSubtotal");

   public static Error WorkOrderNotCompleted
      => Error.Validation("Work order must be completed before invoicing.", "WorkOrder.NotCompleted");

   public static Error InvoiceAlreadyExists
      => Error.Validation("An invoice already exists for this work order.", "WorkOrder.Invoice.AlreadyExists");

   public static Error CannotUpdateStatusBeforeStartTime
      => Error.Validation("Cannot update status before the start time.", "WorkOrder.Status.CannotUpdateBeforeStartTime");
}