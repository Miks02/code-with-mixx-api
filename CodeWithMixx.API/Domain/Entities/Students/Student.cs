using CodeWithMixx.API.Domain.Entities.Users;

namespace CodeWithMixx.API.Domain.Entities.Students;

public class Student : ISoftDeletable
{
    public User User { get; set; } = null!;
    public string UserId { get; set; } = null!;
    
    public string? University { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public static Student Create(string userId, string? university)
    {
        return new Student
        {
            UserId = userId,
            University = university
        };
    }

    public void Delete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        University = null;
    }


}