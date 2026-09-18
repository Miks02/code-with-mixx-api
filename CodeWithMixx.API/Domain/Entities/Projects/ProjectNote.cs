namespace CodeWithMixx.API.Domain.Entities.Projects;

public class ProjectNote : IAuditable
{
    public int Id { get; private set; }
    public string Content { get; private set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public int ProjectId { get; private set; }
}