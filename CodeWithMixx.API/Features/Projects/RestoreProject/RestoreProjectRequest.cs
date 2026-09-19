using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Projects.RestoreProject;

public record RestoreProjectRequest
{
    [FromRoute]
    public int ProjectId { get; init; }
};
