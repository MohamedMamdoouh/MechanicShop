namespace MechanicShop.Application.Common.Constants;

public static class CacheDurations
{
    public static readonly TimeSpan LaborList = TimeSpan.FromMinutes(60);

    public static readonly TimeSpan UserById = TimeSpan.FromMinutes(15);

    public static readonly TimeSpan CustomerById = TimeSpan.FromMinutes(15);

    public static readonly TimeSpan RepairTaskList = TimeSpan.FromMinutes(30);

    public static readonly TimeSpan RepairTaskById = TimeSpan.FromMinutes(30);

    public static readonly TimeSpan InvoiceById = TimeSpan.FromMinutes(60);

    public static readonly TimeSpan CustomerList = TimeSpan.FromMinutes(30);

    public static readonly TimeSpan WorkOrderById = TimeSpan.FromMinutes(5);

    public static readonly TimeSpan WorkOrderPaginatedList = TimeSpan.FromMinutes(2);

    public static readonly TimeSpan DailySchedule = TimeSpan.FromMinutes(10);

    public static readonly TimeSpan DashboardStats = TimeSpan.FromMinutes(15);
}
