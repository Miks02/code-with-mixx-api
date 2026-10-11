using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Projects;
using CodeWithMixx.API.Domain.Entities.Reservations;

namespace CodeWithMixx.API.Features.Reservations.GetProjectReservationsSummary;

public record GetProjectReservationsSummaryResponse
{
    public PagedResult<ProjectReservation> Reservations { get; init; } = null!;
    public int TotalProjects { get; init; }
    public int ActiveProjects { get; init; }
    public int CompletedProjects { get; init; }
    public int PendingProjects { get; init; }

    public record ProjectReservation
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
        public IReadOnlyList<ProjectItem> Projects { get; init; } = [];
    }

    public record ProjectItem
    {
        public int Id { get; init; }
        public int SubjectId { get; init; }
        public string SubjectName { get; init; } = null!;
        public ProjectType ProjectType { get; init; }
        public decimal Price { get; init; }
        public decimal Progress { get; init; }
        public DateTime ReservedAt { get; init; }
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
    }
};
