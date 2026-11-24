using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.ErrorCatalog;
using CodeWithMixx.API.Infrastructure.Exceptions;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Authentication.RotateTokens;

public class RotateTokensHandler(
    ICookieProvider cookieProvider,
    ITokenService tokenService,
    AppDbContext context) : IHandler<RotateTokensRequest, Result>
{
    public async Task<Result> HandleAsync(RotateTokensRequest request, CancellationToken ct = default)
    {
        var currentRefreshToken = cookieProvider.GetRefreshTokenCookie();
        if (string.IsNullOrEmpty(currentRefreshToken))
            return Result.Failure(AuthError.MissingToken());
        
        var hashedRefreshToken = tokenService.HashRefreshToken(currentRefreshToken); 
        
        var oldToken = await context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenHash == hashedRefreshToken, ct);

        if (oldToken?.ReplacedByTokenHash != null && oldToken.RevokedAt is not null)
        {
            await tokenService.RevokeAllUserTokensAsync(oldToken.UserId);
            throw new RefreshTokenReuseException(oldToken.UserId, oldToken.SessionId);
        }

        if (oldToken is null || oldToken.RevokedAt.HasValue || oldToken.ExpiresAt <= DateTime.UtcNow)
        {
            cookieProvider.RemoveAuthCookies();
            return Result.Failure(AuthError.ExpiredToken());
        }
        
        var tokens = await tokenService.AssignAuthTokens(oldToken.User, oldToken.UserAgent, oldToken.SessionId);
        
        var newHashedRefreshToken = tokenService.HashRefreshToken(tokens.RefreshToken);
        
        oldToken.Revoke(newHashedRefreshToken);
        await context.SaveChangesAsync(ct);
        
        cookieProvider.SetAccessTokenCookie(tokens.AccessToken);
        cookieProvider.SetRefreshTokenCookie(tokens.RefreshToken);
        
        return Result.Success();
    }
}