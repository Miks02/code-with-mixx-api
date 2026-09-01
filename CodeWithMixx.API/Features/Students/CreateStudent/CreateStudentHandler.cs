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
        AppDbContext context) : IHandler<CreateStudentRequest, Result<CreateStudentResponse>>
    {
        public async Task<Result<CreateStudentResponse>> HandleAsync(CreateStudentRequest request, CancellationToken ct = default)
        {
            await using var transaction = await context.Database.BeginTransactionAsync(ct);

            try
            {
                var newUser = User.CreateUser(request.FirstName, request.LastName, request.Email, request.PhoneNumber);

                var creationResult = await userManager.CreateAsync(newUser);

                if (!creationResult.Succeeded)
                    return creationResult.HandleIdentityResult(new CreateStudentResponse());

                var roleAssignResult = await userManager.AddToRoleAsync(newUser, "Student");

                if (!roleAssignResult.Succeeded)
                    return roleAssignResult.HandleIdentityResult(new CreateStudentResponse());

                var newStudent = Student.Create(newUser.Id, request.University);

                context.Add(newStudent);
                await context.SaveChangesAsync(ct);

                await transaction.CommitAsync(ct);

                var response = new CreateStudentResponse
                {
                    Id = newStudent.UserId,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    University = request.University
                };

                return Result<CreateStudentResponse>.Success(response);
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }
    }
}
