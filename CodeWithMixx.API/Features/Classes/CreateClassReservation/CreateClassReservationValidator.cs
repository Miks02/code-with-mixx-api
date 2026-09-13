using FluentValidation;

namespace CodeWithMixx.API.Features.Classes.CreateClassReservation;

public class CreateClassReservationRequestValidator : AbstractValidator<CreateClassReservationRequest>
{
    public CreateClassReservationRequestValidator()
    {
        RuleFor(x => x.StudentId)
            .NotEmpty().WithMessage("StudentId is required.");

        RuleFor(x => x.TotalPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Total price cannot be negative.");

        RuleFor(x => x.PaidAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Paid amount cannot be negative.");

        RuleFor(x => x.ReservationStatus)
            .IsInEnum().WithMessage("Invalid reservation status.");

        RuleFor(x => x.Classes)
            .NotEmpty().WithMessage("At least one class is required for a reservation.");

        RuleForEach(x => x.Classes)
            .SetValidator(new ClassDtoValidator());
    }
}

public class ClassDtoValidator : AbstractValidator<CreateClassReservationRequest.ClassDto>
{
    public ClassDtoValidator()
    {
        RuleFor(x => x.SubjectId)
            .GreaterThan(0).WithMessage("SubjectId must be a valid identifier.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Class price cannot be negative.");

        RuleFor(x => x.StartsAt)
            .NotEmpty().WithMessage("Start time is required.");

        RuleFor(x => x.EndsAt)
            .NotEmpty().WithMessage("End time is required.")
            .GreaterThan(x => x.StartsAt).WithMessage("End time must be after start time.");
    }
}