using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Users;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Account.UpdateName;

public class UpdateNameHandler(
    AppDbContext context,
    IUserProvider userProvider)
    : IHandler<UpdateNameRequest, Result<UpdateNameResponse>>
{
    public async Task<Result<UpdateNameResponse>> HandleAsync(UpdateNameRequest request, CancellationToken ct = default)
    {
        var userId = userProvider.GetUserId();

        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null)
            return Result<UpdateNameResponse>.Failure(UserError.NotFound(userId));

        var changeNameResult = user.ChangeName(request.FirstName, request.LastName);
        if (!changeNameResult.IsSuccess)
            return Result<UpdateNameResponse>.Failure(changeNameResult.Errors[0]);

        await context.SaveChangesAsync(ct);

        var response = new UpdateNameResponse
        {
            FirstName = user.FirstName,
            LastName = user.LastName
        };

        return Result<UpdateNameResponse>.Success(response);
    }
}
