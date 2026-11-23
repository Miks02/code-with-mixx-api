using System.Net;
using AwesomeAssertions;
using CodeWithMixx.API.Domain.Entities.Subjects;
using CodeWithMixx.API.Features.Subjects.GetSubjectsLookup;
using CodeWithMixx.API.Infrastructure.Persistence;
using CodeWithMixx.IntegrationTests.Common;
using Microsoft.Extensions.DependencyInjection;

namespace CodeWithMixx.IntegrationTests.Features.Subjects;

public class GetSubjectsLookupTests : IClassFixture<IntegrationTestFactory>, IAsyncLifetime
{
    private const string Url = "api/subjects/lookup";

    private readonly IntegrationTestFactory _factory;
    private readonly HttpClient _httpClient;

    public GetSubjectsLookupTests(IntegrationTestFactory factory)
    {
        _factory = factory;
        _httpClient = factory.CreateHttpsClients();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetLookup_WhenNoSubjectsExist_ShouldReturnOkWithEmptyList()
    {
        var response = await GetLookupAsync();

        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Response.Content.Headers.ContentType!.MediaType.Should().Be("application/json");
        response.Data.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task GetLookup_WhenSubjectsExist_ShouldReturnOkWithAllSubjects()
    {
        var web = await SeedSubjectAsync("Web Programiranje", "Predmet se bavi izučavanjem modernih web tehnologija.");
        var db = await SeedSubjectAsync("Baze Podataka", "Predmet se bavi relacionim bazama podataka.");

        var response = await GetLookupAsync();

        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().BeEquivalentTo(
        [
            new GetSubjectsLookupResponse { Id = web.Id, SubjectName = web.Name },
            new GetSubjectsLookupResponse { Id = db.Id, SubjectName = db.Name }
        ]);
    }

    [Fact]
    public async Task GetLookup_WhenSubjectIsArchived_ShouldNotReturnIt()
    {
        var active = await SeedSubjectAsync("Web Programiranje", "Predmet se bavi izučavanjem modernih web tehnologija.");
        await SeedSubjectAsync("Baze Podataka", "Predmet se bavi relacionim bazama podataka.", archived: true);

        var response = await GetLookupAsync();

        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new GetSubjectsLookupResponse { Id = active.Id, SubjectName = active.Name });
    }

    [Fact]
    public async Task GetLookup_WhenUserIsAnonymous_ShouldReturnOk()
    {
        await SeedSubjectAsync("Web Programiranje", "Predmet se bavi izučavanjem modernih web tehnologija.");

        var response = await _factory.CreateHttpsClients().GetAsync(Url);
        var endpointResponse = await response.ToEndpointResponse<List<GetSubjectsLookupResponse>>();

        endpointResponse.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        endpointResponse.ProblemDetails.Should().BeNull();
    }

    private async Task<EndpointResponse<List<GetSubjectsLookupResponse>>> GetLookupAsync()
    {
        var response = await _httpClient.GetAsync(Url);
        return await response.ToEndpointResponse<List<GetSubjectsLookupResponse>>();
    }

    private async Task<Subject> SeedSubjectAsync(string name, string description, bool archived = false)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var subject = Subject.Create(name, description).Payload!;
        if (archived)
            subject.Archive();

        db.Subjects.Add(subject);
        await db.SaveChangesAsync();
        return subject;
    }
}
