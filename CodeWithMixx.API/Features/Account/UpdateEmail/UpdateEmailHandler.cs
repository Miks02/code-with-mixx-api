using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Users;
using CodeWithMixx.API.Domain.ErrorCatalog;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Account.UpdateEmail;

public class UpdateEmailHandler(
    UserManager<User> userManager,
    AppDbContext context,
    IUserProvider userProvider,
    IAuthEmailSender emailSender) : IHandler<UpdateEmailRequest, Result<UpdateEmailResponse>>
{
    public async Task<Result<UpdateEmailResponse>> HandleAsync(UpdateEmailRequest request, CancellationToken ct = default)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userProvider.GetUserId(), ct);
        if(user is null)
            return Result<UpdateEmailResponse>.Failure(UserError.NotFound());
        
        var oldEmail = user.Email!;
        
        if(string.Compare(user.Email, request.NewEmail, StringComparison.OrdinalIgnoreCase) == 0) 
            return Result<UpdateEmailResponse>.Failure(AuthError.CannotChangeEmailToSameEmail(user.Id));
        
        if(!await userManager.CheckPasswordAsync(user, request.Password))
            return Result<UpdateEmailResponse>.Failure(AuthError.InvalidCurrentPassword());
        
        var emailUpdateResult = await userManager.ChangeEmailAsync(user, request.NewEmail, request.Token);

        if (!emailUpdateResult.Succeeded)
        {
            var error = emailUpdateResult.Errors.First();
            if(error.Code == "DuplicateUserName" || error.Code == "DuplicateEmail")
                return Result<UpdateEmailResponse>.Failure(AuthError.EmailAlreadyInUse(user.Id, request.NewEmail));
            if (error.Code == "InvalidToken")
                return Result<UpdateEmailResponse>.Failure(AuthError.InvalidEmailResetToken());

            return Result<UpdateEmailResponse>.Failure(error);
        }
        
        var setUserNameResult = await userManager.SetUserNameAsync(user, request.NewEmail);

        if (!setUserNameResult.Succeeded)
        {
            var error = setUserNameResult.Errors.First();
            if(error.Code == "DuplicateUserName" || error.Code == "DuplicateEmail")
                return Result<UpdateEmailResponse>.Failure(AuthError.EmailAlreadyInUse(user.Id, request.NewEmail));

            return Result<UpdateEmailResponse>.Failure(error);
        }
        
        await emailSender.SendEmailChangeConfirmationAsync(oldEmail);

        return Result<UpdateEmailResponse>.Success(new UpdateEmailResponse { Email = user.Email!});
    }
}