using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Features.Reservations.Common;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Reservations.GetPagedClassReservations;

public class GetPagedClassReservationsHandler(AppDbContext context)
    : IHandler<GetPagedClassReservationsRequest, Result<PagedResult<GetPagedClassReservationsResponse>>>
{
    public async Task<Result<PagedResult<GetPagedClassReservationsResponse>>> HandleAsync(
        GetPagedClassReservationsRequest request,
        CancellationToken ct = default)
    {
        var subjectIds = request.SubjectIds;

        var query = context.Reservations.AsQueryable();

        if (request.ReservationStatus is not null)
            query = query.Where(r => r.ReservationStatus == request.ReservationStatus);

        if (request.PaymentStatus is not null)
            query = query.Where(r => r.PaymentStatus == request.PaymentStatus);

        if (subjectIds.Length > 0)
            query = query.Where(r => r.Classes.Any(c => subjectIds.AsEnumerable().Contains(c.SubjectId)));

        query = request.SortBy switch
        {
            ClassReservationSortBy.CreatedAtDescending => query.OrderByDescending(r => r.CreatedAt),
            ClassReservationSortBy.CreatedAtAscending => query.OrderBy(r => r.CreatedAt),
            ClassReservationSortBy.StartDateDescending => query.OrderByDescending(r => r.Classes.Min(c => c.StartsAt)),
            ClassReservationSortBy.StartDateAscending => query.OrderBy(r => r.Classes.Min(c => c.StartsAt)),
            _ => query.OrderByDescending(r => r.Classes.Min(c => c.StartsAt))
        };

        var projectedQuery = query.Select(r => new GetPagedClassReservationsResponse
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
            StartsAt = r.Classes.Min(c => c.StartsAt),
            Classes = r.Classes
                .OrderBy(c => c.StartsAt)
                .Select(c => new ClassItem
                {
                    Id = c.Id,
                    SubjectId = c.SubjectId,
                    SubjectName = c.Subject.Name,
                    Price = c.Price,
                    StartsAt = c.StartsAt,
                    EndsAt = c.EndsAt
                })
                .ToList()
        });

        var pagedResult = await PagedResult<GetPagedClassReservationsResponse>.CreateAsync(projectedQuery, request.PageNumber, request.PageSize, ct);

        return Result<PagedResult<GetPagedClassReservationsResponse>>.Success(pagedResult);
    }
}
