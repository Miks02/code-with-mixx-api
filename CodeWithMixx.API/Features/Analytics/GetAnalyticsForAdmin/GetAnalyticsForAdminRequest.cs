using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Analytics.GetAnalyticsForAdmin;

public record GetAnalyticsForAdminRequest
{
    [FromQuery]
    public int? SelectedYearForFinanceChart { get; init; }
    [FromQuery]
    public int? SelectedYearForSubjectsChart { get; init; }
};