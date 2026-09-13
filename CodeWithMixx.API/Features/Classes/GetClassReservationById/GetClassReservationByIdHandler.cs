using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Features.Classes.Common;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Classes.GetClassReservationById;

public class GetClassReservationByIdHandler(AppDbContext context)
    : IHandler<GetClassReservationByIdRequest, Result<GetClassReservationByIdResponse>>
{
    public async Task<Result<GetClassReservationByIdResponse>> HandleAsync(GetClassReservationByIdRequest request, CancellationToken ct = default)
    {
        var reservation = await context.Reservations
            .Where(r => r.Id == request.ReservationId)
            .Select(r => new GetClassReservationByIdResponse
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
                Notes = r.Notes,
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
                        EndsAt = c.EndsAt
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(ct);

        if (reservation is null)
            return Result<GetClassReservationByIdResponse>.Failure(ReservationError.NotFound(request.ReservationId));

        return Result<GetClassReservationByIdResponse>.Success(reservation);
    }
}
