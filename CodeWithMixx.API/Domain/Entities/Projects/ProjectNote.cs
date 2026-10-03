using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Domain.Entities.Projects;

public class ProjectNote : IAuditable
{
    public int Id { get; private set; }
    public string Content { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public int ProjectId { get; private set; }

    private ProjectNote() {}

    public static Result<ProjectNote> Create(string content)
    {
        if(string.IsNullOrWhiteSpace(content))
            return Result<ProjectNote>.Failure(ProjectError.EmptyProjectNote());
        
        var newProjectNote = new ProjectNote
        {
            Content = content,
        };
        return Result<ProjectNote>.Success(newProjectNote);
    }
    
}