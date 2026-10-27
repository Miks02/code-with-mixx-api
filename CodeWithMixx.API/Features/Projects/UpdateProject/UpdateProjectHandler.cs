using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Projects;
using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Projects.UpdateProject;

public class UpdateProjectHandler(AppDbContext context, IUserClock clock) : IHandler<UpdateProjectRequest, Result>
{
    public async Task<Result> HandleAsync(UpdateProjectRequest request, CancellationToken ct = default)
    {
        var reservation = await context.Reservations
            .IgnoreQueryFilters()
            .Include(r => r.Projects.Where(p => !p.IsDeleted || p.Id == request.ProjectId))
                .ThenInclude(p => p.ProjectNotes)
            .FirstOrDefaultAsync(r => r.Projects.Any(p => p.Id == request.ProjectId), ct);

        if (reservation is null)
            return Result.Failure(ProjectError.NotFound(request.ProjectId));

        if (reservation.IsDeleted)
            return Result.Failure(ReservationError.CantUpdateAnArchivedReservation(reservation.Id));

        var project = reservation.Projects.First(p => p.Id == request.ProjectId);

        if (project.IsDeleted)
            return Result.Failure(ProjectError.CantUpdateAnArchivedProject(project.Id));

        var updateData = new ProjectUpdateData
        {
            Id = project.Id,
            SubjectId = project.SubjectId,
            ProjectType = project.ProjectType,
            StartDate = project.StartDate,
            EndDate = project.EndDate,
            ReservedAt = project.ReservedAt,
            Today = clock.Today,
            GithubLink = request.GithubLink,
            DownloadLink = request.DownloadLink,
            Price = request.Price,
            Progress = request.Progress,
            NotesToDelete = request.NotesToDelete,
            Notes = request.NotesToAdd
                .Select(content => new ProjectUpdateData.ProjectNoteData { Content = content })
                .ToList()
        };

        var updateResult = reservation.UpdateProjects([updateData], clock.Today);

        if (!updateResult.IsSuccess)
            return updateResult;

        await context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
