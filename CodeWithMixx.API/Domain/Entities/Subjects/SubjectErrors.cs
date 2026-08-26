using CodeWithMixx.API.Common.Result;

namespace CodeWithMixx.API.Domain.Entities.Subjects;

public class SubjectErrors
{
    public static Error NotFound(int? identifier = null)
    {
        string message = identifier == null
            ? "Subject not found"
            : $"Subject with identifier '{identifier}' is not found";

        return new Error("Subject.NotFound", message, ErrorType.NotFound);
    }
    
    public static Error AlreadyExists(string identifier = "")
    {
        string message = string.IsNullOrWhiteSpace(identifier)
            ? "Subject already exists"
            : $"Subject with identifier '{identifier}' already exists";

        return new Error("Subject.AlreadyExists", message, ErrorType.Conflict);
    }
}