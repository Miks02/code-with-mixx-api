using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using CodeWithMixx.API.Features.Subjects.UpdateSubject;

namespace CodeWithMixx.IntegrationTests.Features.Subjects;

public class UpdateSubjectTests : IClassFixture<IntegrationTestFactory>, IAsyncLifetime
{
    private const string Url = "/api/admin/subjects/";
    private readonly IntegrationTestFactory _factory;
    private HttpClient _httpClient;

    public UpdateSubjectTests(IntegrationTestFactory factory)
    {
        _factory = factory;
        _httpClient = _factory.CreateClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Update_WhenUserIsNotAdmin_ShouldReturnUnauthorized()
    {
        var response = await UpdateSubjectAsync(1, "Test", "Description");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Update_WhenSubjectDoesNotExist_ShouldReturnNotFound()
    {
        _httpClient = await _factory.CreateAdminClientAsync();

        var response = await UpdateSubjectAsync(0, "Test", "Description");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_WhenRequestIsInvalid_ShouldReturnBadRequest()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        var subject = await _factory.CreateSubjectAsync("Test", "Description", false);

        var response = await UpdateSubjectAsync(subject.Id, "", "Description");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_WhenNameBelongsToAnotherSubject_ShouldReturnConflict()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        await _factory.CreateSubjectAsync("Web", "Description", false);
        var subject = await _factory.CreateSubjectAsync("Baze", "Description", false);

        var response = await UpdateSubjectAsync(subject.Id, "WEB", "Description");
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Update_WhenOnlyNameCasingChanges_ShouldReturnOk()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        var subject = await _factory.CreateSubjectAsync("web", "Description", false);

        var response = await UpdateSubjectAsync(subject.Id, "Web", "Description");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Update_WhenRequestIsValid_ShouldReturnOkAndPersistChanges()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        var subject = await _factory.CreateSubjectAsync("Test", "Description", false);
        await _factory.CreateClassReservationAsync(subject.Id);

        var response = await UpdateSubjectAsync(subject.Id, "  Updated  ", "Updated description");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<UpdateSubjectResponse>();
        body.Should().NotBeNull();
        body.Id.Should().Be(subject.Id);
        body.SubjectName.Should().Be("Updated");
        body.SubjectDescription.Should().Be("Updated description");
        body.ClassesCount.Should().Be(1);
        body.StudentsCount.Should().Be(1);

        var persisted = await _factory.FindSubjectAsync(subject.Id);
        persisted!.Name.Should().Be("Updated");
        persisted.Description.Should().Be("Updated description");
    }

    private async Task<HttpResponseMessage> UpdateSubjectAsync(int id, string subjectName, string subjectDescription)
    {
        var request = new UpdateSubjectRequest
        {
            SubjectName = subjectName,
            SubjectDescription = subjectDescription
        };

        return await _httpClient.PutAsJsonAsync(Url + id, request);
    }
}
