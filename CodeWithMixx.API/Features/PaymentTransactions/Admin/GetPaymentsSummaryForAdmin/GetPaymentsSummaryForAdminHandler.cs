using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.PaymentTransactions;
using CodeWithMixx.API.Features.PaymentTransactions.Common;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.PaymentTransactions.Admin.GetPaymentsSummaryForAdmin;

public class GetPaymentsSummaryForAdminHandler(AppDbContext context) : IHandler<GetPaymentsSummaryForAdminRequest, GetPaymentsSummaryForAdminResponse>
{
    public async Task<GetPaymentsSummaryForAdminResponse> HandleAsync(GetPaymentsSummaryForAdminRequest request, CancellationToken ct = default)
    {
        var query = context.PaymentTransactions.AsQueryable();

        if (request.PaymentTypes.Length > 0)
            query = query.Where(pt => Enumerable.Contains(request.PaymentTypes, pt.PaymentType));

        query = request.SortBy switch
        {
            PaymentTransactionsSortBy.TransactionDateAscending => query.OrderBy(pt => pt.TransactionDate).ThenBy(pt => pt.Id),
            PaymentTransactionsSortBy.TransactionDateDescending => query.OrderByDescending(pt => pt.TransactionDate).ThenByDescending(pt => pt.Id),
            PaymentTransactionsSortBy.ReservationIdAscending => query.OrderBy(pt => pt.ReservationId).ThenByDescending(pt => pt.TransactionDate),
            PaymentTransactionsSortBy.ReservationIdDescending => query.OrderByDescending(pt => pt.ReservationId).ThenByDescending(pt => pt.TransactionDate),
            _ => query.OrderByDescending(pt => pt.TransactionDate).ThenByDescending(pt => pt.Id)
        };

        var projectedPageQuery = query
            .Select(pt => new PaymentTransactionItem
            {
                Id = pt.Id,
                Amount = pt.Amount.Value,
                PaymentType = pt.PaymentType,
                ReservationId = pt.ReservationId,
                TransactionDate = pt.TransactionDate,
                CreatedAt = pt.CreatedAt,
                Note = pt.TransactionNote.Note
            });

        var pagedPayments = await PagedResult<PaymentTransactionItem>
            .CreateAsync(projectedPageQuery, request.PageNumber, request.PageSize, ct);

        var transactionStats = await context.PaymentTransactions
            .GroupBy(pt => 1)
            .Select(g => new
            {
                TotalTransactions = g.Count(),
                PaymentsCount = g.Count(pt => pt.PaymentType == PaymentTransactionType.Payment),
                RefundsCount = g.Count(pt => pt.PaymentType == PaymentTransactionType.Refund),
                ReversalsCount = g.Count(pt => pt.PaymentType == PaymentTransactionType.Reversal)
            })
            .FirstOrDefaultAsync(ct);

        var debtStats = await context.Reservations
            .Where(r => r.TotalPrice > r.PaidAmount)
            .GroupBy(r => 1)
            .Select(g => new
            {
                TotalDebt = g.Sum(r => r.TotalPrice - r.PaidAmount),
                DebtsCount = g.Count()
            })
            .FirstOrDefaultAsync(ct);

        return new GetPaymentsSummaryForAdminResponse
        {
            PagedPayments = pagedPayments,
            TotalTransactions = transactionStats?.TotalTransactions ?? 0,
            PaymentsCount = transactionStats?.PaymentsCount ?? 0,
            RefundsCount = transactionStats?.RefundsCount ?? 0,
            ReversalsCount = transactionStats?.ReversalsCount ?? 0,
            TotalDebt = debtStats?.TotalDebt ?? 0,
            DebtsCount = debtStats?.DebtsCount ?? 0
        };
    }
}
