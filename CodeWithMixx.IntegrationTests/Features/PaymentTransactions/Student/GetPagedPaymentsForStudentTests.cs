using System.Net;
using AwesomeAssertions;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.PaymentTransactions;
using CodeWithMixx.API.Features.PaymentTransactions.Common;
using CodeWithMixx.IntegrationTests.Common;
using CodeWithMixx.IntegrationTests.Features.Authenticatiion;
using CodeWithMixx.IntegrationTests.Features.PaymentTransactions;

namespace CodeWithMixx.IntegrationTests.Features.PaymentTransactions.Student;

public class GetPagedPaymentsForStudentTests : IClassFixture<IntegrationTestFactory>, IAsyncLifetime
{
    private const string Url = "/api/student/payments";
    private readonly IntegrationTestFactory _factory;
    private HttpClient _httpClient;

    public GetPagedPaymentsForStudentTests(IntegrationTestFactory factory)
    {
        _factory = factory;
        _httpClient = _factory.CreateClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetPaged_WhenUserIsNotAuthenticated_ShouldReturnUnauthorized()
    {
        var response = await GetPagedAsync("pageNumber=1&pageSize=10");

        response.Response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        response.Data.Should().BeNull();
    }

    [Fact]
    public async Task GetPaged_WhenUserIsAdmin_ShouldReturnForbidden()
    {
        _httpClient = await _factory.CreateAdminClientAsync();

        var response = await GetPagedAsync("pageNumber=1&pageSize=10");

        response.Response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        response.Data.Should().BeNull();
    }

    [Fact]
    public async Task GetPaged_WhenOtherStudentsHavePayments_ShouldReturnOnlyCallerPayments()
    {
        _httpClient = await _factory.CreateStudentClientAsync();
        var student = await _factory.GetOrCreateStudentAsync();
        var otherStudentId = await _factory.CreateOtherStudentAsync();
        var reservationId = await _factory.CreateReservationAsync(student.Id);
        var otherReservationId = await _factory.CreateReservationAsync(otherStudentId);
        var older = await _factory.RegisterPaymentAsync(reservationId, 500m, DateTime.UtcNow.AddDays(-3));
        var newer = await _factory.RegisterPaymentAsync(reservationId, 300m, DateTime.UtcNow.AddDays(-1));
        await _factory.RegisterPaymentAsync(otherReservationId, 400m, DateTime.UtcNow.AddDays(-2));

        var response = await GetPagedAsync("pageNumber=1&pageSize=10");

        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data.Items.Select(p => p.Id).Should().Equal(newer, older);
        response.Data.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetPaged_WhenPaymentTypesAreProvided_ShouldReturnOnlyMatchingCallerPayments()
    {
        _httpClient = await _factory.CreateStudentClientAsync();
        var student = await _factory.GetOrCreateStudentAsync();
        var otherStudentId = await _factory.CreateOtherStudentAsync();
        var reservationId = await _factory.CreateReservationAsync(student.Id);
        var otherReservationId = await _factory.CreateReservationAsync(otherStudentId);
        await _factory.RegisterPaymentAsync(reservationId, 800m, DateTime.UtcNow.AddDays(-3));
        await _factory.RegisterPaymentAsync(otherReservationId, 800m, DateTime.UtcNow.AddDays(-3));
        var refund = await _factory.SubtractPaymentAsync(reservationId, 100m, PaymentTransactionType.Refund, DateTime.UtcNow.AddDays(-2));
        await _factory.SubtractPaymentAsync(otherReservationId, 100m, PaymentTransactionType.Refund, DateTime.UtcNow.AddDays(-1));

        var response = await GetPagedAsync("pageNumber=1&pageSize=10&paymentTypes=Refund");

        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data.Items.Should().ContainSingle()
            .Which.Id.Should().Be(refund);
    }

    [Fact]
    public async Task GetPaged_WhenSortedByTransactionDateAscending_ShouldReturnOldestFirst()
    {
        _httpClient = await _factory.CreateStudentClientAsync();
        var student = await _factory.GetOrCreateStudentAsync();
        var reservationId = await _factory.CreateReservationAsync(student.Id);
        var newer = await _factory.RegisterPaymentAsync(reservationId, 300m, DateTime.UtcNow.AddDays(-1));
        var older = await _factory.RegisterPaymentAsync(reservationId, 500m, DateTime.UtcNow.AddDays(-3));

        var response = await GetPagedAsync("pageNumber=1&pageSize=10&sortBy=TransactionDateAscending");

        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data.Items.Select(p => p.Id).Should().Equal(older, newer);
    }

    private async Task<EndpointResponse<PagedResult<PaymentTransactionItem>>> GetPagedAsync(string query)
    {
        var response = await _httpClient.GetAsync($"{Url}?{query}");
        return await response.ToEndpointResponse<PagedResult<PaymentTransactionItem>>();
    }
}
