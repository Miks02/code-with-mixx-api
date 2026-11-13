using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.RefreshTokens;
using CodeWithMixx.API.Domain.Entities.Users;
using CodeWithMixx.API.Domain.ErrorCatalog;
using CodeWithMixx.API.Features.Authentication.Common;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UAParser;

namespace CodeWithMixx.API.Features.Authentication.Login;

public class LoginHandler(
    UserManager<User> userManager,
    ITokenService tokenService,
    AppDbContext context,
    ICookieProvider cookieProvider,
    IUserProvider userProvider,
    Parser uaParser) : IHandler<LoginRequest, Result<LoginResponse>>
{

    public async Task<Result<LoginResponse>> HandleAsync(LoginRequest request, CancellationToken ct = default)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(ct);

        var user = await userManager.Users.FirstOrDefaultAsync(u => u.Email == request.Email, ct);
        if (user is null)
            return Result<LoginResponse>.Failure(AuthError.LoginFailed($"User with email {request.Email} has not been found."));

        var isPasswordValid = await userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
            return Result<LoginResponse>.Failure(AuthError.LoginFailed("Invalid password."));

        if (user.AccountStatus == AccountStatus.Deactivated)
            return Result<LoginResponse>.Failure(AuthError.AccountDeactivated(user.Id));

        var sessionId = RefreshToken.GenerateSessionId();
        var userAgent = userProvider.GetUserAgent();
        
        var tokens = await tokenService.AssignAuthTokens(user, userAgent, sessionId);
        
        cookieProvider.SetAccessTokenCookie(tokens.AccessToken);
        cookieProvider.SetRefreshTokenCookie(tokens.RefreshToken);
        cookieProvider.SetSessionCookie();
        
        user.UpdateLastLogin();
        await userManager.UpdateAsync(user);
        
        var userRoles = (await userManager.GetRolesAsync(user)).ToList();

        var activeSessions = await context.RefreshTokens
            .Where(rt => rt.UserId == user.Id && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
            .Select(rt => new ActiveSessionItem
            {
                SessionId = rt.SessionId,
                ActiveSince = rt.CreatedAt,
                Browser = uaParser.Parse(rt.UserAgent).UA.Family,
                Os = uaParser.Parse(rt.UserAgent).OS.Family,
                Device = SessionDeviceResolver.Resolve(uaParser.Parse(rt.UserAgent)),
                IsCurrentSession = rt.SessionId == sessionId
            })
            .ToListAsync(ct);

        await transaction.CommitAsync(ct);
        
        return Result<LoginResponse>.Success(new LoginResponse
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email!,
            PhoneNumber = user.PhoneNumber!,
            AccountStatus = user.AccountStatus,
            LastLogin = (DateTime) user.LastLoginAt!,
            Roles = userRoles,
            ActiveSessions = activeSessions
        });
    }
}
