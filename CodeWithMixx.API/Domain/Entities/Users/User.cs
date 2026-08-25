using Microsoft.AspNetCore.Identity;

namespace CodeWithMixx.API.Domain.Entities.Users;

public class User : IdentityUser, IAuditable
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;

    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public static User CreateUser(string firstName, string lastName, string email, string phoneNumber)
    {
        return new User
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            UserName = email,
            PhoneNumber = phoneNumber,
            CreatedAt = DateTime.UtcNow
        };
    }
    
    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
    
}