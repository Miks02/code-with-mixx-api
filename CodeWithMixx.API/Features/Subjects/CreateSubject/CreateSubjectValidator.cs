using FluentValidation;

namespace CodeWithMixx.API.Features.Subjects.CreateSubject;

public class CreateSubjectValidator : AbstractValidator<CreateSubjectRequest>
{
    public CreateSubjectValidator()
    {
        RuleFor(x => x.SubjectName)
            .NotEmpty().WithMessage("Subject name is required.")
            .MaximumLength(100).WithMessage("Subject name must not exceed 100 characters.");
        
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Subject description is required.")
            .MaximumLength(500).WithMessage("Subject description must not exceed 500 characters.");
    }
}