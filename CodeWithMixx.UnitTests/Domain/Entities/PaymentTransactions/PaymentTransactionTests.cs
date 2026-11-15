using AwesomeAssertions;
using CodeWithMixx.API.Common.Result;
using CodeWithMixx.API.Domain.Entities.PaymentTransactions;
using CodeWithMixx.API.Domain.Entities.PaymentTransactions.TransactionNotes;
using CodeWithMixx.API.Domain.ValueObjects.Monies;
using Xunit;

namespace CodeWithMixx.UnitTests.Domain.Entities.PaymentTransactions;

public class PaymentTransactionTests
{
    private const decimal AmountValue = 1500m;
    private const int ReservationId = 7;
    private const string Note = "Vraćen novac za otkazan čas.";

    private static readonly Money Amount = Money.Create(AmountValue).Payload!;
    private static readonly DateTime TransactionDate = DateTime.UtcNow.AddDays(-1);

    private readonly PaymentTransaction _sut;

    public PaymentTransactionTests()
    {
        _sut = CreateValid().Payload!;
    }

    [Fact]
    public void Create_WithValidPayment_ShouldSucceedAndMapValues()
    {
        var result = CreateValid(note: Note);

        result.IsSuccess.Should().BeTrue();
        var transaction = result.Payload!;
        transaction.Amount.Should().Be(Amount);
        transaction.PaymentType.Should().Be(PaymentTransactionType.Payment);
        transaction.TransactionDate.Should().Be(TransactionDate);
        transaction.ReservationId.Should().Be(ReservationId);
        transaction.TransactionNote.Note.Should().Be(Note);
    }

    [Fact]
    public void Create_ShouldLeaveIdAndAuditFieldsUnset()
    {
        _sut.Id.Should().Be(0);
        _sut.CreatedAt.Should().Be(default);
        _sut.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void Create_PaymentWithoutNote_ShouldSucceedWithEmptyNote()
    {
        _sut.TransactionNote.Should().Be(TransactionNote.Empty);
        _sut.TransactionNote.Note.Should().BeNull();
    }

    [Theory]
    [InlineData(PaymentTransactionType.Refund)]
    [InlineData(PaymentTransactionType.Reversal)]
    public void Create_SubtractingTypeWithNote_ShouldSucceed(PaymentTransactionType type)
    {
        var result = CreateValid(paymentType: type, note: Note);

        result.IsSuccess.Should().BeTrue();
        result.Payload!.PaymentType.Should().Be(type);
        result.Payload.TransactionNote.Note.Should().Be(Note);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_RefundWithoutNote_ShouldReturnNoteRequiredForRefund(string? note)
        => AssertFails(CreateValid(paymentType: PaymentTransactionType.Refund, note: note), TransactionNoteError.NoteIsRequiredForRefund());

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ReversalWithoutNote_ShouldReturnNoteRequiredForReversal(string? note)
        => AssertFails(CreateValid(paymentType: PaymentTransactionType.Reversal, note: note), TransactionNoteError.NoteIsRequiredForReversal());

    [Fact]
    public void Create_WithZeroAmount_ShouldReturnCannotBeZero()
        => AssertFails(CreateValid(amount: Money.Zero), PaymentTransactionError.CannotBeZero());

    [Fact]
    public void Create_WithAmountRoundedToZero_ShouldReturnCannotBeZero()
        => AssertFails(CreateValid(amount: Money.Create(0.004m).Payload!), PaymentTransactionError.CannotBeZero());

    [Fact]
    public void Create_WithSmallestPositiveAmount_ShouldSucceed()
        => CreateValid(amount: Money.Create(0.01m).Payload!).IsSuccess.Should().BeTrue();

    [Fact]
    public void Create_WithTransactionDateInTheFuture_ShouldReturnCannotBeInTheFuture()
        => AssertFails(CreateValid(transactionDate: DateTime.UtcNow.AddMinutes(5)), PaymentTransactionError.PaymentDateCannotBeInTheFuture());

    [Fact]
    public void Create_WithTransactionDateNow_ShouldSucceed()
        => CreateValid(transactionDate: DateTime.UtcNow).IsSuccess.Should().BeTrue();

    [Fact]
    public void Create_WithTransactionDateFarInThePast_ShouldSucceed()
        => CreateValid(transactionDate: new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)).IsSuccess.Should().BeTrue();

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void Create_WithNonPositiveReservationId_ShouldReturnInvalidReservation(int reservationId)
        => AssertFails(CreateValid(reservationId: reservationId), PaymentTransactionError.InvalidReservation(reservationId));

    [Fact]
    public void Create_WithMultipleInvalidValues_ShouldReportZeroAmountFirst()
    {
        var result = PaymentTransaction.Create(Money.Zero, PaymentTransactionType.Refund, DateTime.UtcNow.AddDays(1), 0);

        AssertFails(result, PaymentTransactionError.CannotBeZero());
    }

    [Fact]
    public void Create_WithFutureDateAndInvalidReservation_ShouldReportDateFirst()
        => AssertFails(CreateValid(transactionDate: DateTime.UtcNow.AddDays(1), reservationId: 0), PaymentTransactionError.PaymentDateCannotBeInTheFuture());

    [Fact]
    public void Create_WithInvalidReservationAndMissingRefundNote_ShouldReportReservationFirst()
        => AssertFails(CreateValid(paymentType: PaymentTransactionType.Refund, reservationId: 0), PaymentTransactionError.InvalidReservation(0));

    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    public void Create_WithUndefinedPaymentType_ShouldCurrentlyBeAccepted(int type)
        => CreateValid(paymentType: (PaymentTransactionType)type).IsSuccess.Should().BeTrue();

    [Fact]
    public void Create_WithNoteAtMaxLength_ShouldSucceed()
        => CreateValid(note: new string('a', TransactionNoteConstants.MaxNoteLength)).IsSuccess.Should().BeTrue();

    [Theory]
    [InlineData(PaymentTransactionType.Payment)]
    [InlineData(PaymentTransactionType.Refund)]
    [InlineData(PaymentTransactionType.Reversal)]
    public void Create_WithNoteOverMaxLength_ShouldReturnNoteTooLong(PaymentTransactionType type)
        => AssertFails(CreateValid(paymentType: type, note: new string('a', TransactionNoteConstants.MaxNoteLength + 1)), TransactionNoteError.NoteTooLong());

    [Fact]
    public void Create_WithPaddedNote_ShouldCurrentlyStoreItUntrimmed()
        => CreateValid(note: $"  {Note}  ").Payload!.TransactionNote.Note.Should().Be($"  {Note}  ");

    private static API.Common.Results.Result<PaymentTransaction> CreateValid(
        Money? amount = null,
        PaymentTransactionType paymentType = PaymentTransactionType.Payment,
        DateTime? transactionDate = null,
        int reservationId = ReservationId,
        string? note = null)
        => PaymentTransaction.Create(amount ?? Amount, paymentType, transactionDate ?? TransactionDate, reservationId, note);

    private static void AssertFails(API.Common.Results.Result result, Error expected)
    {
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be(expected);
    }
}
