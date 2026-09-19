namespace CodeWithMixx.API.Domain.Entities.Projects;

public record ProjectUpdateData
{
    public int Id { get; init; }
    public int SubjectId { get; init; }
    public ProjectType ProjectType { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public DateTime ReservedAt { get; init; }
    public string? GithubLink { get; init; }
    public string? DownloadLink { get; init; }
    public decimal Price { get; init; }
    public decimal Progress { get; init; }
}
