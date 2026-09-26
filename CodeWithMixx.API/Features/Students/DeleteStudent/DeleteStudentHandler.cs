using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Students;
using CodeWithMixx.API.Domain.Entities.Users;
using CodeWithMixx.API.Features.Authentication.Common;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Students.DeleteStudent
{
    public class DeleteStudentHandler(
        UserManager<User> userManager,
        AppDbContext context,
        ITokenService tokenService) 
        : IHandler<DeleteStudentRequest, Result>
    {
        public async Task<Result> HandleAsync(DeleteStudentRequest request, CancellationToken ct = default)
        {
            var studentToDelete = await context.Users
                .Include(u => u.Student)
                    .ThenInclude(s => s!.Reservations)
                .FirstOrDefaultAsync(u => u.Id == request.Id, ct);

            if (studentToDelete?.Student is null)
                return Result.Failure(StudentError.NotFound(request.Id));

            if (studentToDelete.Student.Reservations.Any())
            {
                await tokenService.RevokeAllUserTokensAsync(studentToDelete.Id);
                studentToDelete.DeleteUser();
                await userManager.UpdateAsync(studentToDelete);
            }
            else 
            {
                context.Users.Remove(studentToDelete);
                await context.SaveChangesAsync(ct);
            }

            return Result.Success();
        }
    }
}
