using CodeWithMixx.API.Domain.Entities.PaymentTransactions;
using CodeWithMixx.API.Features.PaymentTransactions.Common;
using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.PaymentTransactions.Student.GetPaymentsSummaryForStudent;

public record GetPaymentsSummaryForStudentRequest
{
    [FromQuery(Name = "pageNumber")]
    public int PageNumber { get; init; } = 1;
    [FromQuery(Name = "pageSize")]
    public int PageSize { get; init; } = 10;
    [FromQuery(Name = "paymentTypes")]
    public PaymentTransactionType[] PaymentTypes { get; init; } = [];
    [FromQuery(Name = "sortBy")]
    public PaymentTransactionsSortBy? SortBy { get; init; } = PaymentTransactionsSortBy.TransactionDateDescending;
}
