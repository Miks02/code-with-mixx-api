using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Subjects;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Subjects.ArchiveSubject;

public class ArchiveSubjectHandler(AppDbContext context) : IHandler<ArchiveSubjectRequest, Result>
{
    public async Task<Result> HandleAsync(ArchiveSubjectRequest request, CancellationToken ct = default)
    {
        var subject = await context.Subjects
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == request.Id, ct);

        if (subject is null)
            return Result.Failure(SubjectError.NotFound(request.Id));

        if (subject.IsDeleted)
            return Result.Failure(SubjectError.AlreadyArchived(request.Id));

        subject.Archive();

        await context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
