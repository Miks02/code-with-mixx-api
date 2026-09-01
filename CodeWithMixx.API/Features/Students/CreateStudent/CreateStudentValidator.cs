using FluentValidation;

namespace CodeWithMixx.API.Features.Students.CreateStudent
{
    public class CreateStudentValidator : AbstractValidator<CreateStudentRequest>
    {
        public CreateStudentValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email address.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .Matches(@"^\+?\d{10,15}$").WithMessage("Invalid phone number format.");

            RuleFor(x => x.University)
                .MaximumLength(100).WithMessage("University name cannot exceed 100 characters.")
                .When(x => !string.IsNullOrEmpty(x.University));
        }
    }
}
