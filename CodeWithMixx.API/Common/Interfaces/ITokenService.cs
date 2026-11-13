using CodeWithMixx.API.Domain.Entities.Users;

namespace CodeWithMixx.API.Common.Interfaces;

public interface ITokenService
{
    Task<TokenResponseDto> AssignAuthTokens(User user, string userAgent, Guid sessionId);
    Task RevokeAllUserTokensAsync(string userId);
    string HashRefreshToken(string refreshToken);
}