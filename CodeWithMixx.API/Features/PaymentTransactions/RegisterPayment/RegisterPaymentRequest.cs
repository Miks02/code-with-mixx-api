namespace CodeWithMixx.API.Features.PaymentTransactions.RegisterPayment;

public record RegisterPaymentRequest
{
    public int ReservationId { get; init; }
    public RegisterPaymentBody Body { get; init; } = null!;
};

public record RegisterPaymentBody
{
    public decimal Amount { get; init; }
    public DateTimeOffset TransactionDate { get; init; }
    public string? Note { get; init; }
}
