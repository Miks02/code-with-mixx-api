using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Features.Reservations.Common;
using CodeWithMixx.API.Infrastructure.Persistence;

namespace CodeWithMixx.API.Features.Reservations.Student.GetPagedClassReservationsForStudent;

public class GetPagedClassReservationsForStudentHandler(AppDbContext context, IUserProvider userProvider)
    : IHandler<GetPagedClassReservationsForStudentRequest, Result<PagedResult<GetPagedClassReservationsForStudentResponse>>>
{
    public async Task<Result<PagedResult<GetPagedClassReservationsForStudentResponse>>> HandleAsync(
        GetPagedClassReservationsForStudentRequest request,
        CancellationToken ct = default)
    {
        var studentId = userProvider.GetUserId();
        var subjectIds = request.SubjectIds;

        var query = context.Reservations
            .Where(r => r.StudentId == studentId && r.ReservationType == ReservationType.Class);

        if (subjectIds.Length > 0)
            query = query.Where(r => r.Classes.Any(c => subjectIds.AsEnumerable().Contains(c.SubjectId)));

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
            query = query.Where(r => reservationStatuses.Contains(r.ReservationStatus));

        if (request.PaymentStatuses.Length > 0)
            query = query.Where(r => Enumerable.Contains(request.PaymentStatuses, r.PaymentStatus));

        query = request.SortBy switch
        {
            ClassReservationSortBy.CreatedAtDescending => query.OrderByDescending(r => r.CreatedAt),
            ClassReservationSortBy.CreatedAtAscending => query.OrderBy(r => r.CreatedAt),
            ClassReservationSortBy.StartDateDescending => query.OrderByDescending(r => r.Classes.Min(c => c.StartsAt)),
            ClassReservationSortBy.StartDateAscending => query.OrderBy(r => r.Classes.Min(c => c.StartsAt)),
            _ => query.OrderByDescending(r => r.Classes.Min(c => c.StartsAt))
        };

        var projectedQuery = query.Select(r => new GetPagedClassReservationsForStudentResponse
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

        var pagedResult = await PagedResult<GetPagedClassReservationsForStudentResponse>.CreateAsync(projectedQuery, request.PageNumber, request.PageSize, ct);

        return Result<PagedResult<GetPagedClassReservationsForStudentResponse>>.Success(pagedResult);
    }
}
