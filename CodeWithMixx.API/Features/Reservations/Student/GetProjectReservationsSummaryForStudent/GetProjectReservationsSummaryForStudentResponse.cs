using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Features.Reservations.Common;

namespace CodeWithMixx.API.Features.Reservations.Student.GetProjectReservationsSummaryForStudent;

public record GetProjectReservationsSummaryForStudentResponse
{
    public PagedResult<ProjectReservation> Reservations { get; init; } = null!;
    public int TotalProjects { get; init; }
    public int ActiveProjects { get; init; }
    public int CompletedProjects { get; init; }
    public int PendingProjects { get; init; }

    public record ProjectReservation
    {
        public int Id { get; init; }
        public ReservationStatus ReservationStatus { get; init; }
        public PaymentStatus PaymentStatus { get; init; }
        public decimal TotalPrice { get; init; }
        public decimal PaidAmount { get; init; }
        public decimal DiscountRate { get; init; }
        public decimal Bonus { get; init; }
        public string? Description { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
        public DateOnly ReservedAt { get; init; }
        public IReadOnlyList<ProjectItem> Projects { get; init; } = [];
    }
};
