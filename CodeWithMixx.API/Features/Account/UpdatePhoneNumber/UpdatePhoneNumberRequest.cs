namespace CodeWithMixx.API.Features.Account.UpdatePhoneNumber;

public record UpdatePhoneNumberRequest
{
    public string PhoneNumber { get; init; } = null!;
};