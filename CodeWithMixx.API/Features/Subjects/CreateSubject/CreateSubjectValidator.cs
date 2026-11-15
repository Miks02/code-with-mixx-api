using CodeWithMixx.API.Domain.Entities.Subjects;
using FluentValidation;

namespace CodeWithMixx.API.Features.Subjects.CreateSubject;

public class CreateSubjectValidator : AbstractValidator<CreateSubjectRequest>
{
    public CreateSubjectValidator()
    {
        RuleFor(x => x.SubjectName)
            .NotEmpty().WithMessage("Subject name is required.")
            .MaximumLength(SubjectConstants.MaxNameLength).WithMessage($"Subject name must not exceed {SubjectConstants.MaxNameLength} characters.");
        
        RuleFor(x => x.SubjectDescription)
            .NotEmpty().WithMessage("Subject description is required.")
            .MaximumLength(SubjectConstants.MaxDescriptionLength).WithMessage($"Subject description must not exceed {SubjectConstants.MaxDescriptionLength} characters.");
    }
}