using FluentValidation;

namespace CodeWithMixx.API.Features.Authentication.ResetPassword;

public class ResetPasswordValidator : AbstractValidator<ResetPasswordBody>
{
    public ResetPasswordValidator()
    {
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long.");

        RuleFor(x => x.ConfirmedPassword)
            .NotEmpty().WithMessage("Confirmed password is required.")
            .Equal(x => x.Password).WithMessage("Passwords do not match.");
    }
}