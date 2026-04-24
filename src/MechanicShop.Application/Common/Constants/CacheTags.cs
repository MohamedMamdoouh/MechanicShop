namespace MechanicShop.Application.Common.Constants;

public static class CacheTags
{
    public const string Customer = "customer";

    public const string Labors = "labors";

    public const string Invoices = "invoices";

    public const string Dashboard = "dashboard";

    public const string RepairTasks = "repairTasks";

    public const string Schedules = "schedules";

    public const string WorkOrders = "workOrders";

    public static string WorkOrderById(Guid id) => $"workOrders:item:{id}";
}
