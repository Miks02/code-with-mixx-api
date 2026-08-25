using FluentValidation;

namespace CodeWithMixx.API.Features.Authentication.Login;

public class LoginValidator : AbstractValidator<LoginRequest>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email adress is required")
            .EmailAddress().WithMessage("Email adress is not valid");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}