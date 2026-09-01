using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Students;
using CodeWithMixx.API.Domain.Entities.Users;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;

namespace CodeWithMixx.API.Features.Students.CreateStudent
{
    public class CreateStudentHandler(
        UserManager<User> userManager,
        AppDbContext context) : IHandler<CreateStudentRequest, Result>
    {
        public async Task<Result> HandleAsync(CreateStudentRequest request, CancellationToken ct = default)
        {
            await using var transaction = await context.Database.BeginTransactionAsync(ct);

            try
            {
                var newUser = User.CreateUser(request.FirstName, request.LastName, request.Email, request.PhoneNumber);

                var creationResult = await userManager.CreateAsync(newUser);

                if (!creationResult.Succeeded)
                    return creationResult.HandleIdentityResult();

                var roleAssignResult = await userManager.AddToRoleAsync(newUser, "Student");

                if (!roleAssignResult.Succeeded)
                    return roleAssignResult.HandleIdentityResult();

                var newStudent = Student.Create(newUser.Id, request.University);

                context.Add(newStudent);
                await context.SaveChangesAsync(ct);

                await transaction.CommitAsync(ct);

                return Result.Success();
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }
    }
}
