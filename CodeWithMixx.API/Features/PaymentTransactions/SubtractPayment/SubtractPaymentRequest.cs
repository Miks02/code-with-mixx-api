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
}
