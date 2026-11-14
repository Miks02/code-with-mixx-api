using System.Net.Mail;
using System.Text.RegularExpressions;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Admins;

namespace CodeWithMixx.API.Domain.Entities.Inquiries;

public class Inquiry : IAuditable, ISoftDeletable
{
    public int Id { get; private set; }
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string PhoneNumber { get; private set; } = null!;
    public string Message { get; private set; } = null!;
    public InquiryType InquiryType { get; private set; }
    public Admin? Admin { get; private set; }
    public string? ResolvedByAdmin { get; private set; }
    public DateTime? ResolvedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    private Inquiry() {}

    public static Result<Inquiry> Create(string firstName, string lastName, string email, string phoneNumber, string message, InquiryType inquiryType)
    {
        firstName = firstName.Trim();
        lastName = lastName.Trim();
        email = email.Trim();
        phoneNumber = phoneNumber.Trim();
        message = message.Trim();

        var validationResult = Validate(firstName, lastName, email, phoneNumber, message, inquiryType);

        if (!validationResult.IsSuccess)
            return Result<Inquiry>.Failure(validationResult.Errors[0]);

        return Result<Inquiry>.Success(new Inquiry
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PhoneNumber = phoneNumber,
            Message = message,
            InquiryType = inquiryType
        });
    }

    public Result Resolve(string resolvedByAdmin)
    {
        if(string.IsNullOrWhiteSpace(resolvedByAdmin))
            return Result.Failure(InquiryError.InvalidAdminId());
        
        if(ResolvedAt.HasValue || ResolvedByAdmin is not null)
            return Result.Failure(InquiryError.AlreadyResolved(ResolvedAt));

        var resolvedAt = DateTime.UtcNow;

        if(resolvedAt <= CreatedAt)
            return Result.Failure(InquiryError.InvalidResolvedAt());

        ResolvedByAdmin = resolvedByAdmin;
        ResolvedAt = resolvedAt;
        return Result.Success();
    }

    public Result Delete()
    {
        DeletedAt = DateTime.UtcNow;
        IsDeleted = true;
        return Result.Success();
    }

    private static Result Validate(string firstName, string lastName, string email, string phoneNumber, string message, InquiryType inquiryType)
    {
        if (firstName.Length == 0)
            return Result.Failure(InquiryError.FirstNameRequired());

        if (firstName.Length > InquiryConstants.MaxNameLength)
            return Result.Failure(InquiryError.FirstNameTooLong());

        if (lastName.Length == 0)
            return Result.Failure(InquiryError.LastNameRequired());

        if (lastName.Length > InquiryConstants.MaxNameLength)
            return Result.Failure(InquiryError.LastNameTooLong());

        if (email.Length == 0)
            return Result.Failure(InquiryError.EmailRequired());

        if (email.Length > InquiryConstants.MaxEmailLength)
            return Result.Failure(InquiryError.EmailTooLong());

        if (!IsValidEmail(email))
            return Result.Failure(InquiryError.InvalidEmail());

        if (phoneNumber.Length == 0)
            return Result.Failure(InquiryError.PhoneNumberRequired());

        if (!Regex.IsMatch(phoneNumber, InquiryConstants.PhoneNumberPattern))
            return Result.Failure(InquiryError.InvalidPhoneNumber());

        if (message.Length == 0)
            return Result.Failure(InquiryError.MessageRequired());

        if (message.Length < InquiryConstants.MinMessageLength)
            return Result.Failure(InquiryError.MessageTooShort());

        if (message.Length > InquiryConstants.MaxMessageLength)
            return Result.Failure(InquiryError.MessageTooLong());

        if (!Enum.IsDefined(inquiryType))
            return Result.Failure(InquiryError.InvalidInquiryType());

        return Result.Success();
    }

    private static bool IsValidEmail(string email)
        => MailAddress.TryCreate(email, out var address)
           && address.Address == email
           && address.Host.Contains('.');
}
