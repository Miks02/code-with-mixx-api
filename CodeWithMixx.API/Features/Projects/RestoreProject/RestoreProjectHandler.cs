using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Projects;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Projects.RestoreProject;

public class RestoreProjectHandler(AppDbContext context) : IHandler<RestoreProjectRequest, Result>
{
    public async Task<Result> HandleAsync(RestoreProjectRequest request, CancellationToken ct = default)
    {
        var project = await context.Projects
            .IgnoreQueryFilters()
            .Include(p => p.Reservation)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, ct);

        if (project is null)
            return Result.Failure(ProjectError.NotFound(request.ProjectId));

        if (project.Reservation.IsDeleted)
            return Result.Failure(ProjectError.ReservationArchived(project.Id, project.ReservationId));

        var restoreResult = project.Restore();
        if (!restoreResult.IsSuccess)
            return restoreResult;

        await context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
