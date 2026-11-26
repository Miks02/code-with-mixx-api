using System.Net;
using AwesomeAssertions;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.PaymentTransactions;
using CodeWithMixx.API.Features.PaymentTransactions.Common;
using CodeWithMixx.IntegrationTests.Common;
using CodeWithMixx.IntegrationTests.Features.Authenticatiion;
using CodeWithMixx.IntegrationTests.Features.PaymentTransactions;

namespace CodeWithMixx.IntegrationTests.Features.PaymentTransactions.Admin;

public class GetPagedPaymentsForAdminTests : IClassFixture<IntegrationTestFactory>, IAsyncLifetime
{
    private const string Url = "/api/admin/payments";
    private readonly IntegrationTestFactory _factory;
    private HttpClient _httpClient;

    public GetPagedPaymentsForAdminTests(IntegrationTestFactory factory)
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
    public async Task GetPaged_WhenUserIsStudent_ShouldReturnForbidden()
    {
        _httpClient = await _factory.CreateStudentClientAsync();

        var response = await GetPagedAsync("pageNumber=1&pageSize=10");

        response.Response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        response.Data.Should().BeNull();
    }

    [Fact]
    public async Task GetPaged_WhenMultipleStudentsHavePayments_ShouldReturnAllPaymentsNewestFirst()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        var student = await _factory.GetOrCreateStudentAsync();
        var otherStudentId = await _factory.CreateOtherStudentAsync();
        var reservationId = await _factory.CreateReservationAsync(student.Id);
        var otherReservationId = await _factory.CreateReservationAsync(otherStudentId);
        var oldest = await _factory.RegisterPaymentAsync(reservationId, 500m, DateTime.UtcNow.AddDays(-3));
        var middle = await _factory.RegisterPaymentAsync(otherReservationId, 400m, DateTime.UtcNow.AddDays(-2));
        var newest = await _factory.RegisterPaymentAsync(reservationId, 300m, DateTime.UtcNow.AddDays(-1));

        var response = await GetPagedAsync("pageNumber=1&pageSize=10");

        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data.Items.Select(p => p.Id).Should().Equal(newest, middle, oldest);
    }

    [Fact]
    public async Task GetPaged_WhenPaymentTypesAreProvided_ShouldReturnOnlyMatchingTypes()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        var student = await _factory.GetOrCreateStudentAsync();
        var reservationId = await _factory.CreateReservationAsync(student.Id);
        await _factory.RegisterPaymentAsync(reservationId, 800m, DateTime.UtcNow.AddDays(-3));
        var refund = await _factory.SubtractPaymentAsync(reservationId, 100m, PaymentTransactionType.Refund, DateTime.UtcNow.AddDays(-2));
        var reversal = await _factory.SubtractPaymentAsync(reservationId, 200m, PaymentTransactionType.Reversal, DateTime.UtcNow.AddDays(-1));

        var response = await GetPagedAsync("pageNumber=1&pageSize=10&paymentTypes=Refund&paymentTypes=Reversal");

        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data.Items.Select(p => p.Id).Should().BeEquivalentTo([refund, reversal]);
    }

    [Fact]
    public async Task GetPaged_WhenSortedByTransactionDateAscending_ShouldReturnOldestFirst()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        var student = await _factory.GetOrCreateStudentAsync();
        var reservationId = await _factory.CreateReservationAsync(student.Id);
        var newest = await _factory.RegisterPaymentAsync(reservationId, 300m, DateTime.UtcNow.AddDays(-1));
        var oldest = await _factory.RegisterPaymentAsync(reservationId, 500m, DateTime.UtcNow.AddDays(-3));
        var middle = await _factory.RegisterPaymentAsync(reservationId, 400m, DateTime.UtcNow.AddDays(-2));

        var response = await GetPagedAsync("pageNumber=1&pageSize=10&sortBy=TransactionDateAscending");

        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data.Items.Select(p => p.Id).Should().Equal(oldest, middle, newest);
    }

    [Fact]
    public async Task GetPaged_WhenSortedByReservationIdDescending_ShouldReturnHighestReservationFirst()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        var student = await _factory.GetOrCreateStudentAsync();
        var firstReservationId = await _factory.CreateReservationAsync(student.Id);
        var secondReservationId = await _factory.CreateReservationAsync(student.Id);
        await _factory.RegisterPaymentAsync(firstReservationId, 500m, DateTime.UtcNow.AddDays(-1));
        await _factory.RegisterPaymentAsync(secondReservationId, 400m, DateTime.UtcNow.AddDays(-3));

        var response = await GetPagedAsync("pageNumber=1&pageSize=10&sortBy=ReservationIdDescending");

        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data.Items.Select(p => p.ReservationId).Should().Equal(secondReservationId, firstReservationId);
    }

    [Fact]
    public async Task GetPaged_WhenPageSizeIsSmallerThanTotal_ShouldReturnRequestedPage()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        var student = await _factory.GetOrCreateStudentAsync();
        var reservationId = await _factory.CreateReservationAsync(student.Id);
        await _factory.RegisterPaymentAsync(reservationId, 300m, DateTime.UtcNow.AddDays(-3));
        await _factory.RegisterPaymentAsync(reservationId, 300m, DateTime.UtcNow.AddDays(-2));
        await _factory.RegisterPaymentAsync(reservationId, 300m, DateTime.UtcNow.AddDays(-1));

        var response = await GetPagedAsync("pageNumber=2&pageSize=2");

        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data.Items.Should().ContainSingle();
        response.Data.TotalCount.Should().Be(3);
        response.Data.TotalPages.Should().Be(2);
    }

    private async Task<EndpointResponse<PagedResult<PaymentTransactionItem>>> GetPagedAsync(string query)
    {
        var response = await _httpClient.GetAsync($"{Url}?{query}");
        return await response.ToEndpointResponse<PagedResult<PaymentTransactionItem>>();
    }
}
