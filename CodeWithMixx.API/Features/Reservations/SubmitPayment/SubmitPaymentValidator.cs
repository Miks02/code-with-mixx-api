using FluentValidation;

namespace CodeWithMixx.API.Features.Reservations.SubmitPayment;

public class SubmitPaymentValidator : AbstractValidator<SubmitPaymentRequest>
{
    public SubmitPaymentValidator()
    {
        RuleFor(x => x.Body.Amount)
            .GreaterThan(0).WithMessage("Payment amount must be greater than zero.");
    }
}
