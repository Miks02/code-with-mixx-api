using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Reservations;

namespace CodeWithMixx.API.Features.Classes.GetClassReservationsSummary;

public record GetClassReservationsSummaryResponse
{
    public PagedResult<ClassReservationDto> Reservations { get; init; } = null!;
    public int TotalClassesCount { get; init; }
    public int HeldClassesCount { get; init; }
    public int ScheduledClassesCount { get; init; }
    public int CancelledClassesCount { get; init; }
    public NextClassDto? NextClass { get; init; }

    public record ClassReservationDto
    {
        public int Id { get; init; }
        public string StudentId { get; init; } = null!;
        public string StudentFullName { get; init; } = null!;
        public ReservationStatus ReservationStatus { get; init; }
        public PaymentStatus PaymentStatus { get; init; }
        public decimal TotalPrice { get; init; }
        public decimal PaidAmount { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime StartsAt { get; init; }
        public IReadOnlyList<ClassDto> Classes { get; init; } = [];

        public record ClassDto
        {
            public string SubjectName { get; init; } = null!;
            public decimal Price { get; init; }
            public DateTime StartsAt { get; init; }
            public DateTime EndsAt { get; init; }
        }
    }

    public record NextClassDto
    {
        public int ReservationId { get; init; }
        public string StudentFullName { get; init; } = null!;
        public DateTime StartsAt { get; init; }
        public string? University { get; init; }
        public string SubjectName { get; init; } = null!;
    }
}
