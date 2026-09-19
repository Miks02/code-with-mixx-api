using CodeWithMixx.API.Domain.Entities.Projects;
using CodeWithMixx.API.Domain.Entities.Reservations;

namespace CodeWithMixx.API.Features.Projects.CreateProjectReservation;

public record CreateProjectReservationResponse
{
    public int Id { get; init; }
    public string AdminId { get; init; } = null!;
    public string StudentId { get; init; } = null!;
    public PaymentStatus PaymentStatus { get; init; }
    public ReservationStatus ReservationStatus { get; init; }
    public decimal TotalPrice { get; init; }
    public decimal PaidAmount { get; init; }
    public DateTime CreatedAt { get; init; }
    
    public IReadOnlyList<ProjectItem> Projects { get; init; } = [];
    
    public record ProjectItem
    {
        public int Id { get; init; }
        public int SubjectId { get; init; }
        public string SubjectName { get; init; } = null!;
        public decimal Price { get; init; }
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }   
        public DateTime CreatedAt { get; init; }
        public string? GithubLink { get; init; }
        public string? DownloadLink { get; init; }
        public decimal Progress { get; init; }
        public ProjectType ProjectType { get; init; }
        public DateTime ReservedAt { get; init; }
        public IReadOnlyList<ProjectNoteItem> Notes { get; init; } = [];
    }

    public record ProjectNoteItem
    {
        public int Id { get; init; }
        public string Content { get; init; } = null!;
        public DateTime CreatedAt { get; init; }
    }
}