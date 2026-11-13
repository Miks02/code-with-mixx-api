using FluentValidation;

namespace CodeWithMixx.API.Features.Account.UpdateEmail;

public class UpdateEmailValidator : AbstractValidator<UpdateEmailRequest>
{
    public UpdateEmailValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token is required.")
            .Length(6).WithMessage("Enter a valid token.");
        
        RuleFor(x => x.NewEmail)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email address.");
        
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}