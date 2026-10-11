namespace CodeWithMixx.API.Features.Reservations.Common;

public static class ReservationStatusFilterResolver
{
    private static readonly Dictionary<ReservationStatusFilter, List<ReservationStatusFilter>> ConflictingFilters = new()
        {
            [ReservationStatusFilter.Confirmed] = [ReservationStatusFilter.Cancelled],
            [ReservationStatusFilter.Completed] = [ReservationStatusFilter.Cancelled],
            [ReservationStatusFilter.Cancelled] = [ReservationStatusFilter.Confirmed, ReservationStatusFilter.Completed],
        };

    public static IReadOnlyList<ReservationStatusFilter> GetInvalidFilters(IReadOnlyList<ReservationStatusFilter> selectedFilters)
    {
        List<ReservationStatusFilter> invalidFilters = [];

        foreach (var filter in selectedFilters)
        {
            if (invalidFilters.Contains(filter))
                continue;

            if (ConflictingFilters.TryGetValue(filter, out var conflicts))
                invalidFilters.AddRange(conflicts);
        }

        return invalidFilters;
    }
}
