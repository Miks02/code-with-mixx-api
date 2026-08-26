using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Subjects;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Subjects.DeleteSubject;

public class DeleteSubjectHandler(AppDbContext context) : IHandler<DeleteSubjectRequest, Result>
{
    public async Task<Result> HandleAsync(DeleteSubjectRequest request, CancellationToken ct = default)
    {
        var subject = await context.Subjects
            .FirstOrDefaultAsync(s => s.Id == request.Id, ct);

        if (subject is null)
            return Result.Failure(SubjectError.NotFound(request.Id));

        subject.Delete();
        await context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
