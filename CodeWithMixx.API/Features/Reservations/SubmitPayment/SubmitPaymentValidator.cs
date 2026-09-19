using FluentValidation;

namespace CodeWithMixx.API.Features.Reservations.SubmitPayment;

public class SubmitPaymentValidator : AbstractValidator<SubmitPaymentBody>
{
    public SubmitPaymentValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Payment amount must be greater than zero.");
    }
}
