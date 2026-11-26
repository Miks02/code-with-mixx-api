using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Projects;
using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Features.Reservations.Common;
using CodeWithMixx.API.Infrastructure.Persistence;

namespace CodeWithMixx.API.Features.Reservations.Student.GetPagedProjectReservationsForStudent;

public class GetPagedProjectReservationsForStudentHandler(AppDbContext context, IUserProvider userProvider)
    : IHandler<GetPagedProjectReservationsForStudentRequest, PagedResult<GetPagedProjectReservationsForStudentResponse>>
{
    public async Task<PagedResult<GetPagedProjectReservationsForStudentResponse>> HandleAsync(
        GetPagedProjectReservationsForStudentRequest request,
        CancellationToken ct = default)
    {
        var studentId = userProvider.GetUserId();

        var reservationsQuery = context.Reservations
            .Where(r => r.StudentId == studentId && r.ReservationType == ReservationType.Project);

        if (request.SubjectIds.Length > 0)
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

        if (request.PaymentStatuses.Length > 0)
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
            .Select(r => new GetPagedProjectReservationsForStudentResponse
            {
                Id = r.Id,
                ReservationStatus = r.ReservationStatus,
                PaymentStatus = r.PaymentStatus,
                TotalPrice = r.TotalPrice,
                PaidAmount = r.PaidAmount,
                DiscountRate = r.DiscountRate,
                Bonus = r.Bonus,
                Description = r.Description,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                ReservedAt = r.ReservedAt,
                Projects = r.Projects
                    .OrderBy(p => p.StartDate)
                    .Select(p => new ProjectItem
                    {
                        Id = p.Id,
                        Name = p.Name,
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

        return await PagedResult<GetPagedProjectReservationsForStudentResponse>.CreateAsync(
            projectedQuery, request.PageNumber, request.PageSize, ct);
    }
}
