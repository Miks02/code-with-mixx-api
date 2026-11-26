using CodeWithMixx.API.Domain.Entities.Reservations;

namespace CodeWithMixx.API.Features.Analytics.GetAnalyticsForStudent;

public record GetAnalyticsForStudentResponse
{
    public decimal TotalPaid { get; init; }
    public int CompletedClasses { get; init; }
    public int ActiveProjects { get; init; }
    public int HeldSubjects { get; init; }

    public IReadOnlyList<UpcomingClass> UpcomingClasses { get; init; } = [];
    public IReadOnlyList<ActiveProject> ActiveProjectsList { get; init; } = [];
    public ClassReservationSummary ClassReservationsSummary { get; init; } = null!;
    public BalanceSummary OpenBalanceSummary { get; init; } = null!;
    public IReadOnlyList<RecentPayment> RecentPayments { get; init; } = [];

    public record UpcomingClass
    {
        public int ReservationId { get; init; }
        public string ClassName { get; init; } = null!;
        public string SubjectName { get; init; } = null!;
        public DateTime StartsAt { get; init; }
        public DateTime EndsAt { get; init; }
    }

    public record ActiveProject
    {
        public int ReservationId { get; init; }
        public string ProjectName { get; init; } = null!;
        public string SubjectName { get; init; } = null!;
        public DateOnly StartDate { get; init; }
        public DateOnly EndDate { get; init; }
    }

    public record ClassReservationSummary
    {
        public int Total { get; init; }
        public decimal TotalHours { get; init; }
        public IReadOnlyList<ClassReservationItem> ClassReservationItems { get; init; } = [];
    }

    public record ClassReservationItem
    {
        public int ReservationId { get; init; }
        public int ScheduledClassesCount { get; init; }
        public int CompletedClassesCount { get; init; }
        public DateOnly ReservedAt { get; init; }
        public ReservationStatus ReservationStatus { get; init; }
    }

    public record BalanceSummary
    {
        public decimal RemainingDebt { get; init; }
        public int OverdueReservationsCount { get; init; }
        public IReadOnlyList<OpenBalanceItem> OpenBalanceItems { get; init; } = [];
    }

    public record OpenBalanceItem
    {
        public int ReservationId { get; init; }
        public ReservationType ReservationType { get; init; }
        public decimal TotalPrice { get; init; }
        public decimal PaidAmount { get; init; }
        public decimal RemainingAmount { get; init; }
        public PaymentStatus PaymentStatus { get; init; }
    }

    public record RecentPayment
    {
        public int TransactionId { get; init; }
        public int ReservationId { get; init; }
        public ReservationType ReservationType { get; init; }
        public decimal Amount { get; init; }
        public DateTime TransactionDate { get; init; }
    }
}
