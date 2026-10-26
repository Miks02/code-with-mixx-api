using FluentValidation;

namespace CodeWithMixx.API.Features.Projects.UpdateProject;

public class UpdateProjectValidator : AbstractValidator<UpdateProjectRequest>
{
    public UpdateProjectValidator()
    {
        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Project price cannot be negative.");

        RuleFor(x => x.Progress)
            .InclusiveBetween(0, 100).WithMessage("Progress must be between 0 and 100.");

        RuleFor(x => x.GithubLink)
            .MaximumLength(200).WithMessage("GitHub link must be less than 200 characters.")
            .When(x => x.GithubLink is not null);

        RuleFor(x => x.DownloadLink)
            .MaximumLength(200).WithMessage("Download link must be less than 200 characters.")
            .When(x => x.DownloadLink is not null);

        RuleFor(x => x.NotesToDelete)
            .Must(list => list.All(id => id > 0)).WithMessage("All note IDs to delete must be valid identifiers.");

        RuleForEach(x => x.NotesToAdd)
            .NotEmpty().WithMessage("Project note content cannot be empty.")
            .MaximumLength(500).WithMessage("Project note cannot exceed 500 characters.");
    }
}
