using MechanicShop.Domain.Common.BaseEntities;
using MechanicShop.Domain.Common.Results;
namespace MechanicShop.Domain.WorkOrders.Billing;

public class Invoice : AuditableEntity
{
    public Guid WorkOrderId { get; }
    public DateTimeOffset IssuedAt { get; }
    public decimal DiscountAmount { get; private set; }
    public decimal TaxAmount { get; }
    public decimal SubtotalAmount => _lineItems.Sum(li => li.LineTotal);
    public decimal TotalAmount => SubtotalAmount - DiscountAmount + TaxAmount;
    public PaymentStatus PaymentStatus { get; private set; }
    public DateTimeOffset? PaidAt { get; private set; }

    public WorkOrder WorkOrder { get; } = null!;

    private readonly List<InvoiceLineItem> _lineItems = [];
    public IReadOnlyCollection<InvoiceLineItem> LineItems => _lineItems.AsReadOnly();

    // Parameterless constructor for EF Core and other ORMs.
    private Invoice() { }

    private Invoice(
        Guid id,
        Guid workOrderId,
        DateTimeOffset issuedAt,
        decimal discountAmount,
        decimal taxAmount,
        PaymentStatus status,
        DateTimeOffset? paidAt,
        List<InvoiceLineItem> lineItems)
    {
        Id = id;
        WorkOrderId = workOrderId;
        IssuedAt = issuedAt;
        DiscountAmount = discountAmount;
        TaxAmount = taxAmount;
        PaymentStatus = status;
        PaidAt = paidAt;
        _lineItems = lineItems ?? [];
    }

    private static Result<Invoice> Create(
        Guid invoiceId,
        Guid workOrderId,
        DateTimeOffset issuedAt,
        decimal discountAmount,
        decimal taxAmount,
        PaymentStatus status,
        List<InvoiceLineItem> lineItems)
    {
        var errors = new List<Error>();

        if (invoiceId == Guid.Empty)
        {
            errors.Add(InvoiceErrors.InvoiceIdRequired);
        }

        if (lineItems is null || lineItems.Count == 0)
        {
            errors.Add(InvoiceErrors.LineItemsEmpty);
        }

        var subtotal = lineItems?.Sum(li => li.LineTotal) ?? 0;

        if (workOrderId == Guid.Empty)
        {
            errors.Add(InvoiceErrors.WorkOrderIdInvalid);
        }

        if (issuedAt == default)
        {
            errors.Add(InvoiceErrors.IssuedAtInvalid);
        }

        if (discountAmount < 0)
        {
            errors.Add(InvoiceErrors.DiscountNegative);
        }

        if (lineItems is not null && lineItems.Count > 0 && discountAmount > 0 && discountAmount > subtotal)
        {
            errors.Add(InvoiceErrors.DiscountExceedsSubTotal);
        }

        if (taxAmount < 0)
        {
            errors.Add(InvoiceErrors.TaxAmountInvalid);
        }

        if (!System.Enum.IsDefined(status))
        {
            errors.Add(InvoiceErrors.StatusInvalid);
        }

        if (errors.Count > 0)
        {
            return errors;
        }

        return new Invoice(
            invoiceId,
            workOrderId,
            issuedAt,
            discountAmount,
            taxAmount,
            status,
            null,
            lineItems!);
    }

    public Result<Updated> MarkAsPaid()
    {
        var errors = new List<Error>();

        if (PaymentStatus == PaymentStatus.Paid)
        {
            errors.Add(InvoiceErrors.InvoiceAlreadyPaid);
        }

        if (PaymentStatus == PaymentStatus.Refunded)
        {
            errors.Add(InvoiceErrors.CannotPayRefundedInvoice);
        }

        if (errors.Count > 0)
        {
            return errors;
        }

        PaymentStatus = PaymentStatus.Paid;
        PaidAt = DateTimeOffset.UtcNow;

        return Result.Updated;
    }

    public Result<Updated> ApplyDiscount(decimal discountAmount)
    {
        var errors = new List<Error>();

        if (PaymentStatus == PaymentStatus.Paid)
        {
            errors.Add(InvoiceErrors.InvoiceAlreadyPaid);
        }

        if (discountAmount < 0)
        {
            errors.Add(InvoiceErrors.DiscountNegative);
        }

        if (discountAmount > SubtotalAmount)
        {
            errors.Add(InvoiceErrors.DiscountExceedsSubTotal);
        }

        if (errors.Count > 0)
        {
            return errors;
        }

        DiscountAmount = discountAmount;

        return Result.Updated;
    }

    public static Result<Invoice> CreateFromWorkOrder(WorkOrder workOrder)
    {
        var canInvoice = workOrder.EnsureCanBeInvoiced();
        if (!canInvoice.IsSuccess)
        {
            return canInvoice.Errors.ToList();
        }

        var invoiceId = Guid.NewGuid();
        var lineItems = new List<InvoiceLineItem>();
        var lineNumber = 1;

        foreach (var task in workOrder.RepairTasks)
        {
            var lineItemResult = InvoiceLineItem.Create(
                invoiceId: invoiceId,
                lineNumber: lineNumber++,
                description: task.Name,
                quantity: 1,
                unitPrice: task.TotalCost);

            if (!lineItemResult.IsSuccess)
            {
                return lineItemResult.Errors.ToList();
            }

            lineItems.Add(lineItemResult.Value);
        }

        var discountAmount = workOrder.Discount;
        var taxAmount = workOrder.Tax;

        return Create(
            invoiceId: invoiceId,
            workOrderId: workOrder.Id,
            issuedAt: DateTimeOffset.UtcNow,
            discountAmount: discountAmount,
            taxAmount: taxAmount,
            status: PaymentStatus.Unpaid,
            lineItems: lineItems);
    }
}