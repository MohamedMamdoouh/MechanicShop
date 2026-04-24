using FluentValidation;
using MechanicShop.Application.Common.Models;
using Microsoft.Extensions.Options;
namespace MechanicShop.Application.Features.Dashboard.Queries;

public sealed class GetWorkOrderStatsQueryValidator : AbstractValidator<GetWorkOrderStatsQuery>
{
    public GetWorkOrderStatsQueryValidator(IOptions<DashboardSettings> settings, TimeProvider timeProvider)
    {
        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().DateTime);
        var oldestAllowedDate = today.AddMonths(-settings.Value.DashboardHistoryLimitInMonths);

        RuleFor(x => x.Date)
            .NotEmpty()
            .WithMessage("Date is required.")
            .WithErrorCode("Dashboard.Date.Required")
            .Must(date => date >= oldestAllowedDate)
            .WithMessage($"Date cannot be older than {settings.Value.DashboardHistoryLimitInMonths} months.")
            .WithErrorCode("Dashboard.Date.TooOld")
            .Must(date => date <= DateOnly.FromDateTime(timeProvider.GetUtcNow().DateTime))
            .WithMessage("Date cannot be in the future.")
            .WithErrorCode("Dashboard.Date.Invalid");
    }
}