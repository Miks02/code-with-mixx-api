using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Domain.Entities.Projects;

public class ProjectNote : IAuditable
{
    public int Id { get; private set; }
    public string Content { get; private set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int ProjectId { get; private set; }

    public static Result<ProjectNote> Create(string content)
    {
        if(string.IsNullOrWhiteSpace(content))
            return Result<ProjectNote>.Failure(ProjectError.EmptyProjectNote());
        
        var newProjectNote = new ProjectNote
        {
            Content = content,
            CreatedAt = DateTime.UtcNow
        };
        return Result<ProjectNote>.Success(newProjectNote);
    }
    
}