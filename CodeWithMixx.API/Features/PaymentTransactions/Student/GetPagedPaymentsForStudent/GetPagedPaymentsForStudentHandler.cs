using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Features.PaymentTransactions.Common;
using CodeWithMixx.API.Infrastructure.Persistence;

namespace CodeWithMixx.API.Features.PaymentTransactions.Student.GetPagedPaymentsForStudent;

public class GetPagedPaymentsForStudentHandler(AppDbContext context, IUserProvider userProvider) : IHandler<GetPagedPaymentsForStudentRequest, PagedResult<PaymentTransactionItem>>
{
    public async Task<PagedResult<PaymentTransactionItem>> HandleAsync(GetPagedPaymentsForStudentRequest request, CancellationToken ct = default)
    {
        var studentId = userProvider.GetUserId();

        var studentTransactions = context.PaymentTransactions
            .Where(pt => pt.Reservation.StudentId == studentId);

        var query = studentTransactions;

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
