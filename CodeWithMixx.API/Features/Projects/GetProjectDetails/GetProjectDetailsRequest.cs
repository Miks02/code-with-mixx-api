using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Projects.GetProjectDetails;

public record GetProjectDetailsRequest
{
    [FromRoute]
    public int ProjectId { get; init; }
};
