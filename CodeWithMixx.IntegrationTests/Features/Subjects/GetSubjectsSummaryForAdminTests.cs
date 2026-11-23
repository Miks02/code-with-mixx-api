using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using CodeWithMixx.API.Features.Subjects.GetSubjectsSummaryForAdmin;

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
        var response = await _httpClient.GetAsync(Url);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetSummary_WhenNoSubjectsExist_ShouldReturnZeroStatsAndNoMostPopularSubject()
    {
        _httpClient = await _factory.CreateAdminClientAsync();

        var body = await GetSummaryAsync("pageNumber=1&pageSize=10&onlyArchived=false");

        body.PagedSubjects.Items.Should().BeEmpty();
        body.TotalSubjects.Should().Be(0);
        body.TaughtSubjects.Should().Be(0);
        body.UntaughtSubjects.Should().Be(0);
        body.MostPopularSubject.Should().BeNull();
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

        var body = await GetSummaryAsync("pageNumber=1&pageSize=10&onlyArchived=false");

        body.PagedSubjects.Items.Should().HaveCount(3);
        body.TotalSubjects.Should().Be(3);
        body.TaughtSubjects.Should().Be(2);
        body.UntaughtSubjects.Should().Be(1);
        body.MostPopularSubject.Should().NotBeNull();
        body.MostPopularSubject.Id.Should().Be(popular.Id);
        body.MostPopularSubject.ClassesCount.Should().Be(2);
        body.MostPopularSubject.StudentsCount.Should().Be(2);
    }

    [Fact]
    public async Task GetSummary_WhenSubjectIsArchived_ShouldExcludeItFromStats()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        await _factory.CreateSubjectAsync("Web", "Description", false);
        await _factory.CreateSubjectAsync("Baze", "Description", true);

        var body = await GetSummaryAsync("pageNumber=1&pageSize=10&onlyArchived=false");

        body.PagedSubjects.Items.Should().ContainSingle();
        body.TotalSubjects.Should().Be(1);
        body.UntaughtSubjects.Should().Be(1);
    }

    [Fact]
    public async Task GetSummary_WhenOnlyArchivedIsTrue_ShouldReturnOnlyArchivedSubjectsInPage()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        await _factory.CreateSubjectAsync("Web", "Description", false);
        var archived = await _factory.CreateSubjectAsync("Baze", "Description", true);

        var body = await GetSummaryAsync("pageNumber=1&pageSize=10&onlyArchived=true");

        body.PagedSubjects.Items.Should().ContainSingle()
            .Which.Id.Should().Be(archived.Id);
    }

    [Fact]
    public async Task GetSummary_WhenSearchTermIsProvided_ShouldFilterPageOnly()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        var web = await _factory.CreateSubjectAsync("Web", "Description", false);
        await _factory.CreateSubjectAsync("Baze", "Description", false);

        var body = await GetSummaryAsync("pageNumber=1&pageSize=10&onlyArchived=false&searchTerm=web");

        body.PagedSubjects.Items.Should().ContainSingle()
            .Which.Id.Should().Be(web.Id);
        body.TotalSubjects.Should().Be(2);
    }

    private async Task<GetSubjectsSummaryForAdminResponse> GetSummaryAsync(string query)
    {
        var response = await _httpClient.GetAsync($"{Url}?{query}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<GetSubjectsSummaryForAdminResponse>();
        return body!;
    }
}
