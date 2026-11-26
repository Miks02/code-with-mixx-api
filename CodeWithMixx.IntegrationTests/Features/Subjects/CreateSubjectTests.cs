using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using CodeWithMixx.API.Features.Subjects.Admin.CreateSubject;
using CodeWithMixx.IntegrationTests.Common;

namespace CodeWithMixx.IntegrationTests.Features.Subjects;

public class CreateSubjectTests : IClassFixture<IntegrationTestFactory>, IAsyncLifetime
{
    private const string Url = "/api/admin/subjects";
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
        var response = await CreateSubjectAsync("Test", "Description");
        response.Response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        response.Data.Should().BeNull();
    }

    [Fact]
    public async Task Create_WhenRequestIsInvalid_ShouldReturnBadRequest()
    {
        _httpClient = await _factory.CreateAdminClientAsync();

        var response = await CreateSubjectAsync("", "Description");
        response.Response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Data.Should().BeNull();
        response.ProblemDetails.Should().NotBeNull();
        response.ProblemDetails.Status.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WhenCreatingADuplicateSubject_ShouldReturnConflict()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        await _factory.CreateSubjectAsync("Test", "Description", false);

        var response = await CreateSubjectAsync("Test", "Description");
        response.Response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        response.Data.Should().BeNull();
        response.ProblemDetails.Should().NotBeNull();
        response.ProblemDetails.Status.Should().Be((int)HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Create_WhenCreatingAValidSubject_ShouldReturnCreated()
    {
        _httpClient = await _factory.CreateAdminClientAsync();

        var response = await CreateSubjectAsync("Test", "Description");
        response.Response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.ProblemDetails.Should().BeNull();
        response.Data.Should().NotBeNull();
        response.Data.Id.Should().BePositive();
        response.Data.SubjectName.Should().Be("Test");
        response.Data.SubjectDescription.Should().Be("Description");

        var persisted = await _factory.FindSubjectAsync(response.Data.Id);
        persisted.Should().NotBeNull();
    }

    private async Task<EndpointResponse<CreateSubjectResponse>> CreateSubjectAsync(string subjectName, string subjectDescription)
    {
        var request = new CreateSubjectRequest
        {
            SubjectName = subjectName,
            SubjectDescription = subjectDescription
        };

        var response = await _httpClient.PostAsJsonAsync(Url, request);
        return await response.ToEndpointResponse<CreateSubjectResponse>();
    }
}
