using System.Net;
using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Users;
using CodeWithMixx.API.Domain.ErrorCatalog;
using Microsoft.AspNetCore.Identity;

namespace CodeWithMixx.API.Features.Students.SendInvitation;

public class SendInvitationHandler(UserManager<User> userManager, IAuthEmailSender authEmailSender) : IHandler<SendInvitationRequest, Result>
{
    public async Task<Result> HandleAsync(SendInvitationRequest request, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(request.Id); 
        
        if(user is null)
            return Result.Failure(UserError.NotFound(request.Id));
        
        if(user.AccountStatus == AccountStatus.Deactivated)
            return Result.Failure(AuthError.AccountDeactivated(user.Id));
        
        if(!await userManager.IsInRoleAsync(user, "Student"))
            return Result.Failure(UserError.NotAStudent(request.Id));
        
        if(user.PasswordHash is not null) 
            return Result.Failure(UserError.AlreadyActivated(request.Id));
        
        var stampResult = await userManager.UpdateSecurityStampAsync(user);

        if (!stampResult.Succeeded)
            return stampResult.HandleIdentityResult(); 
        
        var token = await userManager.GenerateUserTokenAsync(user, "InviteTokenProvider", "Invitation");
        
        await authEmailSender.SendInvitationEmailAsync(user.Email!, user.Id, token);
        
        return Result.Success();
    }
}