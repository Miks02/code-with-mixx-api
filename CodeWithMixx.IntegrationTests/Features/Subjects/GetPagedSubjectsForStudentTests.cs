using System.Net;
using AwesomeAssertions;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Features.Subjects.Student.Common;
using CodeWithMixx.IntegrationTests.Common;
using CodeWithMixx.IntegrationTests.Features.Authenticatiion;

namespace CodeWithMixx.IntegrationTests.Features.Subjects;

public class GetPagedSubjectsForStudentTests : IClassFixture<IntegrationTestFactory>, IAsyncLifetime
{
    private const string Url = "/api/student/subjects";
    private readonly IntegrationTestFactory _factory;
    private HttpClient _httpClient;

    public GetPagedSubjectsForStudentTests(IntegrationTestFactory factory)
    {
        _factory = factory;
        _httpClient = _factory.CreateClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetPagedForStudent_WhenUserIsNotAuthenticated_ShouldReturnUnauthorized()
    {
        var response = await GetPagedAsync("pageNumber=1&pageSize=10");
        response.Response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        response.Data.Should().BeNull();
    }

    [Fact]
    public async Task GetPagedForStudent_WhenUserIsAdmin_ShouldReturnForbidden()
    {
        _httpClient = await _factory.CreateAdminClientAsync();

        var response = await GetPagedAsync("pageNumber=1&pageSize=10");

        response.Response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        response.Data.Should().BeNull();
    }

    [Fact]
    public async Task GetPagedForStudent_WhenSubjectIsArchived_ShouldReturnOnlyActiveSubjects()
    {
        _httpClient = await _factory.CreateStudentClientAsync();
        var active = await _factory.CreateSubjectAsync("Web", "Description", false);
        await _factory.CreateSubjectAsync("Baze", "Description", true);

        var response = await GetPagedAsync("pageNumber=1&pageSize=10");

        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data.Items.Should().ContainSingle()
            .Which.Id.Should().Be(active.Id);
    }

    [Fact]
    public async Task GetPagedForStudent_WhenOtherStudentsHaveClasses_ShouldCountOnlyCallerClasses()
    {
        _httpClient = await _factory.CreateStudentClientAsync();
        var student = await _factory.GetOrCreateStudentAsync();
        var subject = await _factory.CreateSubjectAsync("Web", "Description", false);
        await _factory.CreateClassReservationAsync(subject.Id, student.Id);
        await _factory.CreateClassReservationAsync(subject.Id, student.Id);
        await _factory.CreateClassReservationAsync(subject.Id);

        var response = await GetPagedAsync("pageNumber=1&pageSize=10");

        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data.Items.Should().ContainSingle()
            .Which.ClassesCount.Should().Be(2);
    }

    [Fact]
    public async Task GetPagedForStudent_WhenSearchTermIsProvided_ShouldReturnMatchingSubjects()
    {
        _httpClient = await _factory.CreateStudentClientAsync();
        var byName = await _factory.CreateSubjectAsync("Web Programiranje", "Opis", false);
        var byDescription = await _factory.CreateSubjectAsync("Algoritmi", "Osnove web razvoja", false);
        await _factory.CreateSubjectAsync("Baze Podataka", "Relacione baze", false);

        var response = await GetPagedAsync("pageNumber=1&pageSize=10&searchTerm=WEB");

        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data.Items.Select(s => s.Id).Should().BeEquivalentTo([byName.Id, byDescription.Id]);
    }

    [Fact]
    public async Task GetPagedForStudent_WhenSortedByNameAscending_ShouldReturnSubjectsInOrder()
    {
        _httpClient = await _factory.CreateStudentClientAsync();
        await _factory.CreateSubjectAsync("Web", "Description", false);
        await _factory.CreateSubjectAsync("Algoritmi", "Description", false);
        await _factory.CreateSubjectAsync("Baze", "Description", false);

        var response = await GetPagedAsync("pageNumber=1&pageSize=10&sortBy=SubjectNameAscending");

        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data.Items.Select(s => s.SubjectName).Should().Equal("Algoritmi", "Baze", "Web");
    }

    [Fact]
    public async Task GetPagedForStudent_WhenSortedByCreatedAtDescending_ShouldReturnNewestFirst()
    {
        _httpClient = await _factory.CreateStudentClientAsync();
        await _factory.CreateSubjectAsync("Web", "Description", false);
        await _factory.CreateSubjectAsync("Algoritmi", "Description", false);
        await _factory.CreateSubjectAsync("Baze", "Description", false);

        var response = await GetPagedAsync("pageNumber=1&pageSize=10&sortBy=CreatedAtDescending");

        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data.Items.Select(s => s.SubjectName).Should().Equal("Baze", "Algoritmi", "Web");
    }

    [Fact]
    public async Task GetPagedForStudent_WhenPageSizeIsSmallerThanTotal_ShouldReturnRequestedPage()
    {
        _httpClient = await _factory.CreateStudentClientAsync();
        await _factory.CreateSubjectAsync("Web", "Description", false);
        await _factory.CreateSubjectAsync("Baze", "Description", false);
        await _factory.CreateSubjectAsync("Algoritmi", "Description", false);

        var response = await GetPagedAsync("pageNumber=2&pageSize=2");

        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data.Items.Should().ContainSingle();
        response.Data.TotalCount.Should().Be(3);
        response.Data.TotalPages.Should().Be(2);
    }

    private async Task<EndpointResponse<PagedResult<SubjectItem>>> GetPagedAsync(string query)
    {
        var response = await _httpClient.GetAsync($"{Url}?{query}");
        return await response.ToEndpointResponse<PagedResult<SubjectItem>>();
    }
}
