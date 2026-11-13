namespace CodeWithMixx.API.Features.Account.UpdatePhoneNumber;

public record UpdatePhoneNumberResponse
{
    public string PhoneNumber { get; init; } = null!;
}