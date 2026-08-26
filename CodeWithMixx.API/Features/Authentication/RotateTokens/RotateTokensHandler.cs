using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.ErrorCatalog;
using CodeWithMixx.API.Features.Authentication.Common;
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
            .Where(rt => rt.TokenHash == hashedRefreshToken && DateTime.UtcNow < rt.ExpiresAt && !rt.RevokedAt.HasValue)
            .FirstOrDefaultAsync(ct);

        if (oldToken is null)
        {
            cookieProvider.RemoveAuthCookies();
            return Result.Failure(AuthError.ExpiredToken());
        }
        
        var tokens = await tokenService.AssignAuthTokens(oldToken.User);
        
        var newHashedRefreshToken = tokenService.HashRefreshToken(tokens.RefreshToken);
        
        oldToken.Revoke(newHashedRefreshToken);
        await context.SaveChangesAsync(ct);
        
        cookieProvider.SetAccessTokenCookie(tokens.AccessToken);
        cookieProvider.SetRefreshTokenCookie(tokens.RefreshToken);
        
        return Result.Success();
    }
}