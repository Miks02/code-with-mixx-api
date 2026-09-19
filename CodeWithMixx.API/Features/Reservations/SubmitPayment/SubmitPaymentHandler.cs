using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Reservations.SubmitPayment;

public class SubmitPaymentHandler(AppDbContext context)
    : IHandler<SubmitPaymentRequest, Result<SubmitPaymentResponse>>
{
    public async Task<Result<SubmitPaymentResponse>> HandleAsync(SubmitPaymentRequest request, CancellationToken ct = default)
    {
        var reservation = await context.Reservations
            .Include(r => r.Classes)
            .Include(r => r.Projects)
            .FirstOrDefaultAsync(r => r.Id == request.ReservationId, ct);

        if (reservation is null)
            return Result<SubmitPaymentResponse>.Failure(ReservationError.NotFound(request.ReservationId));

        var paymentResult = reservation.RegisterPayment(request.Body.Amount);

        if (!paymentResult.IsSuccess)
            return Result<SubmitPaymentResponse>.Failure(paymentResult.Errors[0]);

        await context.SaveChangesAsync(ct);

        var response = new SubmitPaymentResponse
        {
            Id = reservation.Id,
            PaymentStatus = reservation.PaymentStatus,
            TotalPrice = reservation.TotalPrice,
            PaidAmount = reservation.PaidAmount,
            DiscountRate = reservation.DiscountRate,
            Bonus = reservation.Bonus,
            UpdatedAt = reservation.UpdatedAt
        };

        return Result<SubmitPaymentResponse>.Success(response);
    }
}
