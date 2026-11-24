using System.Net;
using AwesomeAssertions;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.IntegrationTests.Common;

namespace CodeWithMixx.IntegrationTests.Features.Subjects;

public class ArchiveSubjectTests : IClassFixture<IntegrationTestFactory>, IAsyncLifetime
{
    private const string Url = "/api/admin/subjects/";
    private readonly IntegrationTestFactory _factory;
    private HttpClient _httpClient;

    public ArchiveSubjectTests(IntegrationTestFactory factory)
    {
        _factory = factory;
        _httpClient = _factory.CreateClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Archive_WhenUserIsNotAdmin_ShouldReturnUnauthorized()
    {
        var response = await ArchiveSubjectAsync(0);
        response.Response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Archive_WhenSubjectIsArchived_ShouldReturnConflict()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        var subject = await _factory.CreateSubjectAsync("Test description", "Test description", true);

        var response = await ArchiveSubjectAsync(subject.Id);
        response.Response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        response.ProblemDetails.Should().NotBeNull();
        response.ProblemDetails.Status.Should().Be((int)HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Archive_WhenSubjectDoesNotExist_ShouldReturnNotFound()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        var response = await ArchiveSubjectAsync(0);
        response.Response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.ProblemDetails.Should().NotBeNull();
        response.ProblemDetails.Status.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Archive_WhenSubjectIsNotArchived_ShouldReturnNoContent()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        var subject = await _factory.CreateSubjectAsync("Test description", "Test description", false);

        var response = await ArchiveSubjectAsync(subject.Id);
        response.Response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        response.ProblemDetails.Should().BeNull();
    }

    private async Task<EndpointResponse<Result>> ArchiveSubjectAsync(int id)
    {
        var response = await _httpClient.PostAsync(Url + id + "/archive", null);
        return await response.ToEndpointResponse<Result>();
    }
}
