using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Users;
using Microsoft.AspNetCore.Identity;

namespace CodeWithMixx.API.Features.Authentication.SendResetPasswordLink;

public class SendResetPasswordLinkHandler(UserManager<User> userManager, IConfiguration configuration) 
    : IHandler<SendResetPasswordLinkRequest, Result>
{
    public async Task<Result> HandleAsync(SendResetPasswordLinkRequest linkRequest, CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(linkRequest.Email);

        if (user is null)
            return Result.Success();
        
        var url = $"{configuration["Web:ClientUrl"]}/reset-password?token=";
        
        var token = await userManager.GeneratePasswordResetTokenAsync(user);

        url += token;

        Console.WriteLine($"Reset password link for user {user.Email}: {url}");
        return Result.Success();
    }
}