using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Projects;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Projects.ArchiveProject;

public class ArchiveProjectHandler(AppDbContext context) : IHandler<ArchiveProjectRequest, Result>
{
    public async Task<Result> HandleAsync(ArchiveProjectRequest request, CancellationToken ct = default)
    {
        var project = await context.Projects
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, ct);

        if (project is null)
            return Result.Failure(ProjectError.NotFound(request.ProjectId));

        var archiveResult = project.Delete();
        if (!archiveResult.IsSuccess)
            return archiveResult;

        await context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
