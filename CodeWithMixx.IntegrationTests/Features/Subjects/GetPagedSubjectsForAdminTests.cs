using System.Net;
using AwesomeAssertions;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Features.Subjects.Admin.GetPagedSubjectsForAdmin;
using CodeWithMixx.IntegrationTests.Common;

namespace CodeWithMixx.IntegrationTests.Features.Subjects;

public class GetPagedSubjectsForAdminTests : IClassFixture<IntegrationTestFactory>, IAsyncLifetime
{
    private const string Url = "/api/admin/subjects";
    private readonly IntegrationTestFactory _factory;
    private HttpClient _httpClient;

    public GetPagedSubjectsForAdminTests(IntegrationTestFactory factory)
    {
        _factory = factory;
        _httpClient = _factory.CreateClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetPagedForAdmin_WhenUserIsNotAdmin_ShouldReturnUnauthorized()
    {
        var response = await GetPagedAsync("pageNumber=1&pageSize=10&onlyArchived=false");
        response.Response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        response.Data.Should().BeNull();
    }

    [Fact]
    public async Task GetPagedForAdmin_WhenOnlyArchivedIsFalse_ShouldReturnOnlyActiveSubjects()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        var active = await _factory.CreateSubjectAsync("Web", "Description", false);
        await _factory.CreateSubjectAsync("Baze", "Description", true);

        var response = await GetPagedAsync("pageNumber=1&pageSize=10&onlyArchived=false");

        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data.Items.Should().ContainSingle()
            .Which.Id.Should().Be(active.Id);
    }

    [Fact]
    public async Task GetPagedForAdmin_WhenOnlyArchivedIsTrue_ShouldReturnOnlyArchivedSubjects()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        await _factory.CreateSubjectAsync("Web", "Description", false);
        var archived = await _factory.CreateSubjectAsync("Baze", "Description", true);

        var response = await GetPagedAsync("pageNumber=1&pageSize=10&onlyArchived=true");

        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        var item = response.Data.Items.Should().ContainSingle().Subject;
        item.Id.Should().Be(archived.Id);
        item.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPagedForAdmin_WhenSearchTermIsProvided_ShouldReturnMatchingSubjects()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        var byName = await _factory.CreateSubjectAsync("Web Programiranje", "Opis", false);
        var byDescription = await _factory.CreateSubjectAsync("Algoritmi", "Osnove web razvoja", false);
        await _factory.CreateSubjectAsync("Baze Podataka", "Relacione baze", false);

        var response = await GetPagedAsync("pageNumber=1&pageSize=10&onlyArchived=false&searchTerm=WEB");

        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data.Items.Select(s => s.Id).Should().BeEquivalentTo([byName.Id, byDescription.Id]);
    }

    [Fact]
    public async Task GetPagedForAdmin_WhenSortedByNameAscending_ShouldReturnSubjectsInOrder()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        await _factory.CreateSubjectAsync("Web", "Description", false);
        await _factory.CreateSubjectAsync("Algoritmi", "Description", false);
        await _factory.CreateSubjectAsync("Baze", "Description", false);

        var response = await GetPagedAsync("pageNumber=1&pageSize=10&onlyArchived=false&sortBy=SubjectNameAscending");

        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data.Items.Select(s => s.SubjectName).Should().Equal("Algoritmi", "Baze", "Web");
    }

    [Fact]
    public async Task GetPagedForAdmin_WhenSortedByCreatedAtDescending_ShouldReturnNewestFirst()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        await _factory.CreateSubjectAsync("Web", "Description", false);
        await _factory.CreateSubjectAsync("Algoritmi", "Description", false);
        await _factory.CreateSubjectAsync("Baze", "Description", false);

        var response = await GetPagedAsync("pageNumber=1&pageSize=10&onlyArchived=false&sortBy=CreatedAtDescending");

        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data.Items.Select(s => s.SubjectName).Should().Equal("Baze", "Algoritmi", "Web");
    }

    [Fact]
    public async Task GetPagedForAdmin_WhenSubjectHasClasses_ShouldReturnClassesAndStudentsCount()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        var subject = await _factory.CreateSubjectAsync("Web", "Description", false);
        await _factory.CreateClassReservationAsync(subject.Id);
        await _factory.CreateClassReservationAsync(subject.Id);

        var response = await GetPagedAsync("pageNumber=1&pageSize=10&onlyArchived=false");

        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        var item = response.Data.Items.Should().ContainSingle().Subject;
        item.ClassesCount.Should().Be(2);
        item.StudentsCount.Should().Be(2);
    }

    [Fact]
    public async Task GetPagedForAdmin_WhenPageSizeIsSmallerThanTotal_ShouldReturnRequestedPage()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        await _factory.CreateSubjectAsync("Web", "Description", false);
        await _factory.CreateSubjectAsync("Baze", "Description", false);
        await _factory.CreateSubjectAsync("Algoritmi", "Description", false);

        var response = await GetPagedAsync("pageNumber=2&pageSize=2&onlyArchived=false");

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
