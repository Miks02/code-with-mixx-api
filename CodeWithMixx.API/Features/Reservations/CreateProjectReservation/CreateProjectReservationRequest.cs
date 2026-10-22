using CodeWithMixx.API.Domain.Entities.Projects;
using CodeWithMixx.API.Domain.Entities.Reservations;

namespace CodeWithMixx.API.Features.Reservations.CreateProjectReservation;

public record CreateProjectReservationRequest
{
    public string StudentId { get; init; } = null!;
    public decimal PaidAmount { get; init; }
    public decimal? TotalPrice { get; init; }
    public DateOnly ReservedAt { get; init; }
    public ReservationStatus ReservationStatus { get; init; }
    public IReadOnlyList<ProjectItem> Projects { get; init; } = []; 

    public record ProjectItem
    {
        public int SubjectId { get; init; }
        public decimal Price { get; init; }
        public ProjectType ProjectType { get; init; }
        public DateOnly StartDate { get; init; }
        public DateOnly EndDate { get; init; }
        public DateOnly ReservedAt { get; init; }
        public string? GithubLink { get; init; }
        public string? DownloadLink { get; init; }
        public decimal Progress { get; init; }
        public IReadOnlyList<ProjectNoteItem> Notes { get; init; } = [];
    }

    public record ProjectNoteItem
    {
        public string Content { get; init; } = null!;
    }
}