using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.ErrorCatalog;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Authentication.LogoutBySessionId;

public class LogoutBySessionIdHandler(AppDbContext context, IUserProvider userProvider) : IHandler<LogoutBySessionIdRequest, Result>
{
    public async Task<Result> HandleAsync(LogoutBySessionIdRequest request, CancellationToken ct = default)
    {
        var userId = userProvider.GetUserId();
        
        var session = await context.RefreshTokens
            .Where(rt => rt.RevokedAt == null && 
                         rt.ExpiresAt > DateTime.UtcNow &&
                         rt.SessionId.ToString() == request.SessionId &&
                         rt.UserId == userId)
            .FirstOrDefaultAsync(ct);

        if (session is null)
            return Result.Failure(AuthError.SessionNotFound(request.SessionId));
        
        session.Revoke();
        
        await context.SaveChangesAsync(ct);

        return Result.Success();
    }
}