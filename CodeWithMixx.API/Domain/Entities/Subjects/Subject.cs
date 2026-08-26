namespace CodeWithMixx.API.Domain.Entities.Subjects;

public class Subject : IAuditable, ISoftDeletable
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public string Description { get; init; } = null!;
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    private Subject() {}
    
    public static Subject Create(string name, string description)
    {
        return new Subject
        {
            Name = name,
            Description = description,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void Delete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }
}