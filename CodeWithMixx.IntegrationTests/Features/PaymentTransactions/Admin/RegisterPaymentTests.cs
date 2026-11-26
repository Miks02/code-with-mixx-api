using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using CodeWithMixx.API.Domain.Entities.PaymentTransactions;
using CodeWithMixx.API.Features.PaymentTransactions.Admin.RegisterPayment;
using CodeWithMixx.IntegrationTests.Common;
using CodeWithMixx.IntegrationTests.Features.Authenticatiion;
using CodeWithMixx.IntegrationTests.Features.PaymentTransactions;

namespace CodeWithMixx.IntegrationTests.Features.PaymentTransactions.Admin;

public class RegisterPaymentTests : IClassFixture<IntegrationTestFactory>, IAsyncLifetime
{
    private const string ValidNote = "Uplata za prvi termin casova";
    private readonly IntegrationTestFactory _factory;
    private HttpClient _httpClient;

    public RegisterPaymentTests(IntegrationTestFactory factory)
    {
        _factory = factory;
        _httpClient = _factory.CreateClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task RegisterPayment_WhenUserIsNotAuthenticated_ShouldReturnUnauthorized()
    {
        var response = await RegisterAsync(1, new { amount = 500m, transactionDate = DateTimeOffset.UtcNow.AddDays(-1) });

        response.Response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        response.Data.Should().BeNull();
    }

    [Fact]
    public async Task RegisterPayment_WhenUserIsStudent_ShouldReturnForbiddenAndNotPersist()
    {
        _httpClient = await _factory.CreateStudentClientAsync();
        var student = await _factory.GetOrCreateStudentAsync();
        var reservationId = await _factory.CreateReservationAsync(student.Id);

        var response = await RegisterAsync(reservationId, new { amount = 500m, transactionDate = DateTimeOffset.UtcNow.AddDays(-1) });

        response.Response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await _factory.GetTransactionsAsync(reservationId)).Should().BeEmpty();
    }

    [Fact]
    public async Task RegisterPayment_WhenReservationDoesNotExist_ShouldReturnNotFound()
    {
        _httpClient = await _factory.CreateAdminClientAsync();

        var response = await RegisterAsync(999, new { amount = 500m, transactionDate = DateTimeOffset.UtcNow.AddDays(-1) });

        response.Response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Data.Should().BeNull();
    }

    [Fact]
    public async Task RegisterPayment_WhenReservationIsArchived_ShouldReturnNotFoundAndNotPersist()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        var student = await _factory.GetOrCreateStudentAsync();
        var reservationId = await _factory.CreateReservationAsync(student.Id);
        await _factory.ArchiveReservationAsync(reservationId);

        var response = await RegisterAsync(reservationId, new { amount = 500m, transactionDate = DateTimeOffset.UtcNow.AddDays(-1) });

        response.Response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await _factory.GetTransactionsAsync(reservationId)).Should().BeEmpty();
    }

    [Fact]
    public async Task RegisterPayment_WhenAmountIsZero_ShouldReturnBadRequestAndNotPersist()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        var student = await _factory.GetOrCreateStudentAsync();
        var reservationId = await _factory.CreateReservationAsync(student.Id);

        var response = await RegisterAsync(reservationId, new { amount = 0m, transactionDate = DateTimeOffset.UtcNow.AddDays(-1) });

        response.Response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await _factory.GetTransactionsAsync(reservationId)).Should().BeEmpty();
    }

    [Fact]
    public async Task RegisterPayment_WhenTransactionDateIsInFuture_ShouldReturnBadRequestAndNotPersist()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        var student = await _factory.GetOrCreateStudentAsync();
        var reservationId = await _factory.CreateReservationAsync(student.Id);

        var response = await RegisterAsync(reservationId, new { amount = 500m, transactionDate = DateTimeOffset.UtcNow.AddDays(1) });

        response.Response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await _factory.GetTransactionsAsync(reservationId)).Should().BeEmpty();
    }

    [Fact]
    public async Task RegisterPayment_WhenNoteIsTooShort_ShouldReturnBadRequestAndNotPersist()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        var student = await _factory.GetOrCreateStudentAsync();
        var reservationId = await _factory.CreateReservationAsync(student.Id);

        var response = await RegisterAsync(reservationId, new { amount = 500m, transactionDate = DateTimeOffset.UtcNow.AddDays(-1), note = "Kratko" });

        response.Response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await _factory.GetTransactionsAsync(reservationId)).Should().BeEmpty();
    }

    [Fact]
    public async Task RegisterPayment_WhenRequestIsValid_ShouldPersistPaymentAndUpdatePaidAmount()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        var student = await _factory.GetOrCreateStudentAsync();
        var reservationId = await _factory.CreateReservationAsync(student.Id);
        await _factory.RegisterPaymentAsync(reservationId, 200m, DateTime.UtcNow.AddDays(-2));

        var response = await RegisterAsync(reservationId, new { amount = 500m, transactionDate = DateTimeOffset.UtcNow.AddDays(-1), note = ValidNote });

        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data.ReservationId.Should().Be(reservationId);
        response.Data.Amount.Should().Be(500m);
        response.Data.PaymentType.Should().Be(PaymentTransactionType.Payment);

        var reservation = await _factory.FindReservationAsync(reservationId);
        reservation.PaidAmount.Should().Be(700m);

        var transaction = (await _factory.GetTransactionsAsync(reservationId)).Single(t => t.Id == response.Data.Id);
        transaction.Amount.Value.Should().Be(500m);
        transaction.PaymentType.Should().Be(PaymentTransactionType.Payment);
        transaction.TransactionNote.Note.Should().Be(ValidNote);
    }

    private async Task<EndpointResponse<RegisterPaymentResponse>> RegisterAsync(int reservationId, object body)
    {
        var response = await _httpClient.PostAsJsonAsync($"/api/admin/reservations/{reservationId}/payments/register", body);
        return await response.ToEndpointResponse<RegisterPaymentResponse>();
    }
}
