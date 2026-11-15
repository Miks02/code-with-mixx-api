using CodeWithMixx.API.Common.Result;

namespace CodeWithMixx.API.Domain.Entities.Students;

public static class StudentError
{
    public static Error NotFound(string? identifier = null)
    {
        string message = identifier is null
            ? "Student not found"
            : $"Student with identifier '{identifier}' is not found";

        return new Error("Student.NotFound", message, ErrorType.NotFound);
    }

    public static Error UserIdRequired()
        => new("Student.UserIdRequired", "Student must be linked to a user", ErrorType.Validation);

    public static Error UniversityTooLong()
        => new("Student.UniversityTooLong", $"University name cannot exceed {StudentConstants.MaxUniversityLength} characters", ErrorType.Validation);

    public static Error AlreadyDeleted(string identifier)
        => new("Student.AlreadyDeleted", $"Student with identifier '{identifier}' is already deleted", ErrorType.Conflict);

    public static Error CannotModifyDeleted(string identifier)
        => new("Student.CannotModifyDeleted", $"Student with identifier '{identifier}' cannot be modified because it is deleted", ErrorType.Conflict);
}
