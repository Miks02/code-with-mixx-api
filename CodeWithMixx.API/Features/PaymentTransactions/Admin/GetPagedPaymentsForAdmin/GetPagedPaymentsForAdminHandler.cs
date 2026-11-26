using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Features.PaymentTransactions.Common;
using CodeWithMixx.API.Infrastructure.Persistence;

namespace CodeWithMixx.API.Features.PaymentTransactions.Admin.GetPagedPaymentsForAdmin;

public class GetPagedPaymentsForAdminHandler(AppDbContext context) : IHandler<GetPagedPaymentsForAdminRequest, PagedResult<PaymentTransactionItem>>
{
    public async Task<PagedResult<PaymentTransactionItem>> HandleAsync(GetPagedPaymentsForAdminRequest request, CancellationToken ct = default)
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

        return await PagedResult<PaymentTransactionItem>
            .CreateAsync(projectedPageQuery, request.PageNumber, request.PageSize, ct);
    }
}
