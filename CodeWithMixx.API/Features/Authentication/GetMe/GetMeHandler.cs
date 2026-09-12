using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Authentication.GetMe;

public class GetMeHandler(UserManager<User> userManager, IUserProvider userProvider) 
    : IHandler<GetMeRequest, Result<GetMeResponse>>
{
    public async Task<Result<GetMeResponse>> HandleAsync(GetMeRequest request, CancellationToken ct = default)
    {
        var userDetails = await userManager.Users
            .Where(u => u.Id == userProvider.GetUserId())
            .Select(u => new GetMeResponse 
            {
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email!,
                PhoneNumber = u.PhoneNumber!,
                Roles = userProvider.GetUserRoles()
            })
            .FirstOrDefaultAsync(ct);
        
        if(userDetails is null)
            return Result<GetMeResponse>.Failure(UserError.NotFound(userProvider.GetUserId()));
        
        return Result<GetMeResponse>.Success(userDetails);
    }
}