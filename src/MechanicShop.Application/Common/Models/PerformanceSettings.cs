using System.ComponentModel.DataAnnotations;
namespace MechanicShop.Application.Common.Models;

public sealed class PerformanceSettings
{
    public const string SectionName = "PerformanceSettings";

    [Range(1, int.MaxValue, ErrorMessage =
    "PerformanceSettings:LongRunningRequestThresholdInMs must be greater than 0.")]
    public int LongRunningRequestThresholdInMs { get; init; }
}