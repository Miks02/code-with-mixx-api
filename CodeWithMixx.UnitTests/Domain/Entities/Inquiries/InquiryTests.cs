using AwesomeAssertions;
using CodeWithMixx.API.Common.Result;
using CodeWithMixx.API.Domain.Entities.Inquiries;
using Xunit;

namespace CodeWithMixx.UnitTests.Domain.Entities.Inquiries;

public class InquiryTests
{
    private const string FirstName = "Petar";
    private const string LastName = "Petrovic";
    private const string Email = "petar@example.com";
    private const string PhoneNumber = "+381601234567";
    private const string Message = "Treba mi pomoc oko seminarskog rada.";
    private const InquiryType Type = InquiryType.Project;
    private const string AdminId = "admin-1";

    private readonly Inquiry _sut;

    public InquiryTests()
    {
        _sut = CreateValid().Payload!;
    }

    [Fact]
    public void Create_WithValidValues_ShouldSucceedAndMapValues()
    {
        var result = CreateValid();

        result.IsSuccess.Should().BeTrue();
        var inquiry = result.Payload!;
        inquiry.FirstName.Should().Be(FirstName);
        inquiry.LastName.Should().Be(LastName);
        inquiry.Email.Should().Be(Email);
        inquiry.PhoneNumber.Should().Be(PhoneNumber);
        inquiry.Message.Should().Be(Message);
        inquiry.InquiryType.Should().Be(Type);
    }

