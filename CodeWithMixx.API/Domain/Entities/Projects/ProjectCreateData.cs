namespace CodeWithMixx.API.Domain.Entities.Projects;

public record ProjectCreateData
{
    public int SubjectId { get; init; }
    public ProjectType ProjectType { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public DateOnly ReservedAt { get; init; }
    public string? GithubLink { get; init; }
    public string? DownloadLink { get; init; }
    public decimal Price { get; init; }
    public decimal Progress { get; init; }
    public IReadOnlyList<ProjectNoteData> Notes { get; init; } = [];
    
    public DateOnly Today { get; init; }

    public record ProjectNoteData
    {
        public string Content { get; init; } = null!;
        
    }
    
};