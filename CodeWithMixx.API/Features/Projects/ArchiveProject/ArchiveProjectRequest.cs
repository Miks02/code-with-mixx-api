using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Projects.ArchiveProject;

public record ArchiveProjectRequest
{
    [FromRoute]
    public int ProjectId { get; init; }
};
