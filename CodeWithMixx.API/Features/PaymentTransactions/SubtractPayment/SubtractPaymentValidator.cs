using CodeWithMixx.API.Domain.Entities.PaymentTransactions;
using FluentValidation;

namespace CodeWithMixx.API.Features.PaymentTransactions.SubtractPayment;

public class SubtractPaymentValidator : AbstractValidator<SubtractPaymentBody>
{
    public SubtractPaymentValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount to subtract must be greater than zero.");

        RuleFor(x => x.TransactionDate)
            .NotEmpty().WithMessage("Transaction date is required.")
            .LessThanOrEqualTo(_ => DateTimeOffset.UtcNow).WithMessage("Transaction date cannot be in the future.");
        
        RuleFor(x => x.Note)
            .NotEmpty().WithMessage("Note is required for subtracting transactions.")
            .MinimumLength(20).WithMessage("Note must be at least 20 characters long.");
        
        RuleFor(x => x.PaymentType)
            .IsInEnum().WithMessage("Invalid payment type.")
            .When(x => x.PaymentType == PaymentTransactionType.Payment).WithMessage("Invalid payment type for subtraction.");
    }
}
