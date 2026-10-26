using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Projects;
using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Features.Reservations.Common;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Reservations.GetProjectReservationsSummary;

public class GetProjectReservationsSummaryHandler(AppDbContext context, IUserClock clock)
    : IHandler<GetProjectReservationsSummaryRequest, GetProjectReservationsSummaryResponse>
{
    public async Task<GetProjectReservationsSummaryResponse> HandleAsync(
        GetProjectReservationsSummaryRequest request,
        CancellationToken ct = default)
    {

        var reservationsQuery = context.Reservations
            .Where(r => r.ReservationType == ReservationType.Project);
        
        if(request.ShowOnlyDeletedReservations)
            reservationsQuery = reservationsQuery
                .IgnoreQueryFilters()
                .Where(r => r.IsDeleted);
        
        if(request.ShowOnlyDeletedStudents)
            reservationsQuery = reservationsQuery
                .IgnoreQueryFilters()
                .Where(r => r.Student.IsDeleted);
        
        if(request.SubjectIds.Length > 0)
            reservationsQuery = reservationsQuery
                .Where(r => r.Projects.Any(p => ((IEnumerable<int>)request.SubjectIds).Contains(p.SubjectId)));
        
        if(request.PaymentStatuses.Length > 0)
            reservationsQuery = reservationsQuery
                .Where(r => Enumerable.Contains(request.PaymentStatuses, r.PaymentStatus));

        var invalidStatusFilters = ReservationStatusFilterResolver.GetInvalidFilters(request.ReservationStatuses);

        var reservationStatuses = request.ReservationStatuses
            .Except(invalidStatusFilters)
            .Select(f => f switch
            {
                ReservationStatusFilter.Confirmed => ReservationStatus.Confirmed,
                ReservationStatusFilter.Completed => ReservationStatus.Completed,
                ReservationStatusFilter.Cancelled => ReservationStatus.Cancelled,
                _ => (ReservationStatus?)null
            })
            .Where(s => s.HasValue)
            .Select(s => s!.Value)
            .ToList();

        if (reservationStatuses.Count > 0)
            reservationsQuery = reservationsQuery.Where(r => reservationStatuses.Contains(r.ReservationStatus));

        var projectTypes = request.ProjectTypes
            .Select(f => f switch
            {
                ProjectTypeFilter.Seminar => ProjectType.Seminar,
                ProjectTypeFilter.Graduation => ProjectType.Graduation,
                _ => (ProjectType?)null
            })
            .Where(t => t.HasValue)
            .Select(t => t!.Value)
            .Distinct()
            .ToList();

        if (projectTypes.Count > 0)
            reservationsQuery = reservationsQuery.Where(r => r.Projects.Any(p => projectTypes.Contains(p.ProjectType)));

        var projectedQuery = reservationsQuery
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new GetProjectReservationsSummaryResponse.ProjectReservation
            {
                Id = r.Id,
                StudentId = r.StudentId,
                StudentFullName = r.Student.User.FirstName + " " + r.Student.User.LastName,
                ReservationStatus = r.ReservationStatus,
                PaymentStatus = r.PaymentStatus,
                TotalPrice = r.TotalPrice,
                PaidAmount = r.PaidAmount,
                DiscountRate = r.DiscountRate,
                Bonus = r.Bonus,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                DeletedAt = r.DeletedAt,
                IsHardDeletable = !r.RequiresHistoryRetention(clock.Today),
                Projects = r.Projects
                    .OrderBy(p => p.StartDate)
                    .Select(p => new ProjectItem
                    {
                        Id = p.Id,
                        SubjectId = p.SubjectId,
                        SubjectName = p.Subject.Name,
                        ProjectType = p.ProjectType,
                        Price = p.Price,
                        Progress = p.Progress,
                        ReservedAt = p.ReservedAt,
                        StartDate = p.StartDate,
                        EndDate = p.EndDate,
                        GithubLink = p.GithubLink,
                        DownloadLink = p.DownloadLink,
                        NotesCount = p.ProjectNotes.Count
                    })
                    .ToList()
            });

        var pagedResult = await PagedResult<GetProjectReservationsSummaryResponse.ProjectReservation>.CreateAsync(
            projectedQuery, request.PageNumber, request.PageSize, ct);

        var today = clock.Today;

        var stats = await context.Projects
            .GroupBy(p => 1)
            .Select(g => new
            {
                TotalProjects = g.Count(),
                ActiveProjects = g.Count(p => p.Reservation.ReservationStatus == ReservationStatus.Confirmed && p.StartDate <= today),
                CompletedProjects = g.Count(p => p.Reservation.ReservationStatus == ReservationStatus.Completed),
                PendingProjects = g.Count(p => p.Reservation.ReservationStatus == ReservationStatus.Confirmed && p.StartDate > today),
            })
            .FirstOrDefaultAsync(ct) ?? new { TotalProjects = 0, ActiveProjects = 0, CompletedProjects = 0, PendingProjects = 0 };

        return new GetProjectReservationsSummaryResponse
        {
            Reservations = pagedResult,
            TotalProjects = stats.TotalProjects,
            ActiveProjects = stats.ActiveProjects,
            CompletedProjects = stats.CompletedProjects,
            PendingProjects = stats.PendingProjects
        };
    }
}
