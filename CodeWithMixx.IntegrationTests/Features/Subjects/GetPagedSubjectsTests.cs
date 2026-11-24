using System.Net;
using AwesomeAssertions;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Features.Subjects.GetPagedSubjects;
using CodeWithMixx.IntegrationTests.Common;

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
        var response = await GetPagedAsync("pageNumber=1&pageSize=10");

        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data.Items.Should().BeEmpty();
        response.Data.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetPaged_WhenSubjectIsArchived_ShouldNotReturnIt()
    {
        var active = await _factory.CreateSubjectAsync("Web", "Description", false);
        await _factory.CreateSubjectAsync("Baze", "Description", true);

        var response = await GetPagedAsync("pageNumber=1&pageSize=10");

        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data.Items.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new GetPagedSubjectsResponse
            {
                Id = active.Id,
                Name = active.Name,
                Description = active.Description
            });
        response.Data.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetPaged_WhenSearchTermIsProvided_ShouldReturnMatchingSubjects()
    {
        var byName = await _factory.CreateSubjectAsync("Web Programiranje", "Opis", false);
        var byDescription = await _factory.CreateSubjectAsync("Algoritmi", "Osnove web razvoja", false);
        await _factory.CreateSubjectAsync("Baze Podataka", "Relacione baze", false);

        var response = await GetPagedAsync("pageNumber=1&pageSize=10&searchTerm=WEB");

        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data.Items.Select(s => s.Id).Should().BeEquivalentTo([byName.Id, byDescription.Id]);
    }

    [Fact]
    public async Task GetPaged_WhenPageSizeIsSmallerThanTotal_ShouldReturnRequestedPage()
    {
        await _factory.CreateSubjectAsync("Web", "Description", false);
        await _factory.CreateSubjectAsync("Baze", "Description", false);
        await _factory.CreateSubjectAsync("Algoritmi", "Description", false);

        var response = await GetPagedAsync("pageNumber=2&pageSize=2");

        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data.Items.Should().ContainSingle();
        response.Data.PageNumber.Should().Be(2);
        response.Data.PageSize.Should().Be(2);
        response.Data.TotalCount.Should().Be(3);
        response.Data.TotalPages.Should().Be(2);
    }

    private async Task<EndpointResponse<PagedResult<GetPagedSubjectsResponse>>> GetPagedAsync(string query)
    {
        var response = await _httpClient.GetAsync($"{Url}?{query}");
        return await response.ToEndpointResponse<PagedResult<GetPagedSubjectsResponse>>();
    }
}
