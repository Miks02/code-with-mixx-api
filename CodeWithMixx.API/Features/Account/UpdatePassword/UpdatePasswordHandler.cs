using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Users;
using CodeWithMixx.API.Domain.ErrorCatalog;
using Microsoft.AspNetCore.Identity;

namespace CodeWithMixx.API.Features.Account.UpdatePassword;

public class UpdatePasswordHandler(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    ITokenService tokenService,
    ICookieProvider cookieProvider,
    IUserProvider userProvider,
    IAuthEmailSender emailSender) : IHandler<UpdatePasswordRequest, Result>
{
    public async Task<Result> HandleAsync(UpdatePasswordRequest request, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userProvider.GetUserId());
        if (user is null)
            return Result.Failure(UserError.NotFound(userProvider.GetUserId()));
        
        var passwordCheck = await signInManager.CheckPasswordSignInAsync(user, request.CurrentPassword, lockoutOnFailure: true);
        if (passwordCheck.IsLockedOut)
            return Result.Failure(AuthError.AccountLocked());

        if (!passwordCheck.Succeeded)
            return Result.Failure(AuthError.InvalidCurrentPassword());
        
        var changePasswordResult = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!changePasswordResult.Succeeded)
            return Result.Failure(changePasswordResult.Errors.First());
        
        await tokenService.RevokeAllUserTokensAsync(user.Id);
        cookieProvider.RemoveAuthCookies();
        
        await emailSender.SendPasswordChangeConfirmationAsync(user.Email!);
        
        return Result.Success();
    }
}