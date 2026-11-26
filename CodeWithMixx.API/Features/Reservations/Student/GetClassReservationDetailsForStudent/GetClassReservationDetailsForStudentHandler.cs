using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Features.Reservations.Common;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Reservations.Student.GetClassReservationDetailsForStudent;

public class GetClassReservationDetailsForStudentHandler(AppDbContext context, IUserProvider userProvider)
    : IHandler<GetClassReservationDetailsForStudentRequest, Result<GetClassReservationDetailsForStudentResponse>>
{
    public async Task<Result<GetClassReservationDetailsForStudentResponse>> HandleAsync(
        GetClassReservationDetailsForStudentRequest request,
        CancellationToken ct = default)
    {
        var studentId = userProvider.GetUserId();

        var reservation = await context.Reservations
            .Where(r => r.Id == request.ReservationId
                        && r.StudentId == studentId
                        && r.ReservationType == ReservationType.Class)
            .Select(r => new GetClassReservationDetailsForStudentResponse
            {
                Id = r.Id,
                AdminId = r.AdminId,
                AdminFullName = r.Admin.User.FirstName + " " + r.Admin.User.LastName,
                AdminEmail = r.Admin.User.Email!,
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
                        Name = c.Name,
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
            return Result<GetClassReservationDetailsForStudentResponse>.Failure(ReservationError.NotFound(request.ReservationId));

        return Result<GetClassReservationDetailsForStudentResponse>.Success(reservation);
    }
}
