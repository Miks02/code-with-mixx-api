using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Projects;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Projects.GetProjectDetails;

public class GetProjectDetailsHandler(AppDbContext context)
    : IHandler<GetProjectDetailsRequest, Result<GetProjectDetailsResponse>>
{
    public async Task<Result<GetProjectDetailsResponse>> HandleAsync(
        GetProjectDetailsRequest request,
        CancellationToken ct = default)
    {
        var project = await context.Projects
            .IgnoreQueryFilters()
            .Where(p => p.Id == request.ProjectId)
            .Select(p => new GetProjectDetailsResponse
            {
                Id = p.Id,
                ReservationId = p.ReservationId,
                SubjectId = p.SubjectId,
                SubjectName = p.Subject.Name,
                ProjectType = p.ProjectType,
                Price = p.Price,
                Progress = p.Progress,
                GithubLink = p.GithubLink,
                DownloadLink = p.DownloadLink,
                ReservedAt = p.ReservedAt,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                IsDeleted = p.IsDeleted,
                IsReservationArchived = p.Reservation.IsDeleted,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                Notes = p.ProjectNotes
                    .OrderBy(n => n.CreatedAt)
                    .Select(n => new GetProjectDetailsResponse.ProjectNoteItem
                    {
                        Id = n.Id,
                        Content = n.Content,
                        CreatedAt = n.CreatedAt,
                        UpdatedAt = n.UpdatedAt
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(ct);

        if (project is null)
            return Result<GetProjectDetailsResponse>.Failure(ProjectError.NotFound(request.ProjectId));

        return Result<GetProjectDetailsResponse>.Success(project);
    }
}
