using System.Net;
using System.Text;
using System.Text.Json;
using AwesomeAssertions;
using CodeWithMixx.API.Features.Subjects.CreateSubject;

namespace CodeWithMixx.IntegrationTests.Features.Subjects;

public class CreateSubjectTests : IClassFixture<IntegrationTestFactory>, IAsyncLifetime
{
    private readonly IntegrationTestFactory _factory;
    private HttpClient _httpClient;

    public CreateSubjectTests(IntegrationTestFactory factory)
    {
        _factory = factory;
        _httpClient = _factory.CreateClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Create_WhenUserIsNotAdmin_ShouldReturnUnauthorized()
    {
        var response = await CreateSubjectAsync();
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_WhenCreatingADuplicateSubject_ShouldReturnConflict()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        await _factory.CreateSubjectAsync("Test", "Description", false);
        
        var response = await CreateSubjectAsync();
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
    
    [Fact]
    public async Task Create_WhenCreatingAValidSubject_ShouldReturnCreated()
    {
        _httpClient = await _factory.CreateAdminClientAsync();

        var response = await CreateSubjectAsync();
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    private async Task<HttpResponseMessage> CreateSubjectAsync()
    {
        var request = new CreateSubjectRequest
        {
            SubjectName = "Test",
            SubjectDescription = "Description"
        };
        
        return await _httpClient.PostAsync("/api/admin/subjects",
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));
    }

}