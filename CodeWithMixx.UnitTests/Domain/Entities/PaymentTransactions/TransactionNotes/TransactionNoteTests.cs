using AwesomeAssertions;
using CodeWithMixx.API.Common.Result;
using CodeWithMixx.API.Domain.Entities.PaymentTransactions;
using CodeWithMixx.API.Domain.Entities.PaymentTransactions.TransactionNotes;
using Xunit;

namespace CodeWithMixx.UnitTests.Domain.Entities.PaymentTransactions.TransactionNotes;

public class TransactionNoteTests
{
    private const string Note = "Vraćen novac za otkazan čas.";

    [Fact]
    public void Empty_ShouldHaveNullNote()
        => TransactionNote.Empty.Note.Should().BeNull();

    [Fact]
    public void Empty_ShouldBeEqualByValue()
        => TransactionNote.Empty.Should().Be(TransactionNote.Empty);

    [Theory]
    [InlineData(PaymentTransactionType.Payment)]
    [InlineData(PaymentTransactionType.Refund)]
    [InlineData(PaymentTransactionType.Reversal)]
    public void Create_WithNote_ShouldSucceedForEveryType(PaymentTransactionType type)
    {
        var result = TransactionNote.Create(Note, type);

        result.IsSuccess.Should().BeTrue();
        result.Payload!.Note.Should().Be(Note);
    }

    [Fact]
    public void Create_PaymentWithNullNote_ShouldEqualEmpty()
    {
        var result = TransactionNote.Create(null, PaymentTransactionType.Payment);

        result.IsSuccess.Should().BeTrue();
        result.Payload.Should().Be(TransactionNote.Empty);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_PaymentWithBlankNote_ShouldCurrentlyStoreItAsIs(string note)
    {
        var result = TransactionNote.Create(note, PaymentTransactionType.Payment);

        result.IsSuccess.Should().BeTrue();
        result.Payload!.Note.Should().Be(note);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public void Create_RefundWithoutNote_ShouldReturnNoteRequiredForRefund(string? note)
        => AssertFails(TransactionNote.Create(note, PaymentTransactionType.Refund), TransactionNoteError.NoteIsRequiredForRefund());

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public void Create_ReversalWithoutNote_ShouldReturnNoteRequiredForReversal(string? note)
        => AssertFails(TransactionNote.Create(note, PaymentTransactionType.Reversal), TransactionNoteError.NoteIsRequiredForReversal());

    [Theory]
    [InlineData(PaymentTransactionType.Payment)]
    [InlineData(PaymentTransactionType.Refund)]
    [InlineData(PaymentTransactionType.Reversal)]
    public void Create_WithNoteAtMaxLength_ShouldSucceed(PaymentTransactionType type)
    {
        var note = new string('a', TransactionNoteConstants.MaxNoteLength);

        var result = TransactionNote.Create(note, type);

        result.IsSuccess.Should().BeTrue();
        result.Payload!.Note.Should().Be(note);
    }

    [Theory]
    [InlineData(PaymentTransactionType.Payment)]
    [InlineData(PaymentTransactionType.Refund)]
    [InlineData(PaymentTransactionType.Reversal)]
    public void Create_WithNoteOverMaxLength_ShouldReturnNoteTooLong(PaymentTransactionType type)
        => AssertFails(TransactionNote.Create(new string('a', TransactionNoteConstants.MaxNoteLength + 1), type), TransactionNoteError.NoteTooLong());

    [Fact]
    public void Create_WithPaddingThatPushesNoteOverMaxLength_ShouldReturnNoteTooLong()
        => AssertFails(TransactionNote.Create($" {new string('a', TransactionNoteConstants.MaxNoteLength)}", PaymentTransactionType.Payment), TransactionNoteError.NoteTooLong());

    [Fact]
    public void Create_WithSameNote_ShouldBeEqualByValue()
    {
        var first = TransactionNote.Create(Note, PaymentTransactionType.Payment).Payload!;
        var second = TransactionNote.Create(Note, PaymentTransactionType.Refund).Payload!;

        first.Should().Be(second);
    }

    [Fact]
    public void Create_WithDifferentNotes_ShouldNotBeEqual()
    {
        var first = TransactionNote.Create(Note, PaymentTransactionType.Payment).Payload!;
        var second = TransactionNote.Create("Druga napomena", PaymentTransactionType.Payment).Payload!;

        first.Should().NotBe(second);
    }

    [Fact]
    public void NoteIsRequiredForRefund_ShouldBeValidationError()
    {
        var error = TransactionNoteError.NoteIsRequiredForRefund();

        error.Code.Should().Be("TransactionNote.RequiredForRefund");
        error.Description.Should().Be("Note is required for refunded transaction.");
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void NoteIsRequiredForReversal_ShouldBeValidationError()
    {
        var error = TransactionNoteError.NoteIsRequiredForReversal();

        error.Code.Should().Be("TransactionNote.RequiredForReversal");
        error.Description.Should().Be("Note is required for reversed transaction.");
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void NoteTooLong_ShouldIncludeMaxLengthInMessage()
    {
        var error = TransactionNoteError.NoteTooLong();

        error.Code.Should().Be("TransactionNote.TooLong");
        error.Description.Should().Be($"Note cannot exceed {TransactionNoteConstants.MaxNoteLength} characters.");
        error.Type.Should().Be(ErrorType.Validation);
    }

    private static void AssertFails(API.Common.Results.Result result, Error expected)
    {
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be(expected);
    }
}
