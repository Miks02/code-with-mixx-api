using CodeWithMixx.API.Common.Interfaces;
using FluentValidation;

namespace CodeWithMixx.API.Features.Reservations.UpdateProjectReservation;

public class UpdateProjectReservationValidator : AbstractValidator<UpdateProjectReservationRequest>
{
    public UpdateProjectReservationValidator(IUserClock clock)
    {
        RuleFor(x => x.StudentId)
            .NotEmpty().WithMessage("StudentId is required.");

        RuleFor(x => x.TotalPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Total price cannot be negative.");
        
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");

        RuleFor(x => x.ReservedAt)
            .NotEmpty().WithMessage("Reservation date is required.")
            .LessThanOrEqualTo(x => clock.Today).WithMessage("Reservation date cannot be in the future.");

        RuleFor(x => x.ReservationStatus)
            .IsInEnum().WithMessage("Invalid reservation status.");

        RuleFor(x => x.ProjectsToDelete)
            .Must(list => list.All(id => id > 0)).WithMessage("All project IDs to delete must be valid identifiers.");

        RuleFor(x => x.Projects)
            .NotEmpty().WithMessage("At least one project is required for a valid reservation.");

        RuleForEach(x => x.Projects)
            .SetValidator((request, _) => new UpdateProjectItemValidator(clock, request.ReservedAt));

        RuleFor(x => x)
            .Must(x => !x.ProjectsToDelete.Intersect(x.Projects.Select(p => p.Id)).Any())
            .WithMessage("A project cannot be both updated and deleted in the same request.")
            .WithName(nameof(UpdateProjectReservationRequest.ProjectsToDelete));
    }
}

internal class UpdateProjectItemValidator : AbstractValidator<UpdateProjectReservationRequest.ProjectItem>
{
    public UpdateProjectItemValidator(IUserClock clock, DateOnly reservationReservedAt)
    {
        RuleFor(x => x.Id)
            .GreaterThanOrEqualTo(0).WithMessage("Project id must be 0 (new project) or a valid identifier.");

        RuleFor(x => x.SubjectId)
            .GreaterThan(0).WithMessage("SubjectId must be a valid identifier.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Project price cannot be negative.");

        RuleFor(x => x.ProjectType)
            .IsInEnum().WithMessage("Invalid project type.");

        RuleFor(x => x.Progress)
            .InclusiveBetween(0, 100).WithMessage("Progress must be between 0 and 100.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required.")
            .GreaterThanOrEqualTo(x => x.ReservedAt).WithMessage("Start date cannot be before reservation date of a project.")
            .GreaterThanOrEqualTo(reservationReservedAt).WithMessage("Start date cannot be before reservation date.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required.")
            .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("End date cannot be before start date.");

        RuleFor(x => x.ReservedAt)
            .NotEmpty().WithMessage("Reservation date is required.")
            .LessThanOrEqualTo(clock.Today).WithMessage("Reservation date cannot be in the future.");

        RuleFor(x => x.GithubLink)
            .MaximumLength(200).WithMessage("GitHub link must be less than 200 characters.")
            .When(x => x.GithubLink is not null);

        RuleFor(x => x.DownloadLink)
            .MaximumLength(200).WithMessage("Download link must be less than 200 characters.")
            .When(x => x.DownloadLink is not null);

        RuleForEach(x => x.Notes)
            .NotEmpty().WithMessage("Project note content cannot be empty.");
        RuleForEach(x => x.Notes)
            .SetValidator(new ProjectNoteValidator());
    }
}

public class ProjectNoteValidator : AbstractValidator<UpdateProjectReservationRequest.ProjectNoteItem>
{
    public ProjectNoteValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Project note content cannot be empty.");
    }
}