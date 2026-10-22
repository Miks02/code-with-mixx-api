using CodeWithMixx.API.Domain.Entities.PaymentTransactions;

namespace CodeWithMixx.API.Features.PaymentTransactions.SubtractPayment;

public record SubtractPaymentRequest
{
    public int ReservationId { get; init; }
    public SubtractPaymentBody Body { get; init; } = null!;
};

public record SubtractPaymentBody
{
    public decimal Amount { get; init; }
    public DateTimeOffset TransactionDate { get; init; }
    public string Note { get; init; } = null!;
    public PaymentTransactionType PaymentType { get; init; }
}
