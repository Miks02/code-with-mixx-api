using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Reservations.SubtractPayment;

public class SubtractPaymentHandler(AppDbContext context)
    : IHandler<SubtractPaymentRequest, Result<SubtractPaymentResponse>>
{
    public async Task<Result<SubtractPaymentResponse>> HandleAsync(SubtractPaymentRequest request, CancellationToken ct = default)
    {
        var reservation = await context.Reservations
            .Include(r => r.Classes)
            .Include(r => r.Projects)
            .FirstOrDefaultAsync(r => r.Id == request.ReservationId, ct);

        if (reservation is null)
            return Result<SubtractPaymentResponse>.Failure(ReservationError.NotFound(request.ReservationId));

        var paymentResult = reservation.SubtractPayment(request.Body.Amount);

        if (!paymentResult.IsSuccess)
            return Result<SubtractPaymentResponse>.Failure(paymentResult.Errors[0]);

        await context.SaveChangesAsync(ct);

        var response = new SubtractPaymentResponse
        {
            Id = reservation.Id,
            PaymentStatus = reservation.PaymentStatus,
            TotalPrice = reservation.TotalPrice,
            PaidAmount = reservation.PaidAmount,
            DiscountRate = reservation.DiscountRate,
            Bonus = reservation.Bonus,
            UpdatedAt = reservation.UpdatedAt
        };

        return Result<SubtractPaymentResponse>.Success(response);
    }
}
