using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Features.Reservations.Common;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Reservations.GetClassReservationDetails;

public class GetClassReservationDetailsHandler(AppDbContext context)
    : IHandler<GetClassReservationDetailsRequest, Result<GetClassReservationDetailsResponse>>
{
    public async Task<Result<GetClassReservationDetailsResponse>> HandleAsync(
        GetClassReservationDetailsRequest request,
        CancellationToken ct = default)
    {
        var reservation = await context.Reservations
            .Where(r => r.Id == request.ReservationId && r.ReservationType == ReservationType.Class)
            .Select(r => new GetClassReservationDetailsResponse
            {
                Id = r.Id,
                StudentId = r.StudentId,
                StudentFullName = r.Student.User.FirstName + " " + r.Student.User.LastName,
                StudentEmail = r.Student.User.Email!,
                StudentUniversity = r.Student.University,
                ReservationStatus = r.ReservationStatus,
                PaymentStatus = r.PaymentStatus,
                TotalPrice = r.TotalPrice,
                PaidAmount = r.PaidAmount,
                DiscountRate = r.DiscountRate,
                Bonus = r.Bonus,
                Description = r.Description,
                ReservedAt = r.ReservedAt,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                Classes = r.Classes
                    .OrderBy(c => c.StartsAt)
                    .Select(c => new ClassItem
                    {
                        Id = c.Id,
                        SubjectId = c.SubjectId,
                        SubjectName = c.Subject.Name,
                        Price = c.Price,
                        StartsAt = c.StartsAt,
                        EndsAt = c.EndsAt,
                        ReservedAt = c.ReservedAt
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(ct);

        if (reservation is null)
            return Result<GetClassReservationDetailsResponse>.Failure(ReservationError.NotFound(request.ReservationId));

        return Result<GetClassReservationDetailsResponse>.Success(reservation);
    }
}
