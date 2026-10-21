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
    }
}
