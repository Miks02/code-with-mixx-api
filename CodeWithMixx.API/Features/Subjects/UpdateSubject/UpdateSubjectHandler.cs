using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Subjects;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Subjects.UpdateSubject;

public class UpdateSubjectHandler(AppDbContext context) : IHandler<UpdateSubjectRequest, Result<UpdateSubjectResponse>>
{
    public async Task<Result<UpdateSubjectResponse>> HandleAsync(UpdateSubjectRequest request, CancellationToken ct = default)
    {
        var subject = await context.Subjects
            .FirstOrDefaultAsync(s => s.Id == request.Id, ct);

        if (subject is null)
            return Result<UpdateSubjectResponse>.Failure(SubjectError.NotFound(request.Id));

        if(!subject.Name.Equals(request.SubjectName, StringComparison.OrdinalIgnoreCase))
        {
            var subjectExists = await context.Subjects
                .AnyAsync(s => EF.Functions.ILike(s.Name, request.SubjectName), ct);

            if (subjectExists)
                return Result<UpdateSubjectResponse>.Failure(SubjectError.AlreadyExists(request.SubjectName));
        }

        subject.Update(request.SubjectName, request.SubjectDescription);

        await context.SaveChangesAsync(ct);

        var counts = await context.Subjects
            .Where(s => s.Id == subject.Id)
            .Select(s => new
            {
                ClassesCount = s.Classes.Count,
                StudentsCount = s.Classes
                    .Select(c => c.Reservation.Student)
                    .Distinct()
                    .Count()
            })
            .FirstAsync(ct);

        var response = new UpdateSubjectResponse
        {
            Id = subject.Id,
            SubjectName = subject.Name,
            SubjectDescription = subject.Description!,
            CreatedAt = subject.CreatedAt,
            ClassesCount = counts.ClassesCount,
            StudentsCount = counts.StudentsCount
        };

        return Result<UpdateSubjectResponse>.Success(response);
    }
}
