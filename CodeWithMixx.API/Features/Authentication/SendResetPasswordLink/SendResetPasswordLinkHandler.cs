using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Users;
using CodeWithMixx.API.Features.Authentication.Common;
using Microsoft.AspNetCore.Identity;

namespace CodeWithMixx.API.Features.Authentication.SendResetPasswordLink;

public class SendResetPasswordLinkHandler(
    UserManager<User> userManager,
    IAuthEmailSender authEmailSender) 
    : IHandler<SendResetPasswordLinkRequest, Result>
{
    public async Task<Result> HandleAsync(SendResetPasswordLinkRequest linkRequest, CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(linkRequest.Email);

        if (user is null)
            return Result.Success();
        
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        
        await authEmailSender.SendPasswordResetEmailAsync(user.Email!, token);
        return Result.Success();
    }
}