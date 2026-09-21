namespace CodeWithMixx.API.Features.Students.Common
{
    public static class StudentsFilterResolver
    {
        private static readonly Dictionary<StudentsFilterBy, List<StudentsFilterBy>> ConflictingFilters = new()
            {
                [StudentsFilterBy.WithClasses] = [StudentsFilterBy.WithoutClasses],
                [StudentsFilterBy.WithoutClasses] = [StudentsFilterBy.WithClasses],
                [StudentsFilterBy.WithProjects] = [StudentsFilterBy.WithoutProjects],
                [StudentsFilterBy.WithoutProjects] = [StudentsFilterBy.WithProjects],
            };

        public static IReadOnlyList<StudentsFilterBy> GetInvalidFilters(IReadOnlyList<StudentsFilterBy> selectedFilters)
        {
            List<StudentsFilterBy> invalidFilters = [];

            foreach (var filter in selectedFilters)
            {
                if (invalidFilters.Contains(filter))
                    continue;

                if (ConflictingFilters.TryGetValue(filter, out var conflicts))
                {
                    invalidFilters.AddRange(conflicts);
                }
            }

            return invalidFilters;
        }
    }
}
