using CodeWithMixx.API.Domain.Entities.Projects;
using CodeWithMixx.API.Domain.Entities.Reservations;

namespace CodeWithMixx.API.Features.Reservations.GetProjectReservationDetails;

public record GetProjectReservationDetailsResponse
{
    public int Id { get; init; }
    public string StudentId { get; init; } = null!;
    public string StudentFullName { get; init; } = null!;
    public string StudentEmail { get; init; } = null!;
    public string? StudentUniversity { get; init; }
    public ReservationStatus ReservationStatus { get; init; }
    public PaymentStatus PaymentStatus { get; init; }
    public decimal TotalPrice { get; init; }
    public decimal PaidAmount { get; init; }
    public decimal DiscountRate { get; init; }
    public decimal Bonus { get; init; }
    public string? Description { get; init; }
    public DateOnly ReservedAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public IReadOnlyList<ProjectDetailsItem> Projects { get; init; } = [];

    public record ProjectDetailsItem
    {
        public int Id { get; init; }
        public int SubjectId { get; init; }
        public string SubjectName { get; init; } = null!;
        public ProjectType ProjectType { get; init; }
        public decimal Price { get; init; }
        public decimal Progress { get; init; }
        public string? GithubLink { get; init; }
        public string? DownloadLink { get; init; }
        public DateOnly ReservedAt { get; init; }
        public DateOnly StartDate { get; init; }
        public DateOnly EndDate { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
        public IReadOnlyList<ProjectNoteItem> Notes { get; init; } = [];
    }

    public record ProjectNoteItem
    {
        public int Id { get; init; }
        public string Content { get; init; } = null!;
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }
}
