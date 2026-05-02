using MechanicShop.Application.Common.Models;
using Xunit;
namespace MechanicShop.Application.UnitTests.Models;

public class PaginatedListTests
{
    // ── TotalPages ────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(0, 10, 0)] // no items
    [InlineData(10, 10, 1)] // exact fit
    [InlineData(11, 10, 2)] // one overflow item
    [InlineData(20, 10, 2)] // exact two pages
    [InlineData(1, 0, 0)] // zero page size guard
    public void TotalPages_ReturnsExpectedValue(int totalCount, int pageSize, int expectedTotalPages)
    {
        var list = new PaginatedList<int>
        {
            PageNumber = 1,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = [],
        };

        Assert.Equal(expectedTotalPages, list.TotalPages);
    }

    // ── HasPreviousPage ───────────────────────────────────────────────────────

    [Fact]
    public void HasPreviousPage_OnFirstPage_ReturnsFalse()
    {
        var list = new PaginatedList<int>
        {
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 30,
            Items = [],
        };

        Assert.False(list.HasPreviousPage);
    }

    [Fact]
    public void HasPreviousPage_OnSecondPage_ReturnsTrue()
    {
        var list = new PaginatedList<int>
        {
            PageNumber = 2,
            PageSize = 10,
            TotalCount = 30,
            Items = [],
        };

        Assert.True(list.HasPreviousPage);
    }

    [Fact]
    public void HasPreviousPage_OnLastPage_ReturnsTrue()
    {
        var list = new PaginatedList<int>
        {
            PageNumber = 3,
            PageSize = 10,
            TotalCount = 30,
            Items = [],
        };

        Assert.True(list.HasPreviousPage);
    }

    // ── HasNextPage ───────────────────────────────────────────────────────────

    [Fact]
    public void HasNextPage_OnFirstPageOfMultiple_ReturnsTrue()
    {
        var list = new PaginatedList<int>
        {
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 30,
            Items = [],
        };

        Assert.True(list.HasNextPage);
    }

    [Fact]
    public void HasNextPage_OnMiddlePage_ReturnsTrue()
    {
        var list = new PaginatedList<int>
        {
            PageNumber = 2,
            PageSize = 10,
            TotalCount = 30,
            Items = [],
        };

        Assert.True(list.HasNextPage);
    }

    [Fact]
    public void HasNextPage_OnLastPage_ReturnsFalse()
    {
        var list = new PaginatedList<int>
        {
            PageNumber = 3,
            PageSize = 10,
            TotalCount = 30,
            Items = [],
        };

        Assert.False(list.HasNextPage);
    }

    [Fact]
    public void HasNextPage_WhenSinglePage_ReturnsFalse()
    {
        var list = new PaginatedList<int>
        {
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 5,
            Items = [],
        };

        Assert.False(list.HasNextPage);
    }

    [Fact]
    public void HasNextPage_WhenNoItems_ReturnsFalse()
    {
        var list = new PaginatedList<int>
        {
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 0,
            Items = [],
        };

        Assert.False(list.HasNextPage);
    }

    // ── Both flags together (boundary checks) ────────────────────────────────

    [Theory]
    [InlineData(1, 10, 30, false, true)] // first of 3 pages
    [InlineData(2, 10, 30, true, true)] // middle of 3 pages
    [InlineData(3, 10, 30, true, false)] // last of 3 pages
    [InlineData(1, 10, 10, false, false)] // single page exact
    [InlineData(1, 10, 0, false, false)] // empty result set
    public void HasPreviousAndNextPage_ReturnCorrectCombination(
        int pageNumber,
        int pageSize,
        int totalCount,
        bool expectedHasPrevious,
        bool expectedHasNext)
    {
        var list = new PaginatedList<int>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = [],
        };

        Assert.Equal(expectedHasPrevious, list.HasPreviousPage);
        Assert.Equal(expectedHasNext, list.HasNextPage);
    }
}
