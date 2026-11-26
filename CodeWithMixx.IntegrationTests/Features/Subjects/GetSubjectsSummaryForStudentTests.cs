using System.Net;
using AwesomeAssertions;
using CodeWithMixx.API.Features.Subjects.Student.GetSubjectsSummaryForStudent;
using CodeWithMixx.IntegrationTests.Common;
using CodeWithMixx.IntegrationTests.Features.Authenticatiion;

namespace CodeWithMixx.IntegrationTests.Features.Subjects;

public class GetSubjectsSummaryForStudentTests : IClassFixture<IntegrationTestFactory>, IAsyncLifetime
{
    private const string Url = "/api/student/subjects/summary";
    private readonly IntegrationTestFactory _factory;
    private HttpClient _httpClient;

    public GetSubjectsSummaryForStudentTests(IntegrationTestFactory factory)
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
    public async Task GetSummary_WhenNoSubjectsExist_ShouldReturnZeroStatsAndNoMostPopularSubject()
    {
        _httpClient = await _factory.CreateStudentClientAsync();

        var response = await GetSummaryAsync("pageNumber=1&pageSize=10");

        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data.PagedSubjects.Items.Should().BeEmpty();
        response.Data.TotalSubjects.Should().Be(0);
        response.Data.TaughtSubjects.Should().Be(0);
        response.Data.UntaughtSubjects.Should().Be(0);
        response.Data.MostPopularSubject.Should().BeNull();
    }

    [Fact]
    public async Task GetSummary_WhenCallerHasClasses_ShouldReturnStatsOnlyForCaller()
    {
        _httpClient = await _factory.CreateStudentClientAsync();
        var student = await _factory.GetOrCreateStudentAsync();
        var popular = await _factory.CreateSubjectAsync("Web", "Description", false);
        var taught = await _factory.CreateSubjectAsync("Baze", "Description", false);
        var takenByOthers = await _factory.CreateSubjectAsync("Algoritmi", "Description", false);
        await _factory.CreateSubjectAsync("Mreze", "Description", false);
        await _factory.CreateClassReservationAsync(popular.Id, student.Id);
        await _factory.CreateClassReservationAsync(popular.Id, student.Id);
        await _factory.CreateClassReservationAsync(taught.Id, student.Id);
        await _factory.CreateClassReservationAsync(takenByOthers.Id);
        await _factory.CreateClassReservationAsync(takenByOthers.Id);
        await _factory.CreateClassReservationAsync(takenByOthers.Id);

        var response = await GetSummaryAsync("pageNumber=1&pageSize=10");

        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data.PagedSubjects.Items.Should().HaveCount(4);
        response.Data.TotalSubjects.Should().Be(4);
        response.Data.TaughtSubjects.Should().Be(2);
        response.Data.UntaughtSubjects.Should().Be(2);
        response.Data.PagedSubjects.Items.Single(s => s.Id == takenByOthers.Id).ClassesCount.Should().Be(0);
        response.Data.MostPopularSubject.Should().NotBeNull();
        response.Data.MostPopularSubject.Id.Should().Be(popular.Id);
        response.Data.MostPopularSubject.ClassesCount.Should().Be(2);
    }

    [Fact]
    public async Task GetSummary_WhenOnlyOtherStudentsHaveClasses_ShouldReturnNoTaughtSubjectsAndNoMostPopularSubject()
    {
        _httpClient = await _factory.CreateStudentClientAsync();
        var subject = await _factory.CreateSubjectAsync("Web", "Description", false);
        await _factory.CreateClassReservationAsync(subject.Id);

        var response = await GetSummaryAsync("pageNumber=1&pageSize=10");

        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data.TaughtSubjects.Should().Be(0);
        response.Data.UntaughtSubjects.Should().Be(1);
        response.Data.MostPopularSubject.Should().BeNull();
    }

    [Fact]
    public async Task GetSummary_WhenSubjectIsArchived_ShouldExcludeItFromStats()
    {
        _httpClient = await _factory.CreateStudentClientAsync();
        await _factory.CreateSubjectAsync("Web", "Description", false);
        await _factory.CreateSubjectAsync("Baze", "Description", true);

        var response = await GetSummaryAsync("pageNumber=1&pageSize=10");

        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data.PagedSubjects.Items.Should().ContainSingle();
        response.Data.TotalSubjects.Should().Be(1);
        response.Data.UntaughtSubjects.Should().Be(1);
    }

    [Fact]
    public async Task GetSummary_WhenSearchTermIsProvided_ShouldFilterPageOnly()
    {
        _httpClient = await _factory.CreateStudentClientAsync();
        var web = await _factory.CreateSubjectAsync("Web", "Description", false);
        await _factory.CreateSubjectAsync("Baze", "Description", false);

        var response = await GetSummaryAsync("pageNumber=1&pageSize=10&searchTerm=web");

        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data.PagedSubjects.Items.Should().ContainSingle()
            .Which.Id.Should().Be(web.Id);
        response.Data.UntaughtSubjects.Should().Be(2);
    }

    private async Task<EndpointResponse<GetSubjectsSummaryForStudentResponse>> GetSummaryAsync(string query)
    {
        var response = await _httpClient.GetAsync($"{Url}?{query}");
        return await response.ToEndpointResponse<GetSubjectsSummaryForStudentResponse>();
    }
}
