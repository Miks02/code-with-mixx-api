using CodeWithMixx.API.Domain.Entities.Inquiries;
using FluentValidation;

namespace CodeWithMixx.API.Features.Inquiries.SendInquiry;

public class SendInquiryValidator : AbstractValidator<SendInquiryRequest>
{
    public SendInquiryValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(InquiryConstants.MaxNameLength)
            .WithMessage($"First name cannot exceed {InquiryConstants.MaxNameLength} characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(InquiryConstants.MaxNameLength)
            .WithMessage($"Last name cannot exceed {InquiryConstants.MaxNameLength} characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .MaximumLength(InquiryConstants.MaxEmailLength)
            .WithMessage($"Email cannot exceed {InquiryConstants.MaxEmailLength} characters.")
            .EmailAddress().WithMessage("Invalid email address.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(InquiryConstants.PhoneNumberPattern).WithMessage("Invalid phone number format.");

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required.")
            .MinimumLength(InquiryConstants.MinMessageLength)
            .WithMessage($"Message must be at least {InquiryConstants.MinMessageLength} characters long.")
            .MaximumLength(InquiryConstants.MaxMessageLength)
            .WithMessage($"Message cannot exceed {InquiryConstants.MaxMessageLength} characters.");

        RuleFor(x => x.InquiryType)
            .IsInEnum().WithMessage("Invalid inquiry type.");
    }
}
