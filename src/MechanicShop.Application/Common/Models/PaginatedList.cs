namespace MechanicShop.Application.Common.Models;

public class PaginatedList<T>
{
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;

    public IReadOnlyCollection<T>? Items { get; init; }
}