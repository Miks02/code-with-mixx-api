namespace CodeWithMixx.API.Features.Analytics.GetAnalyticsForAdmin;

public record GetAnalyticsForAdminResponse
{
    public decimal TotalRevenue { get; init; } 
    public int TotalStudents { get; init; }
    public int CompletedClasses { get; init; }
    public int CompletedProjects { get; init; }
    
    public IReadOnlyList<int> AvailableYearsForSubjectsChart { get; init; } = [];
    public IReadOnlyList<int> AvailableYearsForFinanceChart { get; init; } = [];
    
    public IReadOnlyList<UpcomingClass> UpcomingClasses { get; init; } = [];
    public IReadOnlyList<UpcomingProject> UpcomingProjects { get; init; } = [];
    public IReadOnlyList<SubjectChartItem> SubjectChartData { get; init; } = [];
    public IReadOnlyList<FinanceChartItem> FinanceChartData { get; init; } = [];

    public record UpcomingClass
    {
        public int ReservationId { get; init; }
        public string SubjectName { get; init; } = null!;
        public string StudentName { get; init; } = null!;
        public DateTime StartsAt { get; init; }
        public DateTime EndsAt { get; init; }
    }
    
    public record UpcomingProject
    {
        public int ReservationId { get; init; }
        public string SubjectName { get; init; } = null!;
        public string StudentName { get; init; } = null!;
        public DateOnly StartDate { get; init; }
        public DateOnly EndDate { get; init; }
    }
    
    public record SubjectChartItem
    {
        public string SubjectName { get; init; } = null!;
        public int NumberOfClasses { get; init; }
    }

    public record FinanceChartItem
    {
        public int Month { get; init; }
        public decimal Revenue { get; init; }
        public int Students { get; init; }
    }
};