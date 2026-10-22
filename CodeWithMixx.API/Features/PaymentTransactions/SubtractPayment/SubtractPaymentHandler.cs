using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.PaymentTransactions.SubtractPayment;

public class SubtractPaymentHandler(AppDbContext context, IUserClock userClock) : IHandler<SubtractPaymentRequest, Result<SubtractPaymentResponse>>
{
    public async Task<Result<SubtractPaymentResponse>> HandleAsync(SubtractPaymentRequest request, CancellationToken ct = default)
    {
        var reservation = await context.Reservations
            .AsSplitQuery()
            .Include(r => r.Classes)
            .Include(r => r.Projects)
            .Include(r => r.PaymentTransactions)
            .FirstOrDefaultAsync(r => r.Id == request.ReservationId, ct);

        if (reservation is null)
            return Result<SubtractPaymentResponse>.Failure(ReservationError.NotFound(request.ReservationId));

        var transactionDateUtc = request.Body.TransactionDate.UtcDateTime;

        var subtractPaymentResult = reservation.SubtractPayment(request.Body.Amount, transactionDateUtc, request.Body.PaymentType, userClock.Today, request.Body.Note);

        if(!subtractPaymentResult.IsSuccess)
            return Result<SubtractPaymentResponse>.Failure(subtractPaymentResult.Errors[0]);

        await context.SaveChangesAsync(ct);

        var newTransaction = subtractPaymentResult.Payload!;

        var response = new SubtractPaymentResponse
        {
            Id = newTransaction.Id,
            ReservationId = newTransaction.ReservationId,
            Amount = newTransaction.Amount.Value,
            PaymentType = newTransaction.PaymentType,
            TransactionDate = transactionDateUtc,
            CreatedAt = newTransaction.CreatedAt
        };

        return Result<SubtractPaymentResponse>.Success(response);
    }
}
