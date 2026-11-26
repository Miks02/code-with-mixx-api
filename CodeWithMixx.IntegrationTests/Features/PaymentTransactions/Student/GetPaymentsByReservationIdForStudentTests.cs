using System.Net;
using AwesomeAssertions;
using CodeWithMixx.API.Domain.Entities.PaymentTransactions;
using CodeWithMixx.API.Features.PaymentTransactions.Student.GetPaymentsByReservationIdForStudent;
using CodeWithMixx.IntegrationTests.Common;
using CodeWithMixx.IntegrationTests.Features.Authenticatiion;
using CodeWithMixx.IntegrationTests.Features.PaymentTransactions;

namespace CodeWithMixx.IntegrationTests.Features.PaymentTransactions.Student;

public class GetPaymentsByReservationIdForStudentTests : IClassFixture<IntegrationTestFactory>, IAsyncLifetime
{
    private readonly IntegrationTestFactory _factory;
    private HttpClient _httpClient;

    public GetPaymentsByReservationIdForStudentTests(IntegrationTestFactory factory)
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
    public async Task GetPayments_WhenUserIsAdmin_ShouldReturnForbidden()
    {
        _httpClient = await _factory.CreateAdminClientAsync();

        var response = await GetPaymentsAsync(1);

        response.Response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        response.Data.Should().BeNull();
    }

    [Fact]
    public async Task GetPayments_WhenReservationDoesNotExist_ShouldReturnNotFound()
    {
        _httpClient = await _factory.CreateStudentClientAsync();

        var response = await GetPaymentsAsync(999);

        response.Response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Data.Should().BeNull();
    }

    [Fact]
    public async Task GetPayments_WhenReservationBelongsToOtherStudent_ShouldReturnNotFound()
    {
        _httpClient = await _factory.CreateStudentClientAsync();
        var otherStudentId = await _factory.CreateOtherStudentAsync();
        var reservationId = await _factory.CreateReservationAsync(otherStudentId);
        await _factory.RegisterPaymentAsync(reservationId, 500m, DateTime.UtcNow.AddDays(-1));

        var response = await GetPaymentsAsync(reservationId);

        response.Response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Data.Should().BeNull();
    }

    [Fact]
    public async Task GetPayments_WhenReservationBelongsToCaller_ShouldReturnPaymentsAndSums()
    {
        _httpClient = await _factory.CreateStudentClientAsync();
        var student = await _factory.GetOrCreateStudentAsync();
        var reservationId = await _factory.CreateReservationAsync(student.Id);
        var older = await _factory.RegisterPaymentAsync(reservationId, 500m, DateTime.UtcNow.AddDays(-2));
        var newer = await _factory.SubtractPaymentAsync(reservationId, 100m, PaymentTransactionType.Refund, DateTime.UtcNow.AddDays(-1));
        var reservation = await _factory.FindReservationAsync(reservationId);

        var response = await GetPaymentsAsync(reservationId);

        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data.ReservationId.Should().Be(reservationId);
        response.Data.Payments.Select(p => p.Id).Should().Equal(newer, older);
        response.Data.PaidSum.Should().Be(500m);
        response.Data.TotalSum.Should().Be(400m);
        response.Data.RemainingAmount.Should().Be(reservation.RemainingAmount);
    }

    private async Task<EndpointResponse<GetPaymentsByReservationIdForStudentResponse>> GetPaymentsAsync(int reservationId)
    {
        var response = await _httpClient.GetAsync($"/api/student/reservations/{reservationId}/payments");
        return await response.ToEndpointResponse<GetPaymentsByReservationIdForStudentResponse>();
    }
}
