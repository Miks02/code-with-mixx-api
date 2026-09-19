using CodeWithMixx.API.Domain.Entities.Projects;
using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Features.Reservations.CreateClassReservation;

namespace CodeWithMixx.API.Features.Reservations.CreateProjectReservation;

public record CreateProjectReservationRequest
{
    public string StudentId { get; init; } = null!;
    public decimal PaidAmount { get; init; }
    public decimal? TotalPrice { get; init; }
    public ReservationStatus ReservationStatus { get; init; }
    public IReadOnlyList<ProjectItem> Projects { get; init; } = []; 

    public record ProjectItem
    {
        public int SubjectId { get; init; }
        public decimal Price { get; init; }
        public ProjectType ProjectType { get; init; }
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
        public DateTime ReservedAt { get; init; }
        public string? GithubLink { get; init; }
        public string? DownloadLink { get; init; }
        public decimal Progress { get; init; }
        public IReadOnlyList<string> Notes { get; init; } = [];
    }
}