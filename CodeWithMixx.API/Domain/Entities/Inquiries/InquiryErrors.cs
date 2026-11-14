using CodeWithMixx.API.Common.Result;

namespace CodeWithMixx.API.Domain.Entities.Inquiries;

public static class InquiryError
{
    public static Error NotFound(int? identifier = null)
    {
        string message = identifier is null
            ? "Inquiry not found"
            : $"Inquiry with identifier '{identifier}' is not found";

        return new Error("Inquiry.NotFound", message, ErrorType.NotFound);
    }

    public static Error FirstNameRequired()
        => new("Inquiry.FirstNameRequired", "First name is required", ErrorType.Validation);

    public static Error FirstNameTooLong()
        => new("Inquiry.FirstNameTooLong", $"First name cannot exceed {InquiryConstants.MaxNameLength} characters", ErrorType.Validation);

    public static Error LastNameRequired()
        => new("Inquiry.LastNameRequired", "Last name is required", ErrorType.Validation);

    public static Error LastNameTooLong()
        => new("Inquiry.LastNameTooLong", $"Last name cannot exceed {InquiryConstants.MaxNameLength} characters", ErrorType.Validation);

    public static Error EmailRequired()
        => new("Inquiry.EmailRequired", "Email is required", ErrorType.Validation);

    public static Error EmailTooLong()
        => new("Inquiry.EmailTooLong", $"Email cannot exceed {InquiryConstants.MaxEmailLength} characters", ErrorType.Validation);

    public static Error InvalidEmail()
        => new("Inquiry.InvalidEmail", "Email address is not valid", ErrorType.Validation);

    public static Error PhoneNumberRequired()
        => new("Inquiry.PhoneNumberRequired", "Phone number is required", ErrorType.Validation);

    public static Error InvalidPhoneNumber()
        => new("Inquiry.InvalidPhoneNumber", "Phone number is not valid. It must contain 10 to 15 digits, with an optional leading '+'", ErrorType.Validation);

    public static Error MessageRequired()
        => new("Inquiry.MessageRequired", "Message is required", ErrorType.Validation);

    public static Error MessageTooShort()
        => new("Inquiry.MessageTooShort", $"Message must be at least {InquiryConstants.MinMessageLength} characters long", ErrorType.Validation);

    public static Error MessageTooLong()
        => new("Inquiry.MessageTooLong", $"Message cannot exceed {InquiryConstants.MaxMessageLength} characters", ErrorType.Validation);

    public static Error InvalidInquiryType()
        => new("Inquiry.InvalidInquiryType", "Inquiry type is not valid", ErrorType.Validation);
    
    public static Error InvalidAdminId()
        => new("Inquiry.InvalidAdminId", "Admin identifier is required to resolve an inquiry", ErrorType.Validation);

    public static Error AlreadyResolved(DateTime? resolvedAt = null)
        => new(
            "Inquiry.AlreadyResolved",
            resolvedAt is null
                ? "Inquiry is already resolved"
                : $"Inquiry is already resolved at {resolvedAt.Value.ToUniversalTime():yyyy-MM-dd HH:mm:ss} UTC",
            ErrorType.Conflict);
    
    public static Error InvalidResolvedAt()
        => new("Inquiry.InvalidResolvedAt", "Resolved at date is not valid. Must be after 'CreatedAt'", ErrorType.Validation);
}
