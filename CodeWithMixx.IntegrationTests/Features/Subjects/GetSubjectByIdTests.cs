using System.Net;
using AwesomeAssertions;
using CodeWithMixx.API.Features.Subjects.GetSubjectById;
using CodeWithMixx.IntegrationTests.Common;

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
        var response = await GetSubjectAsync(0);
        response.Response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        response.Data.Should().BeNull();
    }

    [Fact]
    public async Task GetById_WhenSubjectDoesNotExist_ShouldReturnNotFound()
    {
        _httpClient = await _factory.CreateAdminClientAsync();

        var response = await GetSubjectAsync(0);
        response.Response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Data.Should().BeNull();
        response.ProblemDetails.Should().NotBeNull();
        response.ProblemDetails.Status.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetById_WhenSubjectIsArchived_ShouldReturnNotFound()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        var subject = await _factory.CreateSubjectAsync("Test", "Description", true);

        var response = await GetSubjectAsync(subject.Id);
        response.Response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.ProblemDetails.Should().NotBeNull();
        response.ProblemDetails.Status.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetById_WhenSubjectExists_ShouldReturnOkWithSubject()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        var subject = await _factory.CreateSubjectAsync("Test", "Description", false);

        var response = await GetSubjectAsync(subject.Id);
        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.ProblemDetails.Should().BeNull();
        response.Data.Should().NotBeNull();
        response.Data.Id.Should().Be(subject.Id);
        response.Data.SubjectName.Should().Be("Test");
        response.Data.Description.Should().Be("Description");
        response.Data.CreatedAt.Should().BeCloseTo(subject.CreatedAt, TimeSpan.FromMilliseconds(1));
    }

    private async Task<EndpointResponse<GetSubjectByIdResponse>> GetSubjectAsync(int id)
    {
        var response = await _httpClient.GetAsync(Url + id);
        return await response.ToEndpointResponse<GetSubjectByIdResponse>();
    }
}
