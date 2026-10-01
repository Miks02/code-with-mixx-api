using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Domain.Entities.Users;

namespace CodeWithMixx.API.Domain.Entities.Students;

public class Student : ISoftDeletable
{
    public User User { get; private set; } = null!;
    public string UserId { get; private set; } = null!;
    
    public string? University { get; private set; }

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public ICollection<Reservation> Reservations { get; private set; } = [];

    private Student() {}

    public static Student Create(string userId, string? university)
    {
        return new Student
        {
            UserId = userId,
            University = university
        };
    }

    public void ChangeUniversity(string? newUniversity)
    {
        University = newUniversity;
    }
    

    public void Delete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        University = null;
    }


}