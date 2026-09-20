using System.Net;
using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Users;
using CodeWithMixx.API.Domain.ErrorCatalog;
using CodeWithMixx.API.Features.Authentication.Common;
using Microsoft.AspNetCore.Identity;

namespace CodeWithMixx.API.Features.Authentication.ResetPassword;

public class ResetPasswordHandler(UserManager<User> userManager, ITokenService tokenService) : IHandler<ResetPasswordRequest, Result>
{
    public async Task<Result> HandleAsync(ResetPasswordRequest request, CancellationToken ct = default)
    {
       var user = await userManager.FindByIdAsync(request.UserId);
       if(user is null) 
           return Result.Failure(AuthError.InvalidPasswordResetToken($"User with id |{request.UserId}| has not been found during password reset."));
       
       var token = WebUtility.UrlDecode(request.Token);
       
       var resetResult = await userManager.ResetPasswordAsync(user, token, request.Password);

       if (!resetResult.Succeeded)
           return Result.Failure(AuthError.InvalidPasswordResetToken($"Password reset failed for user with id |{request.UserId}|."));
       
       await tokenService.RevokeAllUserTokensAsync(user.Id);
       
       return Result.Success();
    }
}