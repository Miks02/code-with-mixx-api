using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Users;
using CodeWithMixx.API.Domain.ErrorCatalog;
using CodeWithMixx.API.Features.Authentication.Common;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;

namespace CodeWithMixx.API.Features.Authentication.Login;

public class LoginHandler(
    UserManager<User> userManager,
    ITokenService tokenService,
    AppDbContext context,
    ICookieProvider cookieProvider) : IHandler<LoginRequest, Result>
{

    public async Task<Result> HandleAsync(LoginRequest request, CancellationToken ct = default)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(ct);

        try
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null)
                return Result.Failure(AuthError.LoginFailed($"User with email {request.Email} has not been found."));

            var isPasswordValid = await userManager.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid)
                return Result.Failure(AuthError.LoginFailed("Invalid password."));

            var tokens = await tokenService.AssignAuthTokens(user);
            
            cookieProvider.SetAccessTokenCookie(tokens.AccessToken);
            cookieProvider.SetRefreshTokenCookie(tokens.RefreshToken);
            
            user.UpdateLastLogin();
            await userManager.UpdateAsync(user);

            await transaction.CommitAsync(ct);
            return Result.Success();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }
}