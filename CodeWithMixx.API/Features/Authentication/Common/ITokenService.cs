using CodeWithMixx.API.Domain.Entities.Users;

namespace CodeWithMixx.API.Features.Authentication.Common;

public interface ITokenService
{
    Task<TokenResponseDto> AssignAuthTokens(User user);
    Task RevokeAllUserTokensAsync(string userId);
}