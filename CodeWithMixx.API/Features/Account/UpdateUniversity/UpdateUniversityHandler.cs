using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Students;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Account.UpdateUniversity;

public class UpdateUniversityHandler(
    AppDbContext context,
    IUserProvider userProvider)
    : IHandler<UpdateUniversityRequest, Result<UpdateUniversityResponse>>
{
    public async Task<Result<UpdateUniversityResponse>> HandleAsync(UpdateUniversityRequest request, CancellationToken ct = default)
    {
        var userId = userProvider.GetUserId();

        var student = await context.Students.FirstOrDefaultAsync(s => s.UserId == userId, ct);
        if (student is null)
            return Result<UpdateUniversityResponse>.Failure(StudentError.NotFound(userId));

        var changeUniversityResult = student.ChangeUniversity(request.University);
        if (!changeUniversityResult.IsSuccess)
            return Result<UpdateUniversityResponse>.Failure(changeUniversityResult.Errors[0]);

        await context.SaveChangesAsync(ct);

        var response = new UpdateUniversityResponse
        {
            University = student.University
        };

        return Result<UpdateUniversityResponse>.Success(response);
    }
}
