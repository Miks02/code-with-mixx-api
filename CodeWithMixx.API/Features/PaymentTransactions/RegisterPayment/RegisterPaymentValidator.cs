using FluentValidation;

namespace CodeWithMixx.API.Features.PaymentTransactions.RegisterPayment;

public class RegisterPaymentValidator : AbstractValidator<RegisterPaymentBody>
{
    public RegisterPaymentValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Payment amount must be greater than zero.");

        RuleFor(x => x.TransactionDate)
            .NotEmpty().WithMessage("Transaction date is required.")
            .LessThanOrEqualTo(_ => DateTimeOffset.UtcNow).WithMessage("Transaction date cannot be in the future.");
        
        RuleFor(x => x.Note)
            .NotEmpty().WithMessage("Note is required for subtracting transactions.")
            .MinimumLength(20).WithMessage("Note must be at least 20 characters long.");

    }
}
