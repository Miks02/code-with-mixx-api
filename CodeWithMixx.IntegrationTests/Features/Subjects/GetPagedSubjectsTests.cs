using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Features.Subjects.GetPagedSubjects;

namespace CodeWithMixx.IntegrationTests.Features.Subjects;

public class GetPagedSubjectsTests : IClassFixture<IntegrationTestFactory>, IAsyncLifetime
{
    private const string Url = "/api/subjects";
    private readonly IntegrationTestFactory _factory;
    private readonly HttpClient _httpClient;

    public GetPagedSubjectsTests(IntegrationTestFactory factory)
    {
        _factory = factory;
        _httpClient = _factory.CreateClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetPaged_WhenNoSubjectsExist_ShouldReturnOkWithEmptyPage()
    {
        var response = await _httpClient.GetAsync($"{Url}?pageNumber=1&pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<PagedResult<GetPagedSubjectsResponse>>();
        body!.Items.Should().BeEmpty();
        body.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetPaged_WhenSubjectIsArchived_ShouldNotReturnIt()
    {
        var active = await _factory.CreateSubjectAsync("Web", "Description", false);
        await _factory.CreateSubjectAsync("Baze", "Description", true);

        var response = await _httpClient.GetAsync($"{Url}?pageNumber=1&pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<PagedResult<GetPagedSubjectsResponse>>();
        body!.Items.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new GetPagedSubjectsResponse
            {
                Id = active.Id,
                Name = active.Name,
                Description = active.Description
            });
        body.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetPaged_WhenSearchTermIsProvided_ShouldReturnMatchingSubjects()
    {
        var byName = await _factory.CreateSubjectAsync("Web Programiranje", "Opis", false);
        var byDescription = await _factory.CreateSubjectAsync("Algoritmi", "Osnove web razvoja", false);
        await _factory.CreateSubjectAsync("Baze Podataka", "Relacione baze", false);

        var response = await _httpClient.GetAsync($"{Url}?pageNumber=1&pageSize=10&searchTerm=WEB");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<PagedResult<GetPagedSubjectsResponse>>();
        body!.Items.Select(s => s.Id).Should().BeEquivalentTo([byName.Id, byDescription.Id]);
    }

    [Fact]
    public async Task GetPaged_WhenPageSizeIsSmallerThanTotal_ShouldReturnRequestedPage()
    {
        await _factory.CreateSubjectAsync("Web", "Description", false);
        await _factory.CreateSubjectAsync("Baze", "Description", false);
        await _factory.CreateSubjectAsync("Algoritmi", "Description", false);

        var response = await _httpClient.GetAsync($"{Url}?pageNumber=2&pageSize=2");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<PagedResult<GetPagedSubjectsResponse>>();
        body!.Items.Should().ContainSingle();
        body.PageNumber.Should().Be(2);
        body.PageSize.Should().Be(2);
        body.TotalCount.Should().Be(3);
        body.TotalPages.Should().Be(2);
    }
}
