using FluentValidation;

namespace CodeWithMixx.API.Features.Authentication.ActivateAccount;

public class ActivateAccountValidator : AbstractValidator<ActivateAccountRequest>
{
    public ActivateAccountValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User id is required.");
        
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token is required.");  
        
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long.");
        
        RuleFor(x => x.ConfirmedPassword)
            .NotEmpty().WithMessage("Confirmed password is required.")
            .Equal(x => x.Password).WithMessage("Passwords do not match.");       
    }
}