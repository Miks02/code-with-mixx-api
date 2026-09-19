using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Classes.RestoreClass;

public record RestoreClassRequest
{
    [FromRoute]
    public int ClassId { get; init; }
};
