using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Projects;
using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Features.Reservations.Common;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Reservations.GetPagedProjectReservations;

public class GetPagedProjectReservationsHandler(AppDbContext context)
    : IHandler<GetPagedProjectReservationsRequest, PagedResult<GetPagedProjectReservationsResponse>>
{
    public async Task<PagedResult<GetPagedProjectReservationsResponse>> HandleAsync(
        GetPagedProjectReservationsRequest request,
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
        
        if(request.PaymentStatuses.Length > 0) 
           reservationsQuery = reservationsQuery.Where(r => Enumerable.Contains(request.PaymentStatuses, r.PaymentStatus));

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
            .Select(r => new GetPagedProjectReservationsResponse
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
                        DownloadLink = p.DownloadLink
                    })
                    .ToList()
            });

        return await PagedResult<GetPagedProjectReservationsResponse>.CreateAsync(
            projectedQuery, request.PageNumber, request.PageSize, ct);
    }
}
