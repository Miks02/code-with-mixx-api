using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Users;
using CodeWithMixx.API.Domain.ErrorCatalog;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Authentication.Login;

public class LoginHandler(
    UserManager<User> userManager,
    ITokenService tokenService,
    AppDbContext context,
    ICookieProvider cookieProvider) : IHandler<LoginRequest, Result<LoginResponse>>
{

    public async Task<Result<LoginResponse>> HandleAsync(LoginRequest request, CancellationToken ct = default)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(ct);

        try
        {
            var user = await userManager.Users.Include(u => u.RefreshTokens).FirstOrDefaultAsync(u => u.Email == request.Email, ct);
            if (user is null)
                return Result<LoginResponse>.Failure(AuthError.LoginFailed($"User with email {request.Email} has not been found."));

            var isPasswordValid = await userManager.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid)
                return Result<LoginResponse>.Failure(AuthError.LoginFailed("Invalid password."));

            if (user.AccountStatus == AccountStatus.Deactivated)
                return Result<LoginResponse>.Failure(AuthError.AccountDeactivated(user.Id));

            var tokens = await tokenService.AssignAuthTokens(user);
            
            cookieProvider.SetAccessTokenCookie(tokens.AccessToken);
            cookieProvider.SetRefreshTokenCookie(tokens.RefreshToken);
            cookieProvider.SetSessionCookie();
            
            user.UpdateLastLogin();
            await userManager.UpdateAsync(user);
            
            var userRoles = (await userManager.GetRolesAsync(user)).ToList();
            await transaction.CommitAsync(ct);
            
            return Result<LoginResponse>.Success(new LoginResponse
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber!,
                AccountStatus = user.AccountStatus,
                Roles = userRoles,
                ActiveSessions = user.RefreshTokens.Count(rt => rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
            });
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }
}