using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Classes;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Classes.ArchiveClass;

public class ArchiveClassHandler(AppDbContext context) : IHandler<ArchiveClassRequest, Result>
{
    public async Task<Result> HandleAsync(ArchiveClassRequest request, CancellationToken ct = default)
    {
        var @class = await context.Classes
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == request.ClassId, ct);

        if (@class is null)
            return Result.Failure(ClassError.NotFound(request.ClassId));

        var archiveResult = @class.Delete();
        if (!archiveResult.IsSuccess)
            return archiveResult;

        await context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
