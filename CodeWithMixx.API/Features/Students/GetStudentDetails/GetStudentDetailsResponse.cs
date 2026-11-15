using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Features.Reservations.Common;
using CodeWithMixx.API.Features.Students.Common;

namespace CodeWithMixx.API.Features.Students.GetStudentDetails;

public record GetStudentDetailsResponse
{
    public StudentItem Student { get; init; } = null!;
    public ReservationsStats Stats { get; init; } = null!;
    public PagedResult<StudentReservation> Reservations { get; init; } = null!;

    public record ReservationsStats
    {
        public int TotalReservations { get; init; }
        public int ClassReservations { get; init; }
        public int ProjectReservations { get; init; }
        public int TotalClasses { get; init; }
        public int TotalProjects { get; init; }
        public int OverdueReservations { get; init; }
        public decimal TotalPrice { get; init; }
        public decimal TotalPaid { get; init; }
        public decimal TotalRemaining { get; init; }
    }

    public record StudentReservation
    {
        public int Id { get; init; }
        public ReservationType ReservationType { get; init; }
        public ReservationStatus ReservationStatus { get; init; }
        public PaymentStatus PaymentStatus { get; init; }
        public decimal TotalPrice { get; init; }
        public decimal PaidAmount { get; init; }
        public decimal DiscountRate { get; init; }
        public decimal Bonus { get; init; }
        public string? Description { get; init; }
        public DateOnly ReservedAt { get; init; }
        public DateTime CreatedAt { get; init; }
        public IReadOnlyList<ClassItem> Classes { get; init; } = [];
        public IReadOnlyList<ProjectItem> Projects { get; init; } = [];
    }
}
