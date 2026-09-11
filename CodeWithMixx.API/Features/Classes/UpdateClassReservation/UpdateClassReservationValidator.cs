using FluentValidation;

namespace CodeWithMixx.API.Features.Classes.UpdateClassReservation;

public class UpdateClassReservationRequestValidator : AbstractValidator<UpdateClassReservationRequest>
{
    public UpdateClassReservationRequestValidator()
    {
        RuleFor(x => x.StudentId)
            .NotEmpty().WithMessage("StudentId is required.");

        RuleFor(x => x.TotalPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Total price cannot be negative.");

        RuleFor(x => x.PaidAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Paid amount cannot be negative.");

        RuleFor(x => x.ReservationStatus)
            .IsInEnum().WithMessage("Invalid reservation status.");
    }
}
