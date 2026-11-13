using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Users;
using CodeWithMixx.API.Domain.ErrorCatalog;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Account.RequestEmailUpdate;

public class RequestEmailUpdateHandler(
    UserManager<User> userManager,
    IUserProvider userProvider,
    IAuthEmailSender emailSender) : IHandler<RequestEmailUpdateRequest, Result>
{
    public async Task<Result> HandleAsync(RequestEmailUpdateRequest request, CancellationToken ct = default)
    {
        var user = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userProvider.GetUserId(), ct); 
        if(user is null)
            return Result.Failure(UserError.NotFound());
        
        var oldEmail = user.Email!;
        
        if(string.Compare(oldEmail, request.NewEmail, StringComparison.OrdinalIgnoreCase) == 0)
            return Result.Failure(AuthError.CannotChangeEmailToSameEmail(user.Id));
        
        if(await userManager.Users.AnyAsync(u => u.Email == request.NewEmail, ct))
            return Result.Failure(AuthError.EmailAlreadyInUse(user.Id, request.NewEmail));

        var passwordCheck = await userManager.CheckPasswordAsync(user, request.Password);

        if (!passwordCheck)
            return Result.Failure(AuthError.InvalidCurrentPassword());
        
        var token = await userManager.GenerateChangeEmailTokenAsync(user, request.NewEmail);

        await emailSender.SendTokenForEmailChangeAsync(user.Email!, request.NewEmail, token);

        return Result.Success();
    }
}