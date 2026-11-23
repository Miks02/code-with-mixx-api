using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using CodeWithMixx.API.Features.Subjects.GetSubjectById;

namespace CodeWithMixx.IntegrationTests.Features.Subjects;

public class GetSubjectByIdTests : IClassFixture<IntegrationTestFactory>, IAsyncLifetime
{
    private const string Url = "/api/subjects/";
    private readonly IntegrationTestFactory _factory;
    private HttpClient _httpClient;

    public GetSubjectByIdTests(IntegrationTestFactory factory)
    {
        _factory = factory;
        _httpClient = _factory.CreateClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetById_WhenUserIsAnonymous_ShouldReturnUnauthorized()
    {
        var response = await GetSubjectAsync(1);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetById_WhenSubjectDoesNotExist_ShouldReturnNotFound()
    {
        _httpClient = await _factory.CreateAdminClientAsync();

        var response = await GetSubjectAsync(0);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetById_WhenSubjectIsArchived_ShouldReturnNotFound()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        var subject = await _factory.CreateSubjectAsync("Test", "Description", true);

        var response = await GetSubjectAsync(subject.Id);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetById_WhenSubjectExists_ShouldReturnOkWithSubject()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        var subject = await _factory.CreateSubjectAsync("Test", "Description", false);

        var response = await GetSubjectAsync(subject.Id);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<GetSubjectByIdResponse>();
        body.Should().NotBeNull();
        body.Id.Should().Be(subject.Id);
        body.SubjectName.Should().Be("Test");
        body.Description.Should().Be("Description");
        body.CreatedAt.Should().BeCloseTo(subject.CreatedAt, TimeSpan.FromMilliseconds(1));
    }

    private async Task<HttpResponseMessage> GetSubjectAsync(int id)
    {
        return await _httpClient.GetAsync(Url + id);
    }
}
