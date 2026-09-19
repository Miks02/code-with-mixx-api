using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Features.Reservations.Common;
using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Reservations.GetClassReservationsSummary;

public record GetClassReservationsSummaryRequest
{
    [FromQuery(Name = "pageNumber")]
    public int PageNumber { get; init; } = 1;
    [FromQuery(Name = "pageSize")]
    public int PageSize { get; init; } = 10;
    [FromQuery(Name = "reservationStatus")]
    public ReservationStatus? ReservationStatus { get; init; }
    [FromQuery(Name = "paymentStatus")]
    public PaymentStatus? PaymentStatus { get; init; }
    [FromQuery(Name = "subjectIds")]
    public int[] SubjectIds { get; init; } = [];
    [FromQuery(Name = "sortBy")]
    public ClassReservationSortBy SortBy { get; init; } = ClassReservationSortBy.StartDateAscending;
}
