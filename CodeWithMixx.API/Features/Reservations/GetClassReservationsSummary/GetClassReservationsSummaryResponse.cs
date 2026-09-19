using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Features.Reservations.Common;

namespace CodeWithMixx.API.Features.Reservations.GetClassReservationsSummary;

public record GetClassReservationsSummaryResponse
{
    public PagedResult<ClassReservation> Reservations { get; init; } = null!;
    public int TotalClassesCount { get; init; }
    public int HeldClassesCount { get; init; }
    public int ScheduledClassesCount { get; init; }
    public int CancelledClassesCount { get; init; }
    public NextClass? NextClassItem { get; init; }

    public record ClassReservation
    {
        public int Id { get; init; }
        public string StudentId { get; init; } = null!;
        public string StudentFullName { get; init; } = null!;
        public ReservationStatus ReservationStatus { get; init; }
        public PaymentStatus PaymentStatus { get; init; }
        public decimal TotalPrice { get; init; }
        public decimal PaidAmount { get; init; }
        public decimal DiscountRate { get; init; }
        public decimal Bonus { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime StartsAt { get; init; }
        public IReadOnlyList<ClassItem> Classes { get; init; } = [];
    }

    public record NextClass
    {
        public int ReservationId { get; init; }
        public string StudentFullName { get; init; } = null!;
        public DateTime StartsAt { get; init; }
        public string? University { get; init; }
        public string SubjectName { get; init; } = null!;
    }
}
