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
        var subjectResult = Subject.Create(request.SubjectName, request.SubjectDescription);
        if (!subjectResult.IsSuccess)
            return Result<CreateSubjectResponse>.Failure(subjectResult.Errors[0]);

        var newSubject = subjectResult.Payload!;

        var subjectExists = await context.Subjects.AnyAsync(s => EF.Functions.ILike(s.Name, newSubject.Name), ct);
        
        if(subjectExists)
            return Result<CreateSubjectResponse>.Failure(SubjectError.AlreadyExists(newSubject.Name));

        context.Add(newSubject);
        await context.SaveChangesAsync(ct);
        
        var response = new CreateSubjectResponse
        {
            Id = newSubject.Id,
            SubjectName = newSubject.Name,
            SubjectDescription = newSubject.Description,
            CreatedAt = newSubject.CreatedAt
        };
        return Result<CreateSubjectResponse>.Success(response);

    }
}