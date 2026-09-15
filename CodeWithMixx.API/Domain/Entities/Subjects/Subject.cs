using CodeWithMixx.API.Domain.Entities.Classes;

namespace CodeWithMixx.API.Domain.Entities.Subjects;

public class Subject : IAuditable, ISoftDeletable
{
    public int Id { get; init; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public ICollection<Class> Classes { get; set; } = [];
    
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