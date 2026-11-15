using AwesomeAssertions;
using CodeWithMixx.API.Common.Result;
using CodeWithMixx.API.Domain.Entities.PaymentTransactions;
using Xunit;

namespace CodeWithMixx.UnitTests.Domain.Entities.PaymentTransactions;

public class PaymentTransactionErrorTests
{
    [Fact]
    public void PaymentDateCannotBeInTheFuture_ShouldBeValidationError()
    {
        var error = PaymentTransactionError.PaymentDateCannotBeInTheFuture();

        error.Code.Should().Be("PaymentTransaction.CannotBeInTheFuture");
        error.Description.Should().Be("Transaction date cannot be in the future");
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void CannotBeZero_ShouldBeValidationError()
    {
        var error = PaymentTransactionError.CannotBeZero();

        error.Code.Should().Be("PaymentTransaction.CannotBeZero");
        error.Description.Should().Be("Amount cannot be zero");
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void InvalidReservation_ShouldIncludeIdentifierInMessage()
    {
        var error = PaymentTransactionError.InvalidReservation(-3);

        error.Code.Should().Be("PaymentTransaction.InvalidReservation");
        error.Description.Should().Be("Reservation with identifier -3 is invalid");
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Theory]
    [InlineData(PaymentTransactionType.Payment, "Payment")]
    [InlineData(PaymentTransactionType.Refund, "Refund")]
    public void InvalidPaymentType_ShouldIncludeTypeNameInMessage(PaymentTransactionType type, string expectedName)
    {
        var error = PaymentTransactionError.InvalidPaymentType(type);

        error.Code.Should().Be("PaymentTransaction.InvalidPaymentType");
        error.Description.Should().Be($"Payment type {expectedName} is invalid for a given transaction.");
        error.Type.Should().Be(ErrorType.Validation);
    }
}
