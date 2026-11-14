namespace CodeWithMixx.API.Domain.Entities.Inquiries;

public static class InquiryConstants
{
    public const int MaxNameLength = 50;
    public const int MaxEmailLength = 256;
    public const string PhoneNumberPattern = @"^\+?\d{10,15}$";
    public const int MinMessageLength = 10;
    public const int MaxMessageLength = 500;
    
    // Used for deleting multiple inquiries at once.
    public const int MaxInquiryIds = 100;

}
