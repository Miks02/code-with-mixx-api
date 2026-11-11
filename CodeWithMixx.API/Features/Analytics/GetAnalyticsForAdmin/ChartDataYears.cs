namespace CodeWithMixx.API.Features.Analytics.GetAnalyticsForAdmin;

public record ChartDataYears
{
    public int SelectedYear { get; init; }
    public IReadOnlyList<int> AvailableYears { get; init; } = [];
};