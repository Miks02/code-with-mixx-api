using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Domain.ValueObjects.Monies;

namespace CodeWithMixx.API.Domain.Entities.PaymentTransactions;

public class PaymentTransaction : IAuditable
{
    public int Id { get; private set; }
    public Money Amount { get; private set; } = Money.Zero;
    public PaymentTransactionType PaymentType { get; private set; }
    public DateTime TransactionDate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public Reservation Reservation { get; private set; } = null!;
    public int ReservationId { get; private set; }
    
    private PaymentTransaction() {}

    public static Result<PaymentTransaction> Create(Money amount, PaymentTransactionType paymentType, DateTime transactionDate, int reservationId)
    {
        if(amount.Value is 0)
            return Result<PaymentTransaction>.Failure(PaymentTransactionError.CannotBeZero());

        if(transactionDate > DateTime.UtcNow) 
            return Result<PaymentTransaction>.Failure(PaymentTransactionError.PaymentDateCannotBeInTheFuture());

        if(reservationId <= 0)
            return Result<PaymentTransaction>.Failure(PaymentTransactionError.InvalidReservation(reservationId));
        
        var newTransaction = new PaymentTransaction
        {
            Amount = amount,
            PaymentType = paymentType,
            TransactionDate = transactionDate,
            ReservationId = reservationId,
        };
        
        return Result<PaymentTransaction>.Success(newTransaction);
    }
    
}