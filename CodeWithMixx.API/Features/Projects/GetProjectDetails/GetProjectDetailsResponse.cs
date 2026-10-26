using CodeWithMixx.API.Domain.Entities.Projects;

namespace CodeWithMixx.API.Features.Projects.GetProjectDetails;

public record GetProjectDetailsResponse
{
    public int Id { get; init; }
    public int ReservationId { get; init; }
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
    public bool IsDeleted { get; init; }
    public bool IsReservationArchived { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public IReadOnlyList<ProjectNoteItem> Notes { get; init; } = [];

    public record ProjectNoteItem
    {
        public int Id { get; init; }
        public string Content { get; init; } = null!;
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }
}
