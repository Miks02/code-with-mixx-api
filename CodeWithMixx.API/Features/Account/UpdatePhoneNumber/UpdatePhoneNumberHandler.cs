using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Account.UpdatePhoneNumber;

public class UpdatePhoneNumberHandler(UserManager<User> userManager, IUserProvider userProvider)
    : IHandler<UpdatePhoneNumberRequest, Result<UpdatePhoneNumberResponse>>
{
    public async Task<Result<UpdatePhoneNumberResponse>> HandleAsync(UpdatePhoneNumberRequest request, CancellationToken ct = default)
    {
        var user = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userProvider.GetUserId(), ct);
        if(user is null)
            return Result<UpdatePhoneNumberResponse>.Failure(UserError.NotFound());

        var phoneNumberUpdateResult = await userManager.SetPhoneNumberAsync(user, request.PhoneNumber);

        if (!phoneNumberUpdateResult.Succeeded)
        {
            var identityErrorCode = phoneNumberUpdateResult.Errors.First().Code;
            return Result<UpdatePhoneNumberResponse>.Failure(UserError.CannotUpdatePhoneNumber(user.Id, identityErrorCode));
        }
        
        return Result<UpdatePhoneNumberResponse>.Success(new UpdatePhoneNumberResponse {PhoneNumber = user.PhoneNumber!});
    }
}