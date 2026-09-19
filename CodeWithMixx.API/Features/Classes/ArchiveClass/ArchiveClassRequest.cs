using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Classes.ArchiveClass;

public record ArchiveClassRequest
{
    [FromRoute]
    public int ClassId { get; init; }
};
