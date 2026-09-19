using FluentValidation;

namespace CodeWithMixx.API.Features.Projects.CreateProjectReservation;

public class CreateProjectReservationValidator : AbstractValidator<CreateProjectReservationRequest>
{
    public CreateProjectReservationValidator()
    {
        RuleFor(x => x.StudentId)
            .NotEmpty().WithMessage("StudentId is required.");

        RuleFor(x => x.TotalPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Total price cannot be negative.")
            .When(x => x.TotalPrice is not null);

        RuleFor(x => x.PaidAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Paid amount cannot be negative.");

        RuleFor(x => x.ReservationStatus)
            .IsInEnum().WithMessage("Invalid reservation status.");

        RuleFor(x => x.Projects)
            .NotEmpty().WithMessage("At least one project is required for a reservation.");

        RuleForEach(x => x.Projects)
            .SetValidator(new ProjectItemValidator());
    }
}

public class ProjectItemValidator : AbstractValidator<CreateProjectReservationRequest.ProjectItem>
{
    public ProjectItemValidator()
    {
        RuleFor(x => x.SubjectId)
            .GreaterThan(0).WithMessage("SubjectId must be a valid identifier.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Project price cannot be negative.");

        RuleFor(x => x.ProjectType)
            .IsInEnum().WithMessage("Invalid project type.");

        RuleFor(x => x.Progress)
            .InclusiveBetween(0, 100).WithMessage("Progress must be between 0 and 100.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required.")
            .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("End date cannot be before start date.");

        RuleFor(x => x.ReservedAt)
            .NotEmpty().WithMessage("Reservation date is required.")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Reservation date cannot be in the future.");

        RuleFor(x => x.GithubLink)
            .MaximumLength(200).WithMessage("GitHub link must be less than 200 characters.")
            .When(x => x.GithubLink is not null);

        RuleFor(x => x.DownloadLink)
            .MaximumLength(200).WithMessage("Download link must be less than 200 characters.")
            .When(x => x.DownloadLink is not null);

        RuleForEach(x => x.Notes)
            .NotEmpty().WithMessage("Project note content cannot be empty.");
    }
}
