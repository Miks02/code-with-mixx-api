using System.Net;
using AwesomeAssertions;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.IntegrationTests.Common;

namespace CodeWithMixx.IntegrationTests.Features.Subjects;

public class DeleteSubjectTests : IClassFixture<IntegrationTestFactory>, IAsyncLifetime
{
    private const string Url = "/api/admin/subjects/";
    private readonly IntegrationTestFactory _factory;
    private HttpClient _httpClient;

    public DeleteSubjectTests(IntegrationTestFactory factory)
    {
        _factory = factory;
        _httpClient = _factory.CreateClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Delete_WhenUserIsNotAdmin_ShouldReturnUnauthorized()
    {
        var response = await DeleteSubjectAsync(0);
        response.Response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Delete_WhenSubjectDoesNotExist_ShouldReturnNotFound()
    {
        _httpClient = await _factory.CreateAdminClientAsync();

        var response = await DeleteSubjectAsync(0);
        response.Response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.ProblemDetails.Should().NotBeNull();
        response.ProblemDetails.Status.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WhenSubjectHasClasses_ShouldReturnConflict()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        var subject = await _factory.CreateSubjectAsync("Test", "Description", false);
        await _factory.CreateClassReservationAsync(subject.Id);

        var response = await DeleteSubjectAsync(subject.Id);
        response.Response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        response.ProblemDetails.Should().NotBeNull();
        response.ProblemDetails.Status.Should().Be((int)HttpStatusCode.Conflict);

        var persisted = await _factory.FindSubjectAsync(subject.Id);
        persisted.Should().NotBeNull();
    }

    [Fact]
    public async Task Delete_WhenSubjectHasNoClasses_ShouldReturnNoContentAndRemoveSubject()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        var subject = await _factory.CreateSubjectAsync("Test", "Description", false);

        var response = await DeleteSubjectAsync(subject.Id);
        response.Response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        response.ProblemDetails.Should().BeNull();

        var persisted = await _factory.FindSubjectAsync(subject.Id);
        persisted.Should().BeNull();
    }

    private async Task<EndpointResponse<Result>> DeleteSubjectAsync(int id)
    {
        var response = await _httpClient.DeleteAsync(Url + id);
        return await response.ToEndpointResponse<Result>();
    }
}
