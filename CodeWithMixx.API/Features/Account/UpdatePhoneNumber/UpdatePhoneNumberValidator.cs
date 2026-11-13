using FluentValidation;

namespace CodeWithMixx.API.Features.Account.UpdatePhoneNumber;

public class UpdatePhoneNumberValidator : AbstractValidator<UpdatePhoneNumberRequest>
{
    public UpdatePhoneNumberValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^\+?\d{10,15}$").WithMessage("Invalid phone number format.");
    }
}