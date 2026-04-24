using MechanicShop.Domain.WorkOrders.Enum;
namespace MechanicShop.Application.Common.Constants;

public static class CacheKeys
{
    public static string UserById(Guid id) => $"users:{id}";

    public static string CustomerById(Guid id) => $"customers:{id}";

    public static string CustomerList() => $"customers:list";

    public static string CustomerListPaginated(int pageNumber, int pageSize)
        => $"customers:list:pageNumber={pageNumber}:pageSize={pageSize}";

    public static string LaborList() => $"labors:list";

    public static string InvoiceById(Guid id) => $"invoices:{id}";

    public static string InvoiceList() => $"invoices:list";

    public static string DashboardStatsByDate(DateOnly date) => $"dashboard:workOrderStats:{date:yyyy-MM-dd}";

    public static string RepairTaskById(Guid id) => $"repairTasks:{id}";

    public static string RepairTaskList() => $"repairTasks:list";

    public static string Schedule(DateOnly date, Guid? laborId) =>
        $"schedules:{date:yyyy-MM-dd}:labor={laborId?.ToString() ?? "all"}";

    public static string WorkOrderById(Guid id) => $"workOrders:{id}";

    public static string WorkOrderList() => $"workOrders:list";

    public static string WorkOrderListItemPaginated(WorkOrderListFilter filter) =>
        $"workOrders:paginated:pageNumber={filter.PageNumber}:pageSize={filter.PageSize}" +
        $":searchTerm={filter.SearchTerm ?? "null"}:sortBy={filter.SortBy}:sortDescending={filter.SortDescending}" +
        $":vehicleId={filter.VehicleId}:laborId={filter.LaborId}" +
        $":startDateFrom={filter.StartDateFrom}:startDateTo={filter.StartDateTo}" +
        $":endDateFrom={filter.EndDateFrom}:endDateTo={filter.EndDateTo}" +
        $":spot={filter.Spot}:status={filter.Status}";
}

public sealed record WorkOrderListFilter(
    int PageNumber,
    int PageSize,
    string? SearchTerm = null,
    string SortBy = "createdAt",
    bool SortDescending = false,
    Guid? VehicleId = null,
    Guid? LaborId = null,
    DateTime? StartDateFrom = null,
    DateTime? StartDateTo = null,
    DateTime? EndDateFrom = null,
    DateTime? EndDateTo = null,
    Spot? Spot = null,
    WorkOrderState? Status = null);
