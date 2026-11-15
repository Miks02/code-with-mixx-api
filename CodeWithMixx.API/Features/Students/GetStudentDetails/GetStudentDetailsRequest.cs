using CodeWithMixx.API.Domain.Entities.Reservations;
using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Students.GetStudentDetails;

public record GetStudentDetailsRequest
{
    [FromRoute(Name = "id")]
    public string Id { get; init; } = null!;
    [FromQuery(Name = "pageNumber")]
    public int PageNumber { get; init; } = 1;
    [FromQuery(Name = "pageSize")]
    public int PageSize { get; init; } = 10;
    [FromQuery(Name = "reservationType")]
    public ReservationType? ReservationType { get; init; }
}
