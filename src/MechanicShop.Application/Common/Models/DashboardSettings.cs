using System.ComponentModel.DataAnnotations;
namespace MechanicShop.Application.Common.Models;

public sealed class DashboardSettings
{
    public const string SectionName = "DashboardSettings";

    [Range(1, 120, ErrorMessage =
    "DashboardSettings:DashboardHistoryLimitInMonths must be between 1 and 120.")]
    public int DashboardHistoryLimitInMonths { get; init; }
}
