using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Features.Authentication.Common;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Authentication.Logout;

public class LogoutHandler(
    AppDbContext context,
    ICookieProvider cookieProvider, 
    ITokenService tokenService) : IHandler
{
    public async Task<Result> Handle(CancellationToken ct = default)
    {
        var refreshToken = cookieProvider.GetRefreshTokenCookie();

        if (string.IsNullOrWhiteSpace(refreshToken))
            return Result.Success();
        
        cookieProvider.RemoveAuthCookies();
        var hashedRefreshToken = tokenService.HashRefreshToken(refreshToken);
        
        var oldRefreshToken = await context.RefreshTokens
            .Where(rt => rt.TokenHash == hashedRefreshToken && DateTime.UtcNow < rt.ExpiresAt && !rt.RevokedAt.HasValue)
            .FirstOrDefaultAsync(ct);

        if (oldRefreshToken is null)
            return Result.Success();
        
        oldRefreshToken.Revoke();
        await context.SaveChangesAsync(ct);

        return Result.Success();
    }
}