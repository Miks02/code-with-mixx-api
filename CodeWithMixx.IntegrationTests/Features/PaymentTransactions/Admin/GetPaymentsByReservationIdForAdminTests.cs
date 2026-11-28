using System.Net;
using AwesomeAssertions;
using CodeWithMixx.API.Domain.Entities.PaymentTransactions;
using CodeWithMixx.API.Features.PaymentTransactions.Admin.GetPaymentsByReservationIdForAdmin;
using CodeWithMixx.IntegrationTests.Common;
using CodeWithMixx.IntegrationTests.Features.Authenticatiion;
using CodeWithMixx.IntegrationTests.Features.PaymentTransactions;

namespace CodeWithMixx.IntegrationTests.Features.PaymentTransactions.Admin;

public class GetPaymentsByReservationIdForAdminTests : IClassFixture<IntegrationTestFactory>, IAsyncLifetime
{
    private readonly IntegrationTestFactory _factory;
    private HttpClient _httpClient;

    public GetPaymentsByReservationIdForAdminTests(IntegrationTestFactory factory)
    {
        _factory = factory;
        _httpClient = _factory.CreateClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetPayments_WhenUserIsNotAuthenticated_ShouldReturnUnauthorized()
    {
        var response = await GetPaymentsAsync(1);

        response.Response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        response.Data.Should().BeNull();
    }
    
    [Fact]
    public async Task GetPayments_WhenUserIsNotAdmin_ShouldReturnForbidden()
    {
        _httpClient = await _factory.CreateStudentClientAsync();

        var response = await GetPaymentsAsync(1);

        response.Response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        response.Data.Should().BeNull();
    }

    [Fact]
    public async Task GetPayments_WhenReservationDoesNotExist_ShouldReturnNotFound()
    {
        _httpClient = await _factory.CreateAdminClientAsync();

        var response = await GetPaymentsAsync(999);

        response.Response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Data.Should().BeNull();
    }

    [Fact]
    public async Task GetPayments_WhenReservationHasTransactions_ShouldReturnPaymentsAndSums()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        var student = await _factory.GetOrCreateStudentAsync();
        var reservationId = await _factory.CreateReservationAsync(student.Id);
        var oldest = await _factory.RegisterPaymentAsync(reservationId, 500m, DateTime.UtcNow.AddDays(-3));
        var middle = await _factory.RegisterPaymentAsync(reservationId, 300m, DateTime.UtcNow.AddDays(-2));
        var newest = await _factory.SubtractPaymentAsync(reservationId, 200m, PaymentTransactionType.Reversal, DateTime.UtcNow.AddDays(-1));
        var reservation = await _factory.FindReservationAsync(reservationId);

        var response = await GetPaymentsAsync(reservationId);

        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data.ReservationId.Should().Be(reservationId);
        response.Data.Payments.Select(p => p.Id).Should().Equal(newest, middle, oldest);
        response.Data.PaidSum.Should().Be(800m);
        response.Data.ReversedSum.Should().Be(200m);
        response.Data.TotalSum.Should().Be(600m);
        response.Data.RemainingAmount.Should().Be(reservation.RemainingAmount);
    }

    [Fact]
    public async Task GetPayments_WhenReservationIsArchived_ShouldStillReturnPayments()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        var student = await _factory.GetOrCreateStudentAsync();
        var reservationId = await _factory.CreateReservationAsync(student.Id);
        var paymentId = await _factory.RegisterPaymentAsync(reservationId, 500m, DateTime.UtcNow.AddDays(-1));
        await _factory.ArchiveReservationAsync(reservationId);

        var response = await GetPaymentsAsync(reservationId);

        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data.Payments.Should().ContainSingle()
            .Which.Id.Should().Be(paymentId);
    }

    private async Task<EndpointResponse<GetPaymentsByReservationIdForAdminResponse>> GetPaymentsAsync(int reservationId)
    {
        var response = await _httpClient.GetAsync($"/api/reservations/{reservationId}/payments");
        return await response.ToEndpointResponse<GetPaymentsByReservationIdForAdminResponse>();
    }
}
