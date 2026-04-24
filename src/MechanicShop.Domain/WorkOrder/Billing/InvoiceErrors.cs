using MechanicShop.Domain.Common.Results;
namespace MechanicShop.Domain.WorkOrders.Billing;

public static class InvoiceErrors
{
    public static Error InvoiceIdRequired
        => Error.Validation("Invoice id is invalid.", "Invoice.Id.Invalid");

    public static Error WorkOrderIdInvalid
        => Error.Validation("Work order id is invalid.", "Invoice.WorkOrderId.Invalid");

    public static Error LineItemsEmpty
        => Error.Validation("Invoice must have at least one line item.", "Invoice.LineItems.Empty");

    public static Error InvoiceAlreadyPaid
        => Error.Validation("Invoice is already paid.", "Invoice.AlreadyPaid");

    public static Error DiscountNegative
        => Error.Validation("Discount amount cannot be negative.", "Invoice.DiscountAmount.Invalid");

    public static Error DiscountExceedsSubTotal
        => Error.Validation("Discount amount cannot exceed subtotal amount.", "Invoice.DiscountAmount.Exceeds");

    public static Error IssuedAtInvalid
        => Error.Validation("Issued at date is invalid.", "Invoice.IssuedAt.Invalid");

    public static Error TaxAmountInvalid
        => Error.Validation("Tax amount is invalid.", "Invoice.TaxAmount.Invalid");

    public static Error StatusInvalid
        => Error.Validation("Invoice status is invalid.", "Invoice.Status.Invalid");

    public static Error CannotPayRefundedInvoice
        => Error.Validation("Cannot mark a refunded invoice as paid.", "Invoice.CannotPayRefunded");
}