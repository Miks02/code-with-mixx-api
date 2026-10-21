using CodeWithMixx.API.Domain.Entities.PaymentTransactions;

namespace CodeWithMixx.API.Features.PaymentTransactions.Common;

public record PaymentTransactionItem
{
    public int Id { get; init; }
    public int ReservationId { get; init; }
    public decimal Amount { get; init; }
    public PaymentTransactionType PaymentType { get; init; }
    public DateTimeOffset TransactionDate { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
};