using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Features.Classes.Common;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Classes.GetClassReservationsSummary;

public class GetClassReservationsSummaryHandler(AppDbContext context)
    : IHandler<GetClassReservationsSummaryRequest, Result<GetClassReservationsSummaryResponse>>
{

    public async Task<Result<GetClassReservationsSummaryResponse>> HandleAsync(
        GetClassReservationsSummaryRequest request,
        CancellationToken ct = default)
    {
        var subjectIds = request.SubjectIds;

        var reservationsQuery = context.Reservations.AsQueryable();

        if (request.ReservationStatus is not null)
            reservationsQuery = reservationsQuery.Where(r => r.ReservationStatus == request.ReservationStatus);

        if (request.PaymentStatus is not null)
            reservationsQuery = reservationsQuery.Where(r => r.PaymentStatus == request.PaymentStatus);

        if (subjectIds.Length > 0)
            reservationsQuery = reservationsQuery.Where(r => r.Classes.Any(c => subjectIds.AsEnumerable().Contains(c.SubjectId)));

        reservationsQuery = request.SortBy switch
        {
            ClassReservationSortBy.CreatedAtDescending => reservationsQuery.OrderByDescending(r => r.CreatedAt),
            ClassReservationSortBy.CreatedAtAscending => reservationsQuery.OrderBy(r => r.CreatedAt),
            ClassReservationSortBy.StartDateDescending => reservationsQuery.OrderByDescending(r => r.Classes.Min(c => c.StartsAt)),
            ClassReservationSortBy.StartDateAscending => reservationsQuery.OrderBy(r => r.Classes.Min(c => c.StartsAt)),
            _ => reservationsQuery.OrderBy(r => r.Classes.Min(c => c.StartsAt))
        };

        var projectedQuery = reservationsQuery.Select(r => new GetClassReservationsSummaryResponse.ClassReservationDto
        {
            Id = r.Id,
            StudentId = r.StudentId,
            StudentFullName = r.Student.User.FirstName + " " + r.Student.User.LastName,
            ReservationStatus = r.ReservationStatus,
            PaymentStatus = r.PaymentStatus,
            TotalPrice = r.TotalPrice,
            PaidAmount = r.PaidAmount,
            CreatedAt = r.CreatedAt,
            StartsAt = r.Classes.Min(c => c.StartsAt),
            Classes = r.Classes
                .OrderBy(c => c.StartsAt)
                .Select(c => new GetClassReservationsSummaryResponse.ClassReservationDto.ClassDto
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

        var pagedResult = await PagedResult<GetClassReservationsSummaryResponse.ClassReservationDto>.CreateAsync(
            projectedQuery, request.PageNumber, request.PageSize, ct);
        
        var statusCounts = await context.Classes
            .GroupBy(c => c.Reservation.ReservationStatus)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var heldClassesCount = statusCounts.FirstOrDefault(s => s.Status == ReservationStatus.Completed)?.Count ?? 0;
        var scheduledClassesCount = statusCounts.FirstOrDefault(s => s.Status == ReservationStatus.Confirmed)?.Count ?? 0;
        var cancelledClassesCount = statusCounts.FirstOrDefault(s => s.Status == ReservationStatus.Cancelled)?.Count ?? 0;
        var totalClassesCount = statusCounts.Sum(s => s.Count);

        var nextClassQuery = context.Classes
            .Where(c => c.Reservation.ReservationStatus == ReservationStatus.Confirmed && c.StartsAt >= DateTime.UtcNow);

        var nextClass = await nextClassQuery
            .OrderBy(c => c.StartsAt)
            .Select(c => new GetClassReservationsSummaryResponse.NextClassDto
            {
                ReservationId = c.ReservationId,
                StudentFullName = c.Reservation.Student.User.FirstName + " " + c.Reservation.Student.User.LastName,
                StartsAt = c.StartsAt,
                University = c.Reservation.Student.University,
                SubjectName = c.Subject.Name
            })
            .FirstOrDefaultAsync(ct);

        var response = new GetClassReservationsSummaryResponse
        {
            Reservations = pagedResult,
            TotalClassesCount = totalClassesCount,
            HeldClassesCount = heldClassesCount,
            ScheduledClassesCount = scheduledClassesCount,
            CancelledClassesCount = cancelledClassesCount,
            NextClass = nextClass
        };

        return Result<GetClassReservationsSummaryResponse>.Success(response);
    }
}
