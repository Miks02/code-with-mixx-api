using CodeWithMixx.API.Domain.Entities.Reservations;

namespace CodeWithMixx.API.Features.Reservations.SubtractPayment;

public record SubtractPaymentResponse
{
    public int Id { get; init; }
    public PaymentStatus PaymentStatus { get; init; }
    public decimal TotalPrice { get; init; }
    public decimal PaidAmount { get; init; }
    public decimal DiscountRate { get; init; }
    public decimal Bonus { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
