using CodeWithMixx.API.Domain.Entities.Students;
using FluentValidation;

namespace CodeWithMixx.API.Features.Account.UpdateUniversity;

public class UpdateUniversityValidator : AbstractValidator<UpdateUniversityRequest>
{
    public UpdateUniversityValidator()
    {
        RuleFor(x => x.University)
            .NotEmpty()
            .WithMessage("University is required.")
            .MaximumLength(StudentConstants.MaxUniversityLength)
            .WithMessage($"University name cannot exceed {StudentConstants.MaxUniversityLength} characters.");
    }
}
