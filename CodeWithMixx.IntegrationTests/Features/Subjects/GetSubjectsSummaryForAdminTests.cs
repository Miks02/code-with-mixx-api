using System.Net;
using AwesomeAssertions;
using CodeWithMixx.API.Features.Subjects.GetSubjectsSummaryForAdmin;
using CodeWithMixx.IntegrationTests.Common;

namespace CodeWithMixx.IntegrationTests.Features.Subjects;

public class GetSubjectsSummaryForAdminTests : IClassFixture<IntegrationTestFactory>, IAsyncLifetime
{
    private const string Url = "/api/admin/subjects/summary";
    private readonly IntegrationTestFactory _factory;
    private HttpClient _httpClient;

    public GetSubjectsSummaryForAdminTests(IntegrationTestFactory factory)
    {
        _factory = factory;
        _httpClient = _factory.CreateClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetSummary_WhenUserIsNotAdmin_ShouldReturnUnauthorized()
    {
        var response = await GetSummaryAsync("pageNumber=1&pageSize=10&onlyArchived=false");
        response.Response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        response.Data.Should().BeNull();
    }

    [Fact]
    public async Task GetSummary_WhenNoSubjectsExist_ShouldReturnZeroStatsAndNoMostPopularSubject()
    {
        _httpClient = await _factory.CreateAdminClientAsync();

        var response = await GetSummaryAsync("pageNumber=1&pageSize=10&onlyArchived=false");

        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data.PagedSubjects.Items.Should().BeEmpty();
        response.Data.TotalSubjects.Should().Be(0);
        response.Data.TaughtSubjects.Should().Be(0);
        response.Data.UntaughtSubjects.Should().Be(0);
        response.Data.MostPopularSubject.Should().BeNull();
    }

    [Fact]
    public async Task GetSummary_WhenSubjectsExist_ShouldReturnStatsAndMostPopularSubject()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        var popular = await _factory.CreateSubjectAsync("Web", "Description", false);
        var taught = await _factory.CreateSubjectAsync("Baze", "Description", false);
        await _factory.CreateSubjectAsync("Algoritmi", "Description", false);
        await _factory.CreateClassReservationAsync(popular.Id);
        await _factory.CreateClassReservationAsync(popular.Id);
        await _factory.CreateClassReservationAsync(taught.Id);

        var response = await GetSummaryAsync("pageNumber=1&pageSize=10&onlyArchived=false");

        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data.PagedSubjects.Items.Should().HaveCount(3);
        response.Data.TotalSubjects.Should().Be(3);
        response.Data.TaughtSubjects.Should().Be(2);
        response.Data.UntaughtSubjects.Should().Be(1);
        response.Data.MostPopularSubject.Should().NotBeNull();
        response.Data.MostPopularSubject.Id.Should().Be(popular.Id);
        response.Data.MostPopularSubject.ClassesCount.Should().Be(2);
        response.Data.MostPopularSubject.StudentsCount.Should().Be(2);
    }

    [Fact]
    public async Task GetSummary_WhenSubjectIsArchived_ShouldExcludeItFromStats()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        await _factory.CreateSubjectAsync("Web", "Description", false);
        await _factory.CreateSubjectAsync("Baze", "Description", true);

        var response = await GetSummaryAsync("pageNumber=1&pageSize=10&onlyArchived=false");

        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data.PagedSubjects.Items.Should().ContainSingle();
        response.Data.TotalSubjects.Should().Be(1);
        response.Data.UntaughtSubjects.Should().Be(1);
    }

    [Fact]
    public async Task GetSummary_WhenOnlyArchivedIsTrue_ShouldReturnOnlyArchivedSubjectsInPage()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        await _factory.CreateSubjectAsync("Web", "Description", false);
        var archived = await _factory.CreateSubjectAsync("Baze", "Description", true);

        var response = await GetSummaryAsync("pageNumber=1&pageSize=10&onlyArchived=true");

        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data.PagedSubjects.Items.Should().ContainSingle()
            .Which.Id.Should().Be(archived.Id);
    }

    [Fact]
    public async Task GetSummary_WhenSearchTermIsProvided_ShouldFilterPageOnly()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        var web = await _factory.CreateSubjectAsync("Web", "Description", false);
        await _factory.CreateSubjectAsync("Baze", "Description", false);

        var response = await GetSummaryAsync("pageNumber=1&pageSize=10&onlyArchived=false&searchTerm=web");

        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data.PagedSubjects.Items.Should().ContainSingle()
            .Which.Id.Should().Be(web.Id);
        response.Data.TotalSubjects.Should().Be(2);
    }

    private async Task<EndpointResponse<GetSubjectsSummaryForAdminResponse>> GetSummaryAsync(string query)
    {
        var response = await _httpClient.GetAsync($"{Url}?{query}");
        return await response.ToEndpointResponse<GetSubjectsSummaryForAdminResponse>();
    }
}
