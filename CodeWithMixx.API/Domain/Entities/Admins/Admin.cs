using CodeWithMixx.API.Domain.Entities.Users;

namespace CodeWithMixx.API.Domain.Entities.Admins;

public class Admin
{
    public User User { get; set; } = null!;
    public string UserId { get; set; } = null!;
}