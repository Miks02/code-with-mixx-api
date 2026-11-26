using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using CodeWithMixx.API.Domain.Entities.PaymentTransactions;
using CodeWithMixx.API.Features.PaymentTransactions.Admin.SubtractPayment;
using CodeWithMixx.IntegrationTests.Common;
using CodeWithMixx.IntegrationTests.Features.Authenticatiion;
using CodeWithMixx.IntegrationTests.Features.PaymentTransactions;

namespace CodeWithMixx.IntegrationTests.Features.PaymentTransactions.Admin;

public class SubtractPaymentTests : IClassFixture<IntegrationTestFactory>, IAsyncLifetime
{
    private const string ValidNote = "Povracaj novca zbog otkazanog casa";
    private readonly IntegrationTestFactory _factory;
    private HttpClient _httpClient;

    public SubtractPaymentTests(IntegrationTestFactory factory)
    {
        _factory = factory;
        _httpClient = _factory.CreateClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task SubtractPayment_WhenUserIsNotAuthenticated_ShouldReturnUnauthorized()
    {
        var response = await SubtractAsync(1, ValidBody(100m, "Refund"));

        response.Response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        response.Data.Should().BeNull();
    }

    [Fact]
    public async Task SubtractPayment_WhenUserIsStudent_ShouldReturnForbiddenAndNotPersist()
    {
        _httpClient = await _factory.CreateStudentClientAsync();
        var student = await _factory.GetOrCreateStudentAsync();
        var reservationId = await _factory.CreateReservationAsync(student.Id);
        await _factory.RegisterPaymentAsync(reservationId, 500m, DateTime.UtcNow.AddDays(-2));

        var response = await SubtractAsync(reservationId, ValidBody(100m, "Refund"));

        response.Response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await _factory.GetTransactionsAsync(reservationId)).Should().ContainSingle();
    }

    [Fact]
    public async Task SubtractPayment_WhenReservationDoesNotExist_ShouldReturnNotFound()
    {
        _httpClient = await _factory.CreateAdminClientAsync();

        var response = await SubtractAsync(999, ValidBody(100m, "Refund"));

        response.Response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Data.Should().BeNull();
    }

    [Fact]
    public async Task SubtractPayment_WhenNoteIsMissing_ShouldReturnBadRequestAndNotPersist()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        var student = await _factory.GetOrCreateStudentAsync();
        var reservationId = await _factory.CreateReservationAsync(student.Id);
        await _factory.RegisterPaymentAsync(reservationId, 500m, DateTime.UtcNow.AddDays(-2));

        var response = await SubtractAsync(reservationId, new { amount = 100m, transactionDate = DateTimeOffset.UtcNow.AddDays(-1), paymentType = "Refund" });

        response.Response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await _factory.GetTransactionsAsync(reservationId)).Should().ContainSingle();
    }

    [Fact]
    public async Task SubtractPayment_WhenPaymentTypeIsPayment_ShouldReturnBadRequestAndNotPersist()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        var student = await _factory.GetOrCreateStudentAsync();
        var reservationId = await _factory.CreateReservationAsync(student.Id);
        await _factory.RegisterPaymentAsync(reservationId, 500m, DateTime.UtcNow.AddDays(-2));

        var response = await SubtractAsync(reservationId, ValidBody(100m, "Payment"));

        response.Response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await _factory.GetTransactionsAsync(reservationId)).Should().ContainSingle();
        (await _factory.FindReservationAsync(reservationId)).PaidAmount.Should().Be(500m);
    }

    [Fact]
    public async Task SubtractPayment_WhenPaymentTypeIsOutOfRange_ShouldReturnBadRequestAndNotPersist()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        var student = await _factory.GetOrCreateStudentAsync();
        var reservationId = await _factory.CreateReservationAsync(student.Id);
        await _factory.RegisterPaymentAsync(reservationId, 500m, DateTime.UtcNow.AddDays(-2));

        var response = await SubtractAsync(reservationId, new { amount = 100m, transactionDate = DateTimeOffset.UtcNow.AddDays(-1), note = ValidNote, paymentType = 7 });

        response.Response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await _factory.GetTransactionsAsync(reservationId)).Should().ContainSingle();
        (await _factory.FindReservationAsync(reservationId)).PaidAmount.Should().Be(500m);
    }

    [Fact]
    public async Task SubtractPayment_WhenAmountExceedsPaidAmount_ShouldReturnBadRequestAndNotPersist()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        var student = await _factory.GetOrCreateStudentAsync();
        var reservationId = await _factory.CreateReservationAsync(student.Id);
        await _factory.RegisterPaymentAsync(reservationId, 500m, DateTime.UtcNow.AddDays(-2));

        var response = await SubtractAsync(reservationId, ValidBody(600m, "Refund"));

        response.Response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await _factory.GetTransactionsAsync(reservationId)).Should().ContainSingle();
        (await _factory.FindReservationAsync(reservationId)).PaidAmount.Should().Be(500m);
    }

    [Fact]
    public async Task SubtractPayment_WhenAmountIsZero_ShouldReturnBadRequestAndNotPersist()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        var student = await _factory.GetOrCreateStudentAsync();
        var reservationId = await _factory.CreateReservationAsync(student.Id);
        await _factory.RegisterPaymentAsync(reservationId, 500m, DateTime.UtcNow.AddDays(-2));

        var response = await SubtractAsync(reservationId, ValidBody(0m, "Refund"));

        response.Response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await _factory.GetTransactionsAsync(reservationId)).Should().ContainSingle();
    }

    [Theory]
    [InlineData("Refund", PaymentTransactionType.Refund)]
    [InlineData("Reversal", PaymentTransactionType.Reversal)]
    public async Task SubtractPayment_WhenRequestIsValid_ShouldPersistTransactionAndDecreasePaidAmount(string paymentType, PaymentTransactionType expectedType)
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        var student = await _factory.GetOrCreateStudentAsync();
        var reservationId = await _factory.CreateReservationAsync(student.Id);
        await _factory.RegisterPaymentAsync(reservationId, 500m, DateTime.UtcNow.AddDays(-2));

        var response = await SubtractAsync(reservationId, ValidBody(150m, paymentType));

        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data.ReservationId.Should().Be(reservationId);
        response.Data.Amount.Should().Be(150m);
        response.Data.PaymentType.Should().Be(expectedType);

        var reservation = await _factory.FindReservationAsync(reservationId);
        reservation.PaidAmount.Should().Be(350m);

        var transaction = (await _factory.GetTransactionsAsync(reservationId)).Single(t => t.Id == response.Data.Id);
        transaction.Amount.Value.Should().Be(150m);
        transaction.PaymentType.Should().Be(expectedType);
        transaction.TransactionNote.Note.Should().Be(ValidNote);
    }

    private static object ValidBody(decimal amount, string paymentType) => new
    {
        amount,
        transactionDate = DateTimeOffset.UtcNow.AddDays(-1),
        note = ValidNote,
        paymentType
    };

    private async Task<EndpointResponse<SubtractPaymentResponse>> SubtractAsync(int reservationId, object body)
    {
        var response = await _httpClient.PostAsJsonAsync($"/api/admin/reservations/{reservationId}/payments/subtract", body);
        return await response.ToEndpointResponse<SubtractPaymentResponse>();
    }
}
