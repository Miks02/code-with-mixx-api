using CodeWithMixx.API.Domain.Entities.Classes;
using CodeWithMixx.API.Domain.Entities.Projects;

namespace CodeWithMixx.API.Domain.Entities.Subjects;

public class Subject : IAuditable, ISoftDeletable
{
    public int Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public ICollection<Class> Classes { get; private set; } = [];
    public ICollection<Project> Projects { get; private set; } = [];
    
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
        Archive();
    }

    public void Archive()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
    }

    public void Update(string name, string description)
    {
        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }
}