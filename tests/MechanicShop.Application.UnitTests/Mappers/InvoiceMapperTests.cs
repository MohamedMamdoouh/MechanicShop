using System.Reflection;
using MechanicShop.Application.Features.Billing.Mappers;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Billing;
using MechanicShop.Domain.WorkOrders.Enum;
using MechanicShop.Tests.Common.Billing;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.Employees;
using MechanicShop.Tests.Common.RepairTasks;
using MechanicShop.Tests.Common.WorkOrders;
using Xunit;

namespace MechanicShop.Application.UnitTests.Mappers;

public class InvoiceMapperTests
{
    private static void SetField<T>(T entity, string fieldName, object value) where T : class
    {
        var type = entity.GetType();
        while (type is not null)
        {
            var field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field is not null)
            {
                field.SetValue(entity, value);
                return;
            }
            type = type.BaseType;
        }
        throw new InvalidOperationException($"Field '{fieldName}' not found on type '{entity.GetType().Name}'.");
    }

    private static (WorkOrder workOrder, Invoice invoice) CreateHydratedInvoice()
    {
        var repairTask = RepairTaskFactory.Create().Value;
        var customer = CustomerFactory.CreateCustomer(vehicles: []).Value;
        var vehicle = VehicleFactory.CreateVehicle().Value;
        var labor = EmployeeFactory.CreateEmployee().Value;

        SetField(vehicle, "<Customer>k__BackingField", customer);

        var workOrder = WorkOrderFactory.Create(
            startAtUtc: DateTimeOffset.UtcNow.AddHours(-2),
            endAtUtc: DateTimeOffset.UtcNow.AddHours(2),
            repairTasks: [repairTask]).Value;

        workOrder.UpdateStatus(WorkOrderState.InProgress, DateTimeOffset.UtcNow);
        workOrder.UpdateStatus(WorkOrderState.Completed, DateTimeOffset.UtcNow);

        SetField(workOrder, "<Vehicle>k__BackingField", vehicle);
        SetField(workOrder, "<Labor>k__BackingField", labor);

        var invoice = Invoice.CreateFromWorkOrder(workOrder).Value;
        SetField(invoice, "<WorkOrder>k__BackingField", workOrder);

        return (workOrder, invoice);
    }

    // --- InvoiceLineItem.ToDto ---

    [Fact]
    public void InvoiceLineItemToDto_ShouldMapAllFieldsCorrectly()
    {
        var invoiceId = Guid.NewGuid();
        var item = InvoiceLineItemFactory.Create(
            invoiceId: invoiceId,
            lineNumber: 3,
            description: "Brake Replacement",
            quantity: 2,
            unitPrice: 50m).Value;

        var dto = item.ToDto();

        Assert.Equal(invoiceId, dto.InvoiceId);
        Assert.Equal(3, dto.LineNumber);
        Assert.Equal("Brake Replacement", dto.Description);
        Assert.Equal(2, dto.Quantity);
        Assert.Equal(50m, dto.UnitPrice);
        Assert.Equal(item.LineTotal, dto.LineTotal);
    }

    [Fact]
    public void InvoiceLineItemListToDto_ShouldMapAllItemsCorrectly()
    {
        var id = Guid.NewGuid();
        var i1 = InvoiceLineItemFactory.Create(invoiceId: id, lineNumber: 1).Value;
        var i2 = InvoiceLineItemFactory.Create(invoiceId: id, lineNumber: 2).Value;

        var dtos = new List<InvoiceLineItem> { i1, i2 }.ToDto();

        Assert.Equal(2, dtos.Count);
        Assert.Equal(1, dtos[0].LineNumber);
        Assert.Equal(2, dtos[1].LineNumber);
    }

    // --- Invoice.ToDto ---

    [Fact]
    public void InvoiceToDto_ShouldMapAllFieldsCorrectly()
    {
        var (workOrder, invoice) = CreateHydratedInvoice();
        var vehicle = workOrder.Vehicle;
        var customer = vehicle.Customer;

        var dto = invoice.ToDto();

        Assert.Equal(invoice.Id, dto.InvoiceId);
        Assert.Equal(invoice.WorkOrderId, dto.WorkOrderId);
        Assert.Equal(invoice.IssuedAt, dto.IssuedAt);
        Assert.Equal(invoice.DiscountAmount, dto.DiscountAmount);
        Assert.Equal(invoice.SubtotalAmount, dto.SubtotalAmount);
        Assert.Equal(invoice.TaxAmount, dto.TaxAmount);
        Assert.Equal(invoice.TotalAmount, dto.TotalAmount);
        Assert.Equal(invoice.PaymentStatus.ToString(), dto.PaymentStatus);

        Assert.Equal(customer.Id, dto.Customer.Id);
        Assert.Equal(customer.FirstName, dto.Customer.FirstName);
        Assert.Equal(customer.LastName, dto.Customer.LastName);

        Assert.Equal(vehicle.Id, dto.Vehicle.Id);
        Assert.Equal(vehicle.Make, dto.Vehicle.Make);

        Assert.NotEmpty(dto.Items);
        Assert.Equal(invoice.LineItems.Count, dto.Items.Count);
    }

    [Fact]
    public void InvoiceListToDto_ShouldMapAllInvoicesCorrectly()
    {
        var (_, invoice1) = CreateHydratedInvoice();
        var (_, invoice2) = CreateHydratedInvoice();

        var dtos = new List<Invoice> { invoice1, invoice2 }.ToDto();

        Assert.Equal(2, dtos.Count);
        Assert.Equal(invoice1.Id, dtos[0].InvoiceId);
        Assert.Equal(invoice2.Id, dtos[1].InvoiceId);
    }
}