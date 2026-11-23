using System.Net;
using AwesomeAssertions;
using Xunit.Abstractions;

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
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Restore_WhenSubjectIsNotFound_ShouldReturnNotFound()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        var response = await RestoreSubjectAsync(0);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);   
    }
    
    [Fact]
    public async Task Restore_WhenSubjectIsArchived_ShouldReturnNoContent()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        await _factory.CreateSubjectAsync("Test", "Description", true);
        var response = await RestoreSubjectAsync(1);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);   
    }

    [Fact]
    public async Task Restore_WhenSubjectIsNotArchived_ShouldReturnConflict()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        var subject = await _factory.CreateSubjectAsync("Test", "Description", false);
        var response = await RestoreSubjectAsync(subject.Id);
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);   
    }
    
    private async Task<HttpResponseMessage> RestoreSubjectAsync(int id)
    {
        return await _httpClient.PostAsync(Url + id + "/restore", null);
    }
    
}