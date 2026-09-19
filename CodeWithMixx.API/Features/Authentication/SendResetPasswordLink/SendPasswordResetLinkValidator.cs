using FluentValidation;

namespace CodeWithMixx.API.Features.Authentication.SendResetPasswordLink;

public class SendPasswordResetLinkValidator : AbstractValidator<SendResetPasswordLinkRequest>
{
    public SendPasswordResetLinkValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");
    }
}