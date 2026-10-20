using FluentValidation;

namespace CodeWithMixx.API.Features.Reservations.UpdateReservationStatus;

public class UpdateReservationStatusValidator : AbstractValidator<UpdateReservationStatusBody>
{
    public UpdateReservationStatusValidator()
    {
        RuleFor(x => x.ReservationStatus)
            .IsInEnum().WithMessage("Invalid reservation status.");
    }
}
