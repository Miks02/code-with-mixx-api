using System.Net;
using AwesomeAssertions;
using CodeWithMixx.API.Domain.Entities.PaymentTransactions;
using CodeWithMixx.API.Features.PaymentTransactions.Student.GetPaymentsSummaryForStudent;
using CodeWithMixx.IntegrationTests.Common;
using CodeWithMixx.IntegrationTests.Features.Authenticatiion;
using CodeWithMixx.IntegrationTests.Features.PaymentTransactions;

namespace CodeWithMixx.IntegrationTests.Features.PaymentTransactions.Student;

public class GetPaymentsSummaryForStudentTests : IClassFixture<IntegrationTestFactory>, IAsyncLifetime
{
    private const string Url = "/api/student/payments/summary";
    private readonly IntegrationTestFactory _factory;
    private HttpClient _httpClient;

    public GetPaymentsSummaryForStudentTests(IntegrationTestFactory factory)
    {
        _factory = factory;
        _httpClient = _factory.CreateClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetSummary_WhenUserIsNotAuthenticated_ShouldReturnUnauthorized()
    {
        var response = await GetSummaryAsync("pageNumber=1&pageSize=10");

        response.Response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        response.Data.Should().BeNull();
    }

    [Fact]
    public async Task GetSummary_WhenUserIsAdmin_ShouldReturnForbidden()
    {
        _httpClient = await _factory.CreateAdminClientAsync();

        var response = await GetSummaryAsync("pageNumber=1&pageSize=10");

        response.Response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        response.Data.Should().BeNull();
    }

    [Fact]
    public async Task GetSummary_WhenCallerHasNoTransactions_ShouldReturnZeroStats()
    {
        _httpClient = await _factory.CreateStudentClientAsync();
        var otherStudentId = await _factory.CreateOtherStudentAsync();
        var otherReservationId = await _factory.CreateReservationAsync(otherStudentId);
        await _factory.RegisterPaymentAsync(otherReservationId, 400m, DateTime.UtcNow.AddDays(-1));

        var response = await GetSummaryAsync("pageNumber=1&pageSize=10");

        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data.PagedPayments.Items.Should().BeEmpty();
        response.Data.TotalTransactions.Should().Be(0);
        response.Data.PaymentsCount.Should().Be(0);
        response.Data.TotalDebt.Should().Be(0);
        response.Data.DebtsCount.Should().Be(0);
    }

    [Fact]
    public async Task GetSummary_WhenOtherStudentsHaveTransactions_ShouldReturnOnlyCallerStats()
    {
        _httpClient = await _factory.CreateStudentClientAsync();
        var student = await _factory.GetOrCreateStudentAsync();
        var otherStudentId = await _factory.CreateOtherStudentAsync();
        var reservationId = await _factory.CreateReservationAsync(student.Id);
        var otherReservationId = await _factory.CreateReservationAsync(otherStudentId);
        await _factory.RegisterPaymentAsync(reservationId, 800m, DateTime.UtcNow.AddDays(-3));
        await _factory.SubtractPaymentAsync(reservationId, 100m, PaymentTransactionType.Refund, DateTime.UtcNow.AddDays(-2));
        await _factory.RegisterPaymentAsync(otherReservationId, 400m, DateTime.UtcNow.AddDays(-3));
        await _factory.SubtractPaymentAsync(otherReservationId, 50m, PaymentTransactionType.Reversal, DateTime.UtcNow.AddDays(-1));
        var reservation = await _factory.FindReservationAsync(reservationId);

        var response = await GetSummaryAsync("pageNumber=1&pageSize=10");

        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data.PagedPayments.Items.Should().HaveCount(2);
        response.Data.PagedPayments.Items.Should().OnlyContain(p => p.ReservationId == reservationId);
        response.Data.TotalTransactions.Should().Be(2);
        response.Data.PaymentsCount.Should().Be(1);
        response.Data.RefundsCount.Should().Be(1);
        response.Data.ReversalsCount.Should().Be(0);
        response.Data.TotalDebt.Should().Be(reservation.RemainingAmount);
        response.Data.DebtsCount.Should().Be(1);
    }

    private async Task<EndpointResponse<GetPaymentsSummaryForStudentResponse>> GetSummaryAsync(string query)
    {
        var response = await _httpClient.GetAsync($"{Url}?{query}");
        return await response.ToEndpointResponse<GetPaymentsSummaryForStudentResponse>();
    }
}
