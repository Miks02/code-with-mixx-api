using CodeWithMixx.API.Features.Reservations.Common;
using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Reservations.GetProjectReservationsSummary;

public record GetProjectReservationsSummaryRequest
{
    [FromQuery(Name = "pageNumber")]
    public int PageNumber { get; init; } = 1;
    [FromQuery(Name = "pageSize")]
    public int PageSize { get; init; } = 15;
    [FromQuery(Name = "subjectIds")]
    public int[] SubjectIds { get; init; } = [];
    [FromQuery(Name = "projectTypes")]
    public ProjectTypeFilter[] ProjectTypes { get; init; } = [];
    [FromQuery(Name = "reservationStatuses")]
    public ReservationStatusFilter[] ReservationStatuses { get; init; } = [];
};
