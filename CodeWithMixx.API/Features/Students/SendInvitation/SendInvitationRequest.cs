using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Students.SendInvitation;

public record SendInvitationRequest
{
    [FromRoute(Name = "id")]
    public string Id { get; init; } = null!;
};