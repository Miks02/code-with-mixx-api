namespace CodeWithMixx.API.Features.Projects.UpdateProject;

public record UpdateProjectRequest
{
    public int ProjectId { get; init; }
    public decimal Price { get; init; }
    public decimal Progress { get; init; }
    public string? GithubLink { get; init; }
    public string? DownloadLink { get; init; }

    public IReadOnlyList<int> NotesToDelete { get; init; } = [];
    public IReadOnlyList<string> NotesToAdd { get; init; } = [];
};
