using System.Text.RegularExpressions;
using CodeWithMixx.API.Domain.Entities.Users;

namespace CodeWithMixx.API.Domain.Entities.RefreshTokens;

public class RefreshToken : IAuditable
{
    public Guid Id { get; private set; }
    public string TokenHash { get; private set; } = null!;
    public string CreatedByIp { get; private set; } = null!;
    public string? ReplacedByTokenHash { get; private set; }
    
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public User User { get; private set; } = null!;
    public string UserId { get; private set; } = null!;
    
    private RefreshToken() {}

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
    }
}