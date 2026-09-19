namespace CodeWithMixx.API.Features.Reservations.SubmitPayment;

public record SubmitPaymentRequest
{
    public int ReservationId { get; init; }
    public SubmitPaymentBody Body { get; init; } = null!;
};

public record SubmitPaymentBody
{
    public decimal Amount { get; init; }
}
