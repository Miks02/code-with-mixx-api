using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Analytics.GetAnalyticsForAdmin;

public class GetAnalyticsForAdminHandler(AppDbContext context, IUserClock userClock)
    : IHandler<GetAnalyticsForAdminRequest, GetAnalyticsForAdminResponse>
{
    private const int UpcomingItemsLimit = 12;

    public async Task<GetAnalyticsForAdminResponse> HandleAsync(GetAnalyticsForAdminRequest request, CancellationToken ct = default)
    {
        var subjectsChartYears = await GetSubjectsChartYears(request.SelectedYearForSubjectsChart, ct);
        var financeChartYears = await GetFinanceChartYears(request.SelectedYearForFinanceChart, ct);

        return new GetAnalyticsForAdminResponse
        {
            TotalRevenue = await context.Reservations.SumAsync(r => r.PaidAmount, ct),
            TotalStudents = await context.Students.CountAsync(ct),
            CompletedClasses = await GetCompletedClassesCount(ct),
            CompletedProjects = await context.Projects.CountAsync(p => p.Progress == 100, ct),
            UpcomingClasses = await GetUpcomingClasses(ct),
            UpcomingProjects = await GetUpcomingProjects(ct),
            AvailableYearsForSubjectsChart = subjectsChartYears.AvailableYears,
            SubjectChartData = await GetSubjectChart(subjectsChartYears, ct),
            AvailableYearsForFinanceChart = financeChartYears.AvailableYears,
            FinanceChartData = await GetFinanceChart(financeChartYears, ct)
        };
    }

    private async Task<int> GetCompletedClassesCount(CancellationToken ct = default)
    {
        return await context.Reservations
            .Where(r => r.ReservationStatus == ReservationStatus.Completed &&
                        r.ReservationType == ReservationType.Class)
            .SumAsync(r => r.Classes.Count, ct);
    }

    private async Task<IReadOnlyList<GetAnalyticsForAdminResponse.UpcomingClass>> GetUpcomingClasses(CancellationToken ct = default)
    {
        var currentTime = DateTime.UtcNow;

        return await context.Classes
            .Where(c => c.Reservation.ReservationStatus == ReservationStatus.Confirmed &&
                        c.StartsAt > currentTime)
            .Select(c => new GetAnalyticsForAdminResponse.UpcomingClass
            {
                ReservationId = c.ReservationId,
                StartsAt = c.StartsAt,
                EndsAt = c.EndsAt,
                SubjectName = c.Subject.Name,
                StudentName = c.Reservation.Student.User.FirstName + " " + c.Reservation.Student.User.LastName,
            })
            .OrderBy(c => c.StartsAt)
            .Take(UpcomingItemsLimit)
            .ToListAsync(ct);
    }

    private async Task<IReadOnlyList<GetAnalyticsForAdminResponse.UpcomingProject>> GetUpcomingProjects(CancellationToken ct = default)
    {
        var today = userClock.Today;

        return await context.Projects
            .Where(p => p.Reservation.ReservationStatus == ReservationStatus.Confirmed &&
                        p.StartDate >= today &&
                        p.Progress < 100)
            .Select(p => new GetAnalyticsForAdminResponse.UpcomingProject
            {
                ReservationId = p.ReservationId,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                SubjectName = p.Subject.Name,
                StudentName = p.Reservation.Student.User.FirstName + " " + p.Reservation.Student.User.LastName,
            })
            .OrderBy(p => p.StartDate)
            .Take(UpcomingItemsLimit)
            .ToListAsync(ct);
    }

    private async Task<ChartDataYears> GetSubjectsChartYears(int? requestedYear, CancellationToken ct = default)
    {
        var availableYears = await context.Classes
            .Where(c => c.Reservation.ReservationStatus == ReservationStatus.Completed)
            .Select(c => c.Reservation.ReservedAt.Year)
            .Distinct()
            .OrderBy(y => y)
            .ToListAsync(ct);

        return ResolveChartYears(availableYears, requestedYear);
    }

    private async Task<ChartDataYears> GetFinanceChartYears(int? requestedYear, CancellationToken ct = default)
    {
        var availableYears = await context.Reservations
            .Where(r => r.ReservationStatus == ReservationStatus.Completed)
            .Select(r => r.ReservedAt.Year)
            .Distinct()
            .OrderBy(y => y)
            .ToListAsync(ct);

        return ResolveChartYears(availableYears, requestedYear);
    }

    private ChartDataYears ResolveChartYears(List<int> availableYears, int? requestedYear)
    {
        var selectedYear = requestedYear.HasValue && availableYears.Contains(requestedYear.Value)
            ? requestedYear.Value
            : availableYears.Count > 0 ? availableYears[^1] : userClock.Today.Year;

        return new ChartDataYears
        {
            SelectedYear = selectedYear,
            AvailableYears = availableYears
        };
    }

    private async Task<IReadOnlyList<GetAnalyticsForAdminResponse.SubjectChartItem>> GetSubjectChart(ChartDataYears years, CancellationToken ct = default)
    {
        if (years.AvailableYears.Count == 0)
            return [];

        return await context.Classes
            .Where(c => c.Reservation.ReservationStatus == ReservationStatus.Completed &&
                        c.Reservation.ReservedAt.Year == years.SelectedYear)
            .GroupBy(c => c.Subject.Name)
            .Select(g => new GetAnalyticsForAdminResponse.SubjectChartItem
            {
                SubjectName = g.Key,
                NumberOfClasses = g.Count()
            })
            .ToListAsync(ct);
    }

    private async Task<IReadOnlyList<GetAnalyticsForAdminResponse.FinanceChartItem>> GetFinanceChart(ChartDataYears years, CancellationToken ct = default)
    {
        if (years.AvailableYears.Count == 0)
            return [];

        return await context.Reservations
            .Where(r => r.ReservedAt.Year == years.SelectedYear &&
                        r.ReservationStatus == ReservationStatus.Completed)
            .GroupBy(r => r.ReservedAt.Month)
            .Select(g => new GetAnalyticsForAdminResponse.FinanceChartItem
            {
                Month = g.Key,
                Revenue = g.Sum(r => r.PaidAmount),
                Students = g.Select(r => r.StudentId).Distinct().Count()
            })
            .OrderBy(i => i.Month)
            .ToListAsync(ct);
    }
}
