using CodeWithMixx.API.Common.Result;
using CodeWithMixx.API.Common.Results;

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
}
