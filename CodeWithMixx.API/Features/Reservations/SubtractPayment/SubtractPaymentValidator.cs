using FluentValidation;

namespace CodeWithMixx.API.Features.Reservations.SubtractPayment;

public class SubtractPaymentValidator : AbstractValidator<SubtractPaymentBody>
{
    public SubtractPaymentValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount to subtract must be greater than zero.");
    }
}
