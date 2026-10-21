using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.PaymentTransactions.RegisterPayment;

public class RegisterPaymentHandler(AppDbContext context, IUserClock userClock) : IHandler<RegisterPaymentRequest, Result<RegisterPaymentResponse>>
{
    public async Task<Result<RegisterPaymentResponse>> HandleAsync(RegisterPaymentRequest request, CancellationToken ct = default)
    {
        var reservation = await context.Reservations
            .AsSplitQuery()
            .Include(r => r.Classes)
            .Include(r => r.Projects)
            .Include(r => r.PaymentTransactions)
            .FirstOrDefaultAsync(r => r.Id == request.ReservationId, ct);

        if (reservation is null)
            return Result<RegisterPaymentResponse>.Failure(ReservationError.NotFound(request.ReservationId));

        var transactionDateUtc = request.Body.TransactionDate.UtcDateTime;

        var registerPaymentResult = reservation.RegisterPayment(request.Body.Amount, transactionDateUtc, userClock.Today);

        if(!registerPaymentResult.IsSuccess)
            return Result<RegisterPaymentResponse>.Failure(registerPaymentResult.Errors[0]);

        await context.SaveChangesAsync(ct);

        var newTransaction = registerPaymentResult.Payload!;

        var response = new RegisterPaymentResponse
        {
            Id = newTransaction.Id,
            ReservationId = newTransaction.ReservationId,
            Amount = newTransaction.Amount.Value,
            PaymentType = newTransaction.PaymentType,
            TransactionDate = transactionDateUtc,
            CreatedAt = newTransaction.CreatedAt
        };

        return Result<RegisterPaymentResponse>.Success(response);
    }
}
