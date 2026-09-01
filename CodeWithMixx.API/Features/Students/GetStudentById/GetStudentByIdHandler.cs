using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Students;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Students.GetStudentById;

public class GetStudentByIdHandler(AppDbContext context)
    : IHandler<GetStudentByIdRequest, Result<GetStudentByIdResponse>>
{
    public async Task<Result<GetStudentByIdResponse>> HandleAsync(GetStudentByIdRequest request, CancellationToken ct = default)
    {
        var student = await context.Students
            .Where(s => s.UserId == request.Id)
            .Select(s => new GetStudentByIdResponse
            {
                Id = s.UserId,
                FirstName = s.User.FirstName,
                LastName = s.User.LastName,
                Email = s.User.Email!,
                PhoneNumber = s.User.PhoneNumber,
                University = s.University,
                CreatedAt = s.User.CreatedAt,
                LastLogin = s.User.LastLoginAt
            })
            .FirstOrDefaultAsync(ct);

        if (student is null)
            return Result<GetStudentByIdResponse>.Failure(StudentError.NotFound(request.Id));

        return Result<GetStudentByIdResponse>.Success(student);
    }
}
