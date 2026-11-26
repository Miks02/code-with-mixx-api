using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.PaymentTransactions;
using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Features.PaymentTransactions.Common;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.PaymentTransactions.Student.GetPaymentsByReservationIdForStudent;

public class GetPaymentsByReservationIdForStudentHandler(AppDbContext context, IUserProvider userProvider)
    : IHandler<GetPaymentsByReservationIdForStudentRequest, Result<GetPaymentsByReservationIdForStudentResponse>>
{
    public async Task<Result<GetPaymentsByReservationIdForStudentResponse>> HandleAsync(GetPaymentsByReservationIdForStudentRequest request, CancellationToken ct = default)
    {
        var studentId = userProvider.GetUserId();

        var reservation = await context.Reservations
            .IgnoreQueryFilters()
            .Include(r => r.PaymentTransactions)
            .Where(r => r.Id == request.ReservationId && r.StudentId == studentId)
            .FirstOrDefaultAsync(ct);

        if (reservation is null)
            return Result<GetPaymentsByReservationIdForStudentResponse>.Failure(ReservationError.NotFound(request.ReservationId));

        var payments = reservation.PaymentTransactions
            .OrderByDescending(pt => pt.TransactionDate)
            .Select(pt => new PaymentTransactionItem
            {
                Id = pt.Id,
                Amount = pt.Amount.Value,
                PaymentType = pt.PaymentType,
                ReservationId = pt.ReservationId,
                TransactionDate = pt.TransactionDate,
                CreatedAt = pt.CreatedAt,
                Note = pt.TransactionNote.Note
            })
            .ToList();

        var response = new GetPaymentsByReservationIdForStudentResponse
        {
            ReservationId = reservation.Id,
            RemainingAmount = reservation.RemainingAmount,
            BonusAmount = reservation.Bonus,
            ReversedSum = payments.Where(pt => pt.PaymentType == PaymentTransactionType.Reversal).Sum(p => p.Amount),
            TotalSum = reservation.PaidAmount,
            PaidSum = payments.Where(pt => pt.PaymentType == PaymentTransactionType.Payment).Sum(p => p.Amount),
            Payments = payments
        };

        return Result<GetPaymentsByReservationIdForStudentResponse>.Success(response);
    }
}
