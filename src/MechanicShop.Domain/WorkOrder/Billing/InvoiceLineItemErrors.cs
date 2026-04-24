using MechanicShop.Domain.Common.Results;
namespace MechanicShop.Domain.WorkOrders.Billing;

public static class InvoiceLineItemErrors
{
    public static Error InvoiceIdRequired
        => Error.Validation("Invoice id is required.", "InvoiceLineItem.InvoiceId.Required");

    public static Error LineNumberInvalid
        => Error.Validation("Line number is invalid.", "InvoiceLineItem.LineNumber.Invalid");

    public static Error DescriptionRequired
        => Error.Validation("Description is required.", "InvoiceLineItem.Description.Required");

    public static Error QuantityInvalid
        => Error.Validation("Quantity must be greater than zero.", "InvoiceLineItem.Quantity.Invalid");

    public static Error UnitPriceInvalid
        => Error.Validation("Unit price must be greater than zero.", "InvoiceLineItem.UnitPrice.Invalid");
}