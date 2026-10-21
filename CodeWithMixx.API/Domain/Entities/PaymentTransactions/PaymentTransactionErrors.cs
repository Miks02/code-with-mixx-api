using CodeWithMixx.API.Common.Result;

namespace CodeWithMixx.API.Domain.Entities.PaymentTransactions;

public static class PaymentTransactionError
{
    public static Error PaymentDateCannotBeInTheFuture()
        => new("PaymentTransaction.CannotBeInTheFuture", "Transaction date cannot be in the future", ErrorType.Validation);
    
    public static Error CannotBeZero() 
        => new("PaymentTransaction.CannotBeZero", "Amount cannot be zero", ErrorType.Validation);
    
    public static Error InvalidReservation(int identifier)
        => new("PaymentTransaction.InvalidReservation", $"Reservation with identifier {identifier} is invalid", ErrorType.Validation);
    
    public static Error InvalidPaymentType(PaymentTransactionType paymentType)
        => new("PaymentTransaction.InvalidPaymentType", $"Payment type {paymentType} is invalid for a given transaction.", ErrorType.Validation);
}