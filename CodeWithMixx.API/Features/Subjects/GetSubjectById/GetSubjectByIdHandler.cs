using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Subjects;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Subjects.GetSubjectById;

public class GetSubjectByIdHandler(AppDbContext context) : IHandler<GetSubjectByIdRequest, Result<GetSubjectByIdResponse>>
{
    public async Task<Result<GetSubjectByIdResponse>> HandleAsync(GetSubjectByIdRequest request, CancellationToken ct = default)
    {
        var subject = await context.Subjects 
            .Where(s => s.Id == request.Id)
            .Select(s => new GetSubjectByIdResponse
            {
                Id = s.Id,
                SubjectName = s.Name,
                Description = s.Description,
                CreatedAt = s.CreatedAt
            })
            .FirstOrDefaultAsync(ct);
        
        if(subject is null)
            return Result<GetSubjectByIdResponse>.Failure(SubjectError.NotFound(request.Id));
        
        return Result<GetSubjectByIdResponse>.Success(subject);
    }
}