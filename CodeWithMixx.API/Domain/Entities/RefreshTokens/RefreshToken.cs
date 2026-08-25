using System.Text.RegularExpressions;
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
    
    public static RefreshToken CreateRefreshToken(string tokenHash, string userId, string userIp, DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(tokenHash) || tokenHash.Length != 44)
            throw new ArgumentException("Token hash must be a valid 44-character SHA-256 Base64 string.", nameof(tokenHash));
        
        return new RefreshToken
        {
            UserId = userId,
            TokenHash = tokenHash,
            CreatedByIp = userIp,
            ExpiresAt = expiresAt
        };
    }

    public void Revoke(string? replacedByTokenHash = null)
    {
        RevokedAt = DateTime.UtcNow;
        ReplacedByTokenHash = replacedByTokenHash;
        UpdatedAt = DateTime.UtcNow;
    }
}