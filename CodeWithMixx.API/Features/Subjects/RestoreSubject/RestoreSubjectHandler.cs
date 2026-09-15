using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Subjects;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Subjects.RestoreSubject;

public class RestoreSubjectHandler(AppDbContext context) : IHandler<RestoreSubjectRequest, Result>
{
    public async Task<Result> HandleAsync(RestoreSubjectRequest request, CancellationToken ct = default)
    {
        var subject = await context.Subjects
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == request.Id, ct);

        if (subject is null)
            return Result.Failure(SubjectError.NotFound(request.Id));

        if (!subject.IsDeleted)
            return Result.Failure(SubjectError.NotArchived(request.Id));

        subject.Restore();

        await context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
