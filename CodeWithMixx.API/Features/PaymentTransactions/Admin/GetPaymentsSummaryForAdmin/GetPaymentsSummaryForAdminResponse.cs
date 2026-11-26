using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Features.PaymentTransactions.Common;

namespace CodeWithMixx.API.Features.PaymentTransactions.Admin.GetPaymentsSummaryForAdmin;

public record GetPaymentsSummaryForAdminResponse
{
    public PagedResult<PaymentTransactionItem> PagedPayments { get; init; } = null!;
    public int TotalTransactions { get; init; }
    public int PaymentsCount { get; init; }
    public int RefundsCount { get; init; }
    public int ReversalsCount { get; init; }
    public decimal TotalDebt { get; init; }
    public int DebtsCount { get; init; }
}
