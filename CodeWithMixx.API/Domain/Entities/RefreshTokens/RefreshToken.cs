using CodeWithMixx.API.Domain.Entities.Users;

namespace CodeWithMixx.API.Domain.Entities.RefreshTokens;

public class RefreshToken : IAuditable
{
    public Guid Id { get; set; }
    public string TokenHash { get; set; } = null!;
    public string CreatedByIp { get; set; } = null!;
    public string? ReplacedByTokenHash { get; set; }
    
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public User User { get; set; } = null!;
    public string UserId { get; set; } = null!;
}