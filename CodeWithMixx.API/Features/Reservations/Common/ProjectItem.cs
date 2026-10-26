using CodeWithMixx.API.Domain.Entities.Projects;

namespace CodeWithMixx.API.Features.Reservations.Common;

public record ProjectItem
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
    public int NotesCount { get; init; }
};
