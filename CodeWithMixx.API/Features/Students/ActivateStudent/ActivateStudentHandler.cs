using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Students;
using CodeWithMixx.API.Domain.Entities.Users;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Students.ActivateStudent
{
    public class ActivateStudentHandler(
        UserManager<User> userManager,
        AppDbContext context)
        : IHandler<ActivateStudentRequest, Result>
    {
        public async Task<Result> HandleAsync(ActivateStudentRequest request, CancellationToken ct = default)
        {
            var student = await context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == request.Id, ct);

            if (student is null)
                return Result.Failure(StudentError.NotFound(request.Id));

            var activateResult = student.User.ActivateAccount();
            if (!activateResult.IsSuccess)
                return activateResult;

            await userManager.UpdateAsync(student.User);

            return Result.Success();
        }
    }
}
