using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Students;
using CodeWithMixx.API.Domain.Entities.Users;
using CodeWithMixx.API.Features.Authentication.Common;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Students.DeactivateStudent
{
    public class DeactivateStudentHandler(
        UserManager<User> userManager,
        AppDbContext context,
        ITokenService tokenService)
        : IHandler<DeactivateStudentRequest, Result>
    {
        public async Task<Result> HandleAsync(DeactivateStudentRequest request, CancellationToken ct = default)
        {
            var student = await context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == request.Id, ct);

            if (student is null)
                return Result.Failure(StudentError.NotFound(request.Id));

            var deactivateResult = student.User.DeactivateAccount();
            if (!deactivateResult.IsSuccess)
                return deactivateResult;

            await tokenService.RevokeAllUserTokensAsync(student.UserId);

            await userManager.UpdateAsync(student.User);

            return Result.Success();
        }
    }
}
