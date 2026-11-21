using System.Net;
using AwesomeAssertions;

namespace CodeWithMixx.IntegrationTests.Features.Subjects;

public class ArchiveSubjectTests : IClassFixture<IntegrationTestFactory>
{
    private const string Url = "/api/admin/subjects/";
    private readonly IntegrationTestFactory _factory;
    private readonly HttpClient _httpClient;

    public ArchiveSubjectTests(IntegrationTestFactory factory)
    {   
        _factory = factory;
        _httpClient = _factory.CreateClient();
    }

    [Fact]
    public async Task Archive_WhenSubjectIsArchived_ShouldReturnNotFound()
    {
        await _factory.ResetDatabaseAsync();
        var subject = _factory.CreateSubjectAsync("Test description", "Test description", true);

        var response = await ArchiveSubjectAsync(subject.Id);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task Archive_WhenSubjectDoesNotExist_ShouldReturnNotFound()
    {
        var response = await ArchiveSubjectAsync(0);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task Archive_WhenSubjectIsNotArchived_ShouldReturnOk()
    {
        await _factory.ResetDatabaseAsync();
        var subject = _factory.CreateSubjectAsync("Test description", "Test description", false);

        var response = await ArchiveSubjectAsync(subject.Id);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    
    
    private async Task<HttpResponseMessage> ArchiveSubjectAsync(int id)
    {
        return await _httpClient.PostAsync(Url + id + "/archive", null);
    }
}