using System.Net;
using AwesomeAssertions;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.IntegrationTests.Common;

namespace CodeWithMixx.IntegrationTests.Features.Subjects;

public class RestoreSubjectTests : IClassFixture<IntegrationTestFactory>, IAsyncLifetime
{
    private const string Url = "/api/admin/subjects/";
    private readonly IntegrationTestFactory _factory;
    private HttpClient _httpClient;

    public RestoreSubjectTests(IntegrationTestFactory factory)
    {
        _factory = factory;
        _httpClient = _factory.CreateClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Restore_WhenUserIsNotAdmin_ShouldReturnUnauthorized()
    {
        var response = await RestoreSubjectAsync(0);
        response.Response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Restore_WhenSubjectIsNotFound_ShouldReturnNotFound()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        var response = await RestoreSubjectAsync(0);
        response.Response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.ProblemDetails.Should().NotBeNull();
        response.ProblemDetails.Status.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Restore_WhenSubjectIsArchived_ShouldReturnNoContent()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        var subject = await _factory.CreateSubjectAsync("Test", "Description", true);
        var response = await RestoreSubjectAsync(subject.Id);
        response.Response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        response.ProblemDetails.Should().BeNull();
    }

    [Fact]
    public async Task Restore_WhenSubjectIsNotArchived_ShouldReturnConflict()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        var subject = await _factory.CreateSubjectAsync("Test", "Description", false);
        var response = await RestoreSubjectAsync(subject.Id);
        response.Response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        response.ProblemDetails.Should().NotBeNull();
        response.ProblemDetails.Status.Should().Be((int)HttpStatusCode.Conflict);
    }

    private async Task<EndpointResponse<Result>> RestoreSubjectAsync(int id)
    {
        var response = await _httpClient.PostAsync(Url + id + "/restore", null);
        return await response.ToEndpointResponse<Result>();
    }
}
