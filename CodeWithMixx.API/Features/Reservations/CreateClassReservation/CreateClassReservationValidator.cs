using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Domain.Entities.Classes;
using FluentValidation;

namespace CodeWithMixx.API.Features.Reservations.CreateClassReservation;

public class CreateClassReservationRequestValidator : AbstractValidator<CreateClassReservationRequest>
{
    public CreateClassReservationRequestValidator(IUserClock userClock)
    {
        RuleFor(x => x.StudentId)
            .NotEmpty().WithMessage("StudentId is required.");

        RuleFor(x => x.TotalPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Total price cannot be negative.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");

        RuleFor(x => x.ReservationStatus)
            .IsInEnum().WithMessage("Invalid reservation status.");

        RuleFor(x => x.Classes)
            .NotEmpty().WithMessage("At least one class is required for a reservation.");
        
        RuleFor(x => x.ReservedAt)
            .NotEmpty().WithMessage("ReservedAt is required.")
            .LessThanOrEqualTo(x => userClock.Today).WithMessage("Reservation date cannot be in the future.");

        RuleForEach(x => x.Classes)
            .SetValidator((request, _) => new ClassItemRequest(userClock, request.ReservedAt));
    }
}
internal class ClassItemRequest : AbstractValidator<CreateClassReservationRequest.ClassItemRequest>
{
    public ClassItemRequest(IUserClock userClock, DateOnly reservedAt)
    {
        RuleFor(x => x.SubjectId)
            .GreaterThan(0).WithMessage("SubjectId must be a valid identifier.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Class price cannot be negative or zero.");

        RuleFor(x => x.StartsAt)
            .NotEmpty().WithMessage("Start time is required.");

        RuleFor(x => x.EndsAt)
            .NotEmpty().WithMessage("End time is required.")
            .GreaterThan(x => x.StartsAt).WithMessage("End time must be after start time.")
            .Must((x, endsAt) => endsAt - x.StartsAt <= TimeSpan.FromMinutes(ClassConstants.MaxDurationInMinutes))
            .WithMessage($"Class duration cannot exceed {ClassConstants.MaxDurationInMinutes} minutes.");
        
        RuleFor(x => x.ReservedAt)
            .NotEmpty().WithMessage("Reservation date for classes is required") 
            .LessThanOrEqualTo(x => userClock.Today).WithMessage("Reservation date for classes cannot be in the future.")
            .GreaterThanOrEqualTo(x => reservedAt).WithMessage("Reservation date for classes cannot be before the reservation date.");
    }
}