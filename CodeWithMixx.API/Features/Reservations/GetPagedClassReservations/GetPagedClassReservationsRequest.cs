using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Features.Reservations.Common;
using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Reservations.GetPagedClassReservations;

public record GetPagedClassReservationsRequest
{
    [FromQuery(Name = "pageNumber")]
    public int PageNumber { get; init; } = 1;
    [FromQuery(Name = "pageSize")]
    public int PageSize { get; init; } = 10;
    [FromQuery(Name = "subjectIds")]
    public int[] SubjectIds { get; init; } = [];
    [FromQuery(Name = "reservationStatuses")]
    public ReservationStatusFilter[] ReservationStatuses { get; init; } = [];
    [FromQuery(Name = "paymentStatuses")]
    public PaymentStatus[] PaymentStatuses { get; init; } = [];
    [FromQuery(Name = "showOnlyDeletedStudents")]
    public bool ShowOnlyDeletedStudents { get; init; }
    [FromQuery(Name = "showOnlyDeletedReservations")]
    public bool ShowOnlyDeletedReservations { get; init; }
    [FromQuery(Name = "sortBy")]
    public ClassReservationSortBy SortBy { get; init; } = ClassReservationSortBy.StartDateAscending;
}
