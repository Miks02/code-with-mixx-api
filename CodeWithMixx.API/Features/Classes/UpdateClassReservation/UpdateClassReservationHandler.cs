using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Domain.Entities.Students;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Classes.UpdateClassReservation;

public class UpdateClassReservationHandler(AppDbContext context)
    : IHandler<UpdateClassReservationRequest, Result<UpdateClassReservationResponse>>
{
    public async Task<Result<UpdateClassReservationResponse>> HandleAsync(UpdateClassReservationRequest request, CancellationToken ct = default)
    {
        var reservation = await context.Reservations
            .Include(r => r.Classes)
            .FirstOrDefaultAsync(r => r.Id == request.Id, ct);

        if (reservation is null)
            return Result<UpdateClassReservationResponse>.Failure(ReservationError.NotFound(request.Id));

        if (!string.Equals(reservation.StudentId, request.StudentId, StringComparison.Ordinal))
        {
            var studentExists = await context.Students.AnyAsync(s => s.UserId == request.StudentId, ct);

            if (!studentExists)
                return Result<UpdateClassReservationResponse>.Failure(StudentError.NotFound(request.StudentId));

            reservation.ChangeStudent(request.StudentId);
        }
        
        reservation.UpdateReservationStatus(request.ReservationStatus);
        reservation.UpdateNotes(request.Notes);

        var totalPriceResult = reservation.UpdateTotalPrice(request.TotalPrice);

        if (!totalPriceResult.IsSuccess)
            return Result<UpdateClassReservationResponse>.Failure(totalPriceResult.Errors[0]);

        var paidAmountDelta = request.PaidAmount - reservation.PaidAmount;

        if (paidAmountDelta > 0)
        {
            var paymentResult = reservation.RegisterPayment(paidAmountDelta);

            if (!paymentResult.IsSuccess)
                return Result<UpdateClassReservationResponse>.Failure(paymentResult.Errors[0]);
        }
        else if (paidAmountDelta < 0)
        {
            var paymentResult = reservation.SubtractPayment(-paidAmountDelta);

            if (!paymentResult.IsSuccess)
                return Result<UpdateClassReservationResponse>.Failure(paymentResult.Errors[0]);
        }

        await context.SaveChangesAsync(ct);

        var response = new UpdateClassReservationResponse
        {
            Id = reservation.Id,
            StudentId = reservation.StudentId,
            ReservationStatus = reservation.ReservationStatus,
            PaymentStatus = reservation.PaymentStatus,
            TotalPrice = reservation.TotalPrice,
            PaidAmount = reservation.PaidAmount,
            DiscountRate = reservation.DiscountRate,
            Bonus = reservation.Bonus,
            Notes = reservation.Notes,
            UpdatedAt = reservation.UpdatedAt
        };

        return Result<UpdateClassReservationResponse>.Success(response);
    }
}