using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Domain.Entities.PaymentTransactions;
using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Analytics.GetAnalyticsForStudent;

public class GetAnalyticsForStudentHandler(AppDbContext context, IUserProvider userProvider)
    : IHandler<GetAnalyticsForStudentRequest, GetAnalyticsForStudentResponse>
{
    private const int RecentPaymentsLimit = 5;

    public async Task<GetAnalyticsForStudentResponse> HandleAsync(GetAnalyticsForStudentRequest request, CancellationToken ct = default)
    {
        var studentId = userProvider.GetUserId();
        var now = DateTime.UtcNow;

        var reservationStats = await GetReservationStats(studentId, ct);
        var classReservations = await GetClassReservations(studentId, now, ct);
        var activeProjects = await GetActiveProjects(studentId, ct);
        var heldSubjects = await context.Classes
            .Where(r => r.Reservation.StudentId == studentId && 
                        r.StartsAt < now)
            .Select(c => c.Subject.Id)
            .Distinct()
            .CountAsync(ct);
        var nonCancelledClassReservations = classReservations
            .Where(r => r.ReservationStatus != ReservationStatus.Cancelled)
            .ToList();

        return new GetAnalyticsForStudentResponse
        {
            TotalPaid = reservationStats.TotalPaid,
            CompletedClasses = nonCancelledClassReservations.Sum(r => r.CompletedClassesCount),
            HeldSubjects = heldSubjects,
            ActiveProjects = activeProjects.Count,
            UpcomingClasses = await GetUpcomingClasses(studentId, now, ct),
            ActiveProjectsList = activeProjects,
            ClassReservationsSummary = new GetAnalyticsForStudentResponse.ClassReservationSummary
            {
                Total = nonCancelledClassReservations.Count,
                TotalHours = Math.Round((decimal)nonCancelledClassReservations.Sum(r => r.TotalMinutes) / 60, 1),
                ClassReservationItems = classReservations
                    .Select(r => new GetAnalyticsForStudentResponse.ClassReservationItem
                    {
                        ReservationId = r.ReservationId,
                        ScheduledClassesCount = r.ScheduledClassesCount,
                        CompletedClassesCount = r.CompletedClassesCount,
                        ReservedAt = r.ReservedAt,
                        ReservationStatus = r.ReservationStatus
                    })
                    .ToList()
            },
            OpenBalanceSummary = new GetAnalyticsForStudentResponse.BalanceSummary
            {
                RemainingDebt = reservationStats.RemainingDebt,
                OverdueReservationsCount = reservationStats.OverdueReservationsCount,
                OpenBalanceItems = await GetOpenBalanceItems(studentId, ct)
            },
            RecentPayments = await GetRecentPayments(studentId, ct)
        };
    }

    private async Task<ReservationStats> GetReservationStats(string studentId, CancellationToken ct)
    {
        return await context.Reservations
            .Where(r => r.StudentId == studentId)
            .GroupBy(r => 1)
            .Select(g => new ReservationStats
            {
                TotalPaid = g.Sum(r => r.PaidAmount),
                RemainingDebt = g.Sum(r => r.ReservationStatus != ReservationStatus.Cancelled && r.TotalPrice > r.PaidAmount
                    ? r.TotalPrice - r.PaidAmount
                    : 0),
                OverdueReservationsCount = g.Count(r => r.PaymentStatus == PaymentStatus.Overdue)
            })
            .FirstOrDefaultAsync(ct) ?? new ReservationStats();
    }

    private async Task<List<ClassReservationStats>> GetClassReservations(string studentId, DateTime now, CancellationToken ct)
    {
        return await context.Classes
            .Where(c => c.Reservation.StudentId == studentId &&
                        c.Reservation.ReservationType == ReservationType.Class)
            .GroupBy(c => new { c.ReservationId, c.Reservation.ReservationStatus, c.Reservation.ReservedAt })
            .OrderBy(g => g.Min(c => c.StartsAt))
            .Select(g => new ClassReservationStats
            {
                ReservedAt = g.Key.ReservedAt,
                ReservationId = g.Key.ReservationId,
                ReservationStatus = g.Key.ReservationStatus,
                ScheduledClassesCount = g.Count(),
                CompletedClassesCount = g.Count(c => c.EndsAt <= now),
                TotalMinutes = g.Sum(c => (c.EndsAt - c.StartsAt).TotalMinutes),
            })
            .ToListAsync(ct);
    }

    private async Task<IReadOnlyList<GetAnalyticsForStudentResponse.UpcomingClass>> GetUpcomingClasses(string studentId, DateTime now, CancellationToken ct)
    {
        return await context.Classes
            .Where(c => c.Reservation.StudentId == studentId &&
                        c.Reservation.ReservationStatus == ReservationStatus.Confirmed &&
                        c.StartsAt > now && c.StartsAt < now.AddDays(7))
            .OrderBy(c => c.StartsAt)
            .Select(c => new GetAnalyticsForStudentResponse.UpcomingClass
            {
                ReservationId = c.ReservationId,
                ClassName = c.Name,
                SubjectName = c.Subject.Name,
                StartsAt = c.StartsAt,
                EndsAt = c.EndsAt
            })
            .ToListAsync(ct);
    }

    private async Task<IReadOnlyList<GetAnalyticsForStudentResponse.ActiveProject>> GetActiveProjects(string studentId, CancellationToken ct)
    {
        return await context.Projects
            .Where(p => p.Reservation.StudentId == studentId &&
                        p.Reservation.ReservationStatus == ReservationStatus.Confirmed &&
                        p.Progress < 100)
            .OrderBy(p => p.StartDate)
            .Select(p => new GetAnalyticsForStudentResponse.ActiveProject
            {
                ReservationId = p.ReservationId,
                ProjectName = p.Name,
                SubjectName = p.Subject.Name,
                StartDate = p.StartDate,
                EndDate = p.EndDate
            })
            .ToListAsync(ct);
    }

    private async Task<IReadOnlyList<GetAnalyticsForStudentResponse.OpenBalanceItem>> GetOpenBalanceItems(string studentId, CancellationToken ct)
    {
        return await context.Reservations
            .Where(r => r.StudentId == studentId &&
                        r.ReservationStatus != ReservationStatus.Cancelled &&
                        r.TotalPrice > r.PaidAmount)
            .OrderBy(r => r.ReservedAt)
            .Select(r => new GetAnalyticsForStudentResponse.OpenBalanceItem
            {
                ReservationId = r.Id,
                ReservationType = r.ReservationType,
                TotalPrice = r.TotalPrice,
                PaidAmount = r.PaidAmount,
                RemainingAmount = r.TotalPrice - r.PaidAmount,
                PaymentStatus = r.PaymentStatus
            })
            .ToListAsync(ct);
    }

    private async Task<IReadOnlyList<GetAnalyticsForStudentResponse.RecentPayment>> GetRecentPayments(string studentId, CancellationToken ct)
    {
        return await context.PaymentTransactions
            .Where(t => t.Reservation.StudentId == studentId &&
                        t.PaymentType == PaymentTransactionType.Payment)
            .OrderByDescending(t => t.TransactionDate)
            .Select(t => new GetAnalyticsForStudentResponse.RecentPayment
            {
                TransactionId = t.Id,
                ReservationId = t.ReservationId,
                ReservationType = t.Reservation.ReservationType,
                Amount = t.Amount.Value,
                TransactionDate = t.TransactionDate
            })
            .Take(RecentPaymentsLimit)
            .ToListAsync(ct);
    }

    private sealed record ReservationStats
    {
        public decimal TotalPaid { get; init; }
        public decimal RemainingDebt { get; init; }
        public int OverdueReservationsCount { get; init; }
    }

    private sealed record ClassReservationStats
    {
        public int ReservationId { get; init; }
        public ReservationStatus ReservationStatus { get; init; }
        public int ScheduledClassesCount { get; init; }
        public int CompletedClassesCount { get; init; }
        public DateOnly ReservedAt { get; init; }
        public double TotalMinutes { get; init; }
        public int HeldSubjects { get; init; }
    }
}
