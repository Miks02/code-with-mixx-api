using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Students;
using CodeWithMixx.API.Domain.Entities.Users;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Students.UpdateStudent;

public class UpdateStudentHandler(
    AppDbContext context, 
    UserManager<User> userManager) 
    : IHandler<UpdateStudentRequest, Result<UpdateStudentResponse>>
{
    public async Task<Result<UpdateStudentResponse>> HandleAsync(UpdateStudentRequest request, CancellationToken ct = default)
    {
        var student = await context.Students
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.UserId == request.Id, ct);
        
        if(student is null) 
            return Result<UpdateStudentResponse>.Failure(StudentError.NotFound(request.Id));
        
        await using var transaction = await context.Database.BeginTransactionAsync(ct);
        
        student.User.ChangeName(request.FirstName, request.LastName);
        student.ChangeUniversity(request.University);
        
        if (!string.Equals(student.User.Email, student.User.NormalizedEmail, StringComparison.OrdinalIgnoreCase))
        {
            var emailChange = await userManager.SetEmailAsync(student.User, request.Email); 
            if(!emailChange.Succeeded)
                return Result<UpdateStudentResponse>.Failure(emailChange.Errors.First());
            
            var usernameChange = await userManager.SetUserNameAsync(student.User, request.Email);
            if(!usernameChange.Succeeded)
                return Result<UpdateStudentResponse>.Failure(usernameChange.Errors.First());
        }
        
        if(!string.Equals(student.User.PhoneNumber, request.PhoneNumber, StringComparison.OrdinalIgnoreCase))
        {
            var phoneChange = await userManager.SetPhoneNumberAsync(student.User, request.PhoneNumber);
            if(!phoneChange.Succeeded)
                return Result<UpdateStudentResponse>.Failure(phoneChange.Errors.First());
        }

        await context.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        var response = new UpdateStudentResponse
        {
            Id = student.UserId,
            FirstName = student.User.FirstName,
            LastName = student.User.LastName,
            Email = student.User.Email!,
            PhoneNumber = student.User.PhoneNumber!,
            University = student.University,
            AccountStatus = student.User.AccountStatus,
            RegisteredAt = student.User.CreatedAt
        };
        
        return Result<UpdateStudentResponse>.Success(response);
    }
}