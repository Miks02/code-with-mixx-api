using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Features.Reservations.Common;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Reservations.Student.GetClassReservationsSummaryForStudent;

public class GetClassReservationsSummaryForStudentHandler(AppDbContext context, IUserProvider userProvider)
    : IHandler<GetClassReservationsSummaryForStudentRequest, Result<GetClassReservationsSummaryForStudentResponse>>
{
    public async Task<Result<GetClassReservationsSummaryForStudentResponse>> HandleAsync(
        GetClassReservationsSummaryForStudentRequest request,
        CancellationToken ct = default)
    {
        var studentId = userProvider.GetUserId();
        var subjectIds = request.SubjectIds;

        var reservationsQuery = context.Reservations
            .Where(r => r.StudentId == studentId && r.ReservationType == ReservationType.Class);

        if (subjectIds.Length > 0)
            reservationsQuery = reservationsQuery.Where(r => r.Classes.Any(c => subjectIds.AsEnumerable().Contains(c.SubjectId)));

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

        reservationsQuery = request.SortBy switch
        {
            ClassReservationSortBy.CreatedAtDescending => reservationsQuery.OrderByDescending(r => r.CreatedAt),
            ClassReservationSortBy.CreatedAtAscending => reservationsQuery.OrderBy(r => r.CreatedAt),
            ClassReservationSortBy.StartDateDescending => reservationsQuery.OrderByDescending(r => r.Classes.Min(c => c.StartsAt)),
            ClassReservationSortBy.StartDateAscending => reservationsQuery.OrderBy(r => r.Classes.Min(c => c.StartsAt)),
            _ => reservationsQuery.OrderBy(r => r.Classes.Min(c => c.StartsAt))
        };

        var projectedQuery = reservationsQuery.Select(r => new GetClassReservationsSummaryForStudentResponse.ClassReservation
        {
            Id = r.Id,
            ReservationStatus = r.ReservationStatus,
            PaymentStatus = r.PaymentStatus,
            TotalPrice = r.TotalPrice,
            PaidAmount = r.PaidAmount,
            CreatedAt = r.CreatedAt,
            DiscountRate = r.DiscountRate,
            Description = r.Description,
            Bonus = r.Bonus,
            StartsAt = r.Classes.Min(c => c.StartsAt),
            EndsAt = r.Classes.Max(c => c.EndsAt),
            Classes = r.Classes
                .OrderBy(c => c.StartsAt)
                .Select(c => new ClassItem
                {
                    Id = c.Id,
                    Name = c.Name,
                    SubjectId = c.SubjectId,
                    SubjectName = c.Subject.Name,
                    Price = c.Price,
                    StartsAt = c.StartsAt,
                    EndsAt = c.EndsAt,
                    ReservedAt = c.ReservedAt
                })
                .ToList()
        });

        var pagedResult = await PagedResult<GetClassReservationsSummaryForStudentResponse.ClassReservation>.CreateAsync(
            projectedQuery, request.PageNumber, request.PageSize, ct);

        var studentClasses = context.Classes
            .Where(c => c.Reservation.StudentId == studentId);

        var statusCounts = await studentClasses
            .GroupBy(c => c.Reservation.ReservationStatus)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var heldClassesCount = statusCounts.FirstOrDefault(s => s.Status == ReservationStatus.Completed)?.Count ?? 0;
        var scheduledClassesCount = statusCounts.FirstOrDefault(s => s.Status == ReservationStatus.Confirmed)?.Count ?? 0;
        var cancelledClassesCount = statusCounts.FirstOrDefault(s => s.Status == ReservationStatus.Cancelled)?.Count ?? 0;
        var totalClassesCount = statusCounts.Sum(s => s.Count);

        var nextClass = await studentClasses
            .Where(c => c.Reservation.ReservationStatus == ReservationStatus.Confirmed && c.StartsAt >= DateTime.UtcNow)
            .OrderBy(c => c.StartsAt)
            .Select(c => new GetClassReservationsSummaryForStudentResponse.NextClass
            {
                ReservationId = c.ReservationId,
                ClassName = c.Name,
                StartsAt = c.StartsAt,
                SubjectName = c.Subject.Name
            })
            .FirstOrDefaultAsync(ct);

        return Result<GetClassReservationsSummaryForStudentResponse>.Success(new GetClassReservationsSummaryForStudentResponse
        {
            Reservations = pagedResult,
            TotalClassesCount = totalClassesCount,
            HeldClassesCount = heldClassesCount,
            ScheduledClassesCount = scheduledClassesCount,
            CancelledClassesCount = cancelledClassesCount,
            NextClassItem = nextClass
        });
    }
}
