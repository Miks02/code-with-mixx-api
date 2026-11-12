using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Users;
using CodeWithMixx.API.Domain.ErrorCatalog;
using Microsoft.AspNetCore.Identity;

namespace CodeWithMixx.API.Features.Account.UpdatePassword;

public class UpdatePasswordHandler(
    UserManager<User> userManager,
    ITokenService tokenService,
    ICookieProvider cookieProvider,
    IUserProvider userProvider) : IHandler<UpdatePasswordRequest, Result>
{
    public async Task<Result> HandleAsync(UpdatePasswordRequest request, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userProvider.GetUserId());
        if (user is null)
            return Result.Failure(UserError.NotFound(userProvider.GetUserId()));
        
        var isCurrentPasswordValid = await userManager.CheckPasswordAsync(user, request.CurrentPassword);
        if (!isCurrentPasswordValid)
            return Result.Failure(AuthError.InvalidCurrentPassword());
        
        var changePasswordResult = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!changePasswordResult.Succeeded)
            return Result.Failure(changePasswordResult.Errors.First());
        
        await tokenService.RevokeAllUserTokensAsync(user.Id);
        cookieProvider.RemoveAuthCookies();
        return Result.Success();
    }
}