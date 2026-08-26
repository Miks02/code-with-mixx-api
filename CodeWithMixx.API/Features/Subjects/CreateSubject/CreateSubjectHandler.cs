using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Subjects;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Subjects.CreateSubject;

public class CreateSubjectHandler(AppDbContext context) : IHandler<CreateSubjectRequest, Result<CreateSubjectResponse>>
{
    public async Task<Result<CreateSubjectResponse>> HandleAsync(CreateSubjectRequest request, CancellationToken ct = default)
    {
        var subjectExists = await context.Subjects.AnyAsync(s => s.Name == request.SubjectName, ct);
        
        if(subjectExists)
            return Result<CreateSubjectResponse>.Failure(SubjectError.AlreadyExists(request.SubjectName));
        
        var newSubject = Subject.Create(request.SubjectName, request.Description);

        context.Add(newSubject);
        await context.SaveChangesAsync(ct);
        
        var response = new CreateSubjectResponse
        {
            Id = newSubject.Id,
            SubjectName = newSubject.Name,
            Description = newSubject.Description,
            CreatedAt = newSubject.CreatedAt
        };
        return Result<CreateSubjectResponse>.Success(response);

    }
}