    [Fact]
    public void Create_ShouldStartUnresolved()
    {
        _sut.ResolvedAt.Should().BeNull();
        _sut.ResolvedByAdmin.Should().BeNull();
        _sut.Admin.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldTrimAllTextFields()
    {
        var result = Inquiry.Create($"  {FirstName} ", $"\t{LastName}\n", $" {Email} ", $" {PhoneNumber} ", $"  {Message}  ", Type);

        var inquiry = result.Payload!;
        inquiry.FirstName.Should().Be(FirstName);
        inquiry.LastName.Should().Be(LastName);
        inquiry.Email.Should().Be(Email);
        inquiry.PhoneNumber.Should().Be(PhoneNumber);
        inquiry.Message.Should().Be(Message);
    }

    [Theory]
    [InlineData(InquiryType.IndividualClasses)]
    [InlineData(InquiryType.ClassPackage)]
    [InlineData(InquiryType.Project)]
    [InlineData(InquiryType.Other)]
    public void Create_WithEveryDefinedInquiryType_ShouldSucceed(InquiryType type)
    {
        var result = CreateValid(inquiryType: type);

        result.IsSuccess.Should().BeTrue();
        result.Payload!.InquiryType.Should().Be(type);
    }

    [Theory]
    [InlineData("Đorđe")]
    [InlineData("Ana-Marija")]
    public void Create_WithNonAsciiOrHyphenatedName_ShouldSucceed(string firstName)
    {
        CreateValid(firstName: firstName).IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankFirstName_ShouldReturnFirstNameRequired(string firstName)
        => AssertFails(CreateValid(firstName: firstName), InquiryError.FirstNameRequired());

    [Fact]
    public void Create_WithFirstNameAtMaxLength_ShouldSucceed()
        => CreateValid(firstName: new string('a', InquiryConstants.MaxNameLength)).IsSuccess.Should().BeTrue();

    [Fact]
    public void Create_WithFirstNameOverMaxLength_ShouldReturnFirstNameTooLong()
        => AssertFails(CreateValid(firstName: new string('a', InquiryConstants.MaxNameLength + 1)), InquiryError.FirstNameTooLong());

    [Fact]
    public void Create_WithPaddedFirstNameThatFitsAfterTrim_ShouldSucceed()
        => CreateValid(firstName: $"  {new string('a', InquiryConstants.MaxNameLength)}  ").IsSuccess.Should().BeTrue();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankLastName_ShouldReturnLastNameRequired(string lastName)
        => AssertFails(CreateValid(lastName: lastName), InquiryError.LastNameRequired());

    [Fact]
    public void Create_WithLastNameAtMaxLength_ShouldSucceed()
        => CreateValid(lastName: new string('a', InquiryConstants.MaxNameLength)).IsSuccess.Should().BeTrue();

    [Fact]
    public void Create_WithLastNameOverMaxLength_ShouldReturnLastNameTooLong()
        => AssertFails(CreateValid(lastName: new string('a', InquiryConstants.MaxNameLength + 1)), InquiryError.LastNameTooLong());

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankEmail_ShouldReturnEmailRequired(string email)
        => AssertFails(CreateValid(email: email), InquiryError.EmailRequired());

    [Fact]
    public void Create_WithEmailOverMaxLength_ShouldReturnEmailTooLong()
    {
        var email = new string('a', InquiryConstants.MaxEmailLength - "@example.com".Length + 1) + "@example.com";

        AssertFails(CreateValid(email: email), InquiryError.EmailTooLong());
    }

    [Fact]
    public void Create_WithEmailAtMaxLength_ShouldPassLengthCheck()
    {
        var email = new string('a', 64) + "@" + new string('b', InquiryConstants.MaxEmailLength - 64 - 1 - ".com".Length) + ".com";

        var result = CreateValid(email: email);

        result.Errors.Should().NotContain(e => e.Code == "Inquiry.EmailTooLong");
    }

    [Theory]
    [InlineData("petar@example.com")]
    [InlineData("petar.petrovic+tag@mail.example.rs")]
    [InlineData("PETAR@EXAMPLE.COM")]
    [InlineData("a@b.c")]
    public void Create_WithValidEmail_ShouldSucceed(string email)
        => CreateValid(email: email).IsSuccess.Should().BeTrue();

    [Theory]
    [InlineData("petar")]
    [InlineData("petar@")]
    [InlineData("@example.com")]
    [InlineData("petar@localhost")]
    [InlineData("Petar <petar@example.com>")]
    [InlineData("petar@@example.com")]
    public void Create_WithInvalidEmail_ShouldReturnInvalidEmail(string email)
        => AssertFails(CreateValid(email: email), InquiryError.InvalidEmail());

    // Edge case (suspicious): MailAddress is lenient - these pass the domain check but are unlikely to be deliverable.
    [Theory]
    [InlineData("petar@example.com.")]
    [InlineData("petar@-example.com")]
    [InlineData("petar.@example.com")]
    [InlineData("petar@exa_mple.com")]
    [InlineData("\"petar\"@example.com")]
    [InlineData("petar@[127.0.0.1]")]
    public void Create_WithLooselyValidEmail_ShouldCurrentlyBeAccepted(string email)
        => CreateValid(email: email).IsSuccess.Should().BeTrue();

    [Theory]
    [InlineData("a..b@example.com")]
    [InlineData("petar@example..com")]
    [InlineData("petar@.example.com")]
    [InlineData("pe tar@example.com")]
    public void Create_WithMalformedDotsOrSpacesInEmail_ShouldReturnInvalidEmail(string email)
        => AssertFails(CreateValid(email: email), InquiryError.InvalidEmail());

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankPhoneNumber_ShouldReturnPhoneNumberRequired(string phoneNumber)
        => AssertFails(CreateValid(phoneNumber: phoneNumber), InquiryError.PhoneNumberRequired());

    [Theory]
    [InlineData("0601234567")]       // 10 digits - minimum
    [InlineData("+381601234567")]
    [InlineData("123456789012345")]  // 15 digits - maximum
    [InlineData("+123456789012345")]
    public void Create_WithValidPhoneNumber_ShouldSucceed(string phoneNumber)
        => CreateValid(phoneNumber: phoneNumber).IsSuccess.Should().BeTrue();

    [Theory]
    [InlineData("060123456")]         // 9 digits
    [InlineData("1234567890123456")]  // 16 digits
    [InlineData("+381 60 1234567")]
    [InlineData("060-123-4567")]
    [InlineData("++381601234567")]
    [InlineData("381+601234567")]
    [InlineData("abcdefghij")]
    public void Create_WithInvalidPhoneNumber_ShouldReturnInvalidPhoneNumber(string phoneNumber)
        => AssertFails(CreateValid(phoneNumber: phoneNumber), InquiryError.InvalidPhoneNumber());

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankMessage_ShouldReturnMessageRequired(string message)
        => AssertFails(CreateValid(message: message), InquiryError.MessageRequired());

    [Fact]
    public void Create_WithMessageAtMinLength_ShouldSucceed()
        => CreateValid(message: new string('a', InquiryConstants.MinMessageLength)).IsSuccess.Should().BeTrue();

    [Fact]
    public void Create_WithMessageUnderMinLength_ShouldReturnMessageTooShort()
        => AssertFails(CreateValid(message: new string('a', InquiryConstants.MinMessageLength - 1)), InquiryError.MessageTooShort());

    // Padding doesn't count toward the minimum - length is checked after trimming.
    [Fact]
    public void Create_WithShortMessagePaddedWithWhitespace_ShouldReturnMessageTooShort()
        => AssertFails(CreateValid(message: $"     {new string('a', InquiryConstants.MinMessageLength - 1)}     "), InquiryError.MessageTooShort());

    [Fact]
    public void Create_WithMessageAtMaxLength_ShouldSucceed()
        => CreateValid(message: new string('a', InquiryConstants.MaxMessageLength)).IsSuccess.Should().BeTrue();

    [Fact]
    public void Create_WithMessageOverMaxLength_ShouldReturnMessageTooLong()
        => AssertFails(CreateValid(message: new string('a', InquiryConstants.MaxMessageLength + 1)), InquiryError.MessageTooLong());

    [Theory]
    [InlineData(-1)]
    [InlineData(4)]
    [InlineData(99)]
    public void Create_WithUndefinedInquiryType_ShouldReturnInvalidInquiryType(int type)
        => AssertFails(CreateValid(inquiryType: (InquiryType)type), InquiryError.InvalidInquiryType());

    [Fact]
    public void Create_WithMultipleInvalidFields_ShouldReturnOnlyFirstErrorInValidationOrder()
    {
        var result = Inquiry.Create("", "", "not-an-email", "abc", "", (InquiryType)99);

        AssertFails(result, InquiryError.FirstNameRequired());
    }

    [Fact]
    public void Create_WithInvalidEmailAndPhone_ShouldReportEmailFirst()
        => AssertFails(CreateValid(email: "petar", phoneNumber: "abc"), InquiryError.InvalidEmail());

    [Fact]
    public void Resolve_WithValidAdmin_ShouldResolveWithUtcTimestamp()
    {
        var before = DateTime.UtcNow;

        var result = _sut.Resolve(AdminId);

        var after = DateTime.UtcNow;
        result.IsSuccess.Should().BeTrue();
        _sut.ResolvedByAdmin.Should().Be(AdminId);
        _sut.ResolvedAt.Should().NotBeNull();
        _sut.ResolvedAt!.Value.Kind.Should().Be(DateTimeKind.Utc);
        _sut.ResolvedAt.Value.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void Resolve_AfterPersistedCreatedAt_ShouldSucceed()
    {
        SetCreatedAt(_sut, DateTime.UtcNow.AddMinutes(-5));

        _sut.Resolve(AdminId).IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Resolve_WithBlankAdminId_ShouldReturnInvalidAdminIdAndStayUnresolved(string? adminId)
    {
        var result = _sut.Resolve(adminId!);

        AssertFails(result, InquiryError.InvalidAdminId());
        _sut.ResolvedAt.Should().BeNull();
        _sut.ResolvedByAdmin.Should().BeNull();
    }

    [Fact]
    public void Resolve_WhenAlreadyResolved_ShouldReturnAlreadyResolvedAndKeepOriginalResolution()
    {
        _sut.Resolve(AdminId);
        var firstResolvedAt = _sut.ResolvedAt;

        var result = _sut.Resolve("admin-2");

        AssertFails(result, InquiryError.AlreadyResolved(firstResolvedAt));
        _sut.ResolvedByAdmin.Should().Be(AdminId);
        _sut.ResolvedAt.Should().Be(firstResolvedAt);
    }

    [Fact]
    public void Resolve_WhenAlreadyResolvedBySameAdmin_ShouldStillFail()
    {
        _sut.Resolve(AdminId);

        _sut.Resolve(AdminId).IsSuccess.Should().BeFalse();
    }

    // Blank admin is checked before "already resolved", so a resolved inquiry reports InvalidAdminId.
    [Fact]
    public void Resolve_WhenAlreadyResolvedAndBlankAdmin_ShouldReportInvalidAdminIdFirst()
    {
        _sut.Resolve(AdminId);

        AssertFails(_sut.Resolve(""), InquiryError.InvalidAdminId());
    }

    // Clock skew guard: CreatedAt (set by the audit interceptor) in the future blocks resolution.
    [Fact]
    public void Resolve_WhenCreatedAtIsInTheFuture_ShouldReturnInvalidResolvedAtAndStayUnresolved()
    {
        SetCreatedAt(_sut, DateTime.UtcNow.AddMinutes(5));

        var result = _sut.Resolve(AdminId);

        AssertFails(result, InquiryError.InvalidResolvedAt());
        _sut.ResolvedAt.Should().BeNull();
        _sut.ResolvedByAdmin.Should().BeNull();
    }

    // Edge case: admin id is not trimmed, surrounding whitespace is stored as-is.
    [Fact]
    public void Resolve_WithPaddedAdminId_ShouldStoreItUntrimmed()
    {
        _sut.Resolve(" admin-1 ");

        _sut.ResolvedByAdmin.Should().Be(" admin-1 ");
    }

    [Theory]
    [InlineData(InquiryType.ClassPackage, "Paket časova")]
    [InlineData(InquiryType.IndividualClasses, "Individualni časovi")]
    [InlineData(InquiryType.Project, "Seminarski/Diplomski rad")]
    [InlineData(InquiryType.Other, "Ostalo")]
    public void MapInquiryTypeToLocalizedName_ShouldReturnSerbianName(InquiryType type, string expected)
    {
        var inquiry = CreateValid(inquiryType: type).Payload!;

        inquiry.MapInquiryTypeToLocalizedName().Should().Be(expected);
    }

    [Fact]
    public void AlreadyResolved_WithTimestamp_ShouldFormatAsUtc()
    {
        var error = InquiryError.AlreadyResolved(new DateTime(2026, 3, 10, 14, 5, 9, DateTimeKind.Utc));

        error.Description.Should().Be("Inquiry is already resolved at 2026-03-10 14:05:09 UTC");
        error.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public void AlreadyResolved_WithoutTimestamp_ShouldUseGenericMessage()
        => InquiryError.AlreadyResolved().Description.Should().Be("Inquiry is already resolved");

    private static API.Common.Results.Result<Inquiry> CreateValid(
        string firstName = FirstName,
        string lastName = LastName,
        string email = Email,
        string phoneNumber = PhoneNumber,
        string message = Message,
        InquiryType inquiryType = Type)
        => Inquiry.Create(firstName, lastName, email, phoneNumber, message, inquiryType);

    private static void AssertFails(API.Common.Results.Result result, Error expected)
    {
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be(expected);
    }

    // CreatedAt is set by AuditInterceptor on save; tests simulate a persisted value.
    private static void SetCreatedAt(Inquiry inquiry, DateTime createdAt)
        => typeof(Inquiry).GetProperty(nameof(Inquiry.CreatedAt))!.SetValue(inquiry, createdAt);
}
