using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Users;
using CodeWithMixx.API.Domain.ErrorCatalog;
using Microsoft.AspNetCore.Identity;

namespace CodeWithMixx.API.Features.Authentication.ActivateAccount;

public class ActivateAccountHandler(UserManager<User> userManager) : IHandler<ActivateAccountRequest, Result>
{
    public async Task<Result> HandleAsync(ActivateAccountRequest request, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(request.UserId); 
        
        if(user is null) 
            return Result.Failure(UserError.NotFound(request.UserId)); 
        
        if(!await userManager.VerifyUserTokenAsync(user, "InviteTokenProvider", "Invitation", request.Token))
            return Result.Failure(AuthError.InvalidInvitationToken(request.UserId));
        
        if(await userManager.HasPasswordAsync(user))
            return Result.Failure(UserError.AlreadyActivated(request.UserId));
        
        var setPasswordResult = await userManager.AddPasswordAsync(user, request.Password); 
        
        if(!setPasswordResult.Succeeded)
           return Result.Failure(setPasswordResult.Errors.First());
        
        var stampResult = await userManager.UpdateSecurityStampAsync(user); 
        
        if(!stampResult.Succeeded)
            return Result.Failure(stampResult.Errors.First());
        
        return Result.Success();
    }
}