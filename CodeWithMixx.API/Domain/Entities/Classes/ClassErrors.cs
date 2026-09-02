using CodeWithMixx.API.Common.Result;

namespace CodeWithMixx.API.Domain.Entities.Classes
{
    public static class ClassError
    {
        public static Error NotFound(int? identifier = null)
        {
            string message = identifier is null
                ? "Class not found"
                : $"Class with identifier '{identifier}' is not found";

            return new Error("Class.NotFound", message, ErrorType.NotFound);
        }

        public static Error InvalidSchedule(DateTime startsAt, DateTime endsAt)
            => new("Class.InvalidSchedule", $"Class schedule is invalid: starts at '{startsAt:u}' and ends at '{endsAt:u}'", ErrorType.Validation);
    }
}
