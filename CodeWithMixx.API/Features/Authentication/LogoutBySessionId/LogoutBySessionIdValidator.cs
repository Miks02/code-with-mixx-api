using FluentValidation;

namespace CodeWithMixx.API.Features.Authentication.LogoutBySessionId;

public class LogoutBySessionIdValidator : AbstractValidator<LogoutBySessionIdRequest>
{
    public LogoutBySessionIdValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("Session ID is required.");
    }
}