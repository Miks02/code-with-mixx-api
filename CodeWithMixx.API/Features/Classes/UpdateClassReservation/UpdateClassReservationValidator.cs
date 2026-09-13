using CodeWithMixx.API.Features.Classes.Common;
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
        
        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters.");

        RuleFor(x => x.ReservationStatus)
            .IsInEnum().WithMessage("Invalid reservation status.");

        RuleFor(x => x.ClassesToDelete)
            .Must(list => list.All(id => id > 0)).WithMessage("All class IDs to delete must be valid identifiers.");

        RuleFor(x => x.Classes)
            .NotEmpty().WithMessage("At least one class is required for a valid reservation");

        RuleForEach(x => x.Classes)
            .SetValidator(new ClassItemValidator());

        RuleFor(x => x)
            .Must(x => !x.ClassesToDelete.Intersect(x.Classes.Select(c => c.Id)).Any())
            .WithMessage("A class cannot be both updated and deleted in the same request.")
            .WithName(nameof(UpdateClassReservationRequest.ClassesToDelete));
    }
}

public class ClassItemValidator : AbstractValidator<ClassItem>
{
    public ClassItemValidator()
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
