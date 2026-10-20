using CodeWithMixx.API.Domain.Entities.Reservations;

namespace CodeWithMixx.API.Domain.Entities.PaymentTransactions;

public class PaymentTransaction : IAuditable
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public PaymentTransactionType PaymentType { get; set; }
    public DateTime TransactionDate { get; set; }
    public DateTime CreatedAt { get; }
    public DateTime? UpdatedAt { get; }

    public Reservation Reservation { get; set; } = null!;
    public int ReservationId { get; set; }
}