using CodeWithMixx.API.Features.PaymentTransactions.Common;

namespace CodeWithMixx.API.Features.PaymentTransactions.GetPaymentsByReservationId;

public record GetPaymentsByReservationIdResponse
{
    public int ReservationId { get; init; }
    public decimal RemainingAmount { get; init; }
    public decimal BonusAmount { get; init; }
    public decimal TotalSum { get; init; }
    public decimal PaidSum { get; init; }
    public decimal ReversedSum { get; init; }
    public IReadOnlyList<PaymentTransactionItem> Payments { get; init; }
};