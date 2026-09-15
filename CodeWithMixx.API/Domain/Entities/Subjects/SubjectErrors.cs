using CodeWithMixx.API.Common.Result;

namespace CodeWithMixx.API.Domain.Entities.Subjects;

public static class SubjectError
{
    public static Error NotFound(int? identifier = null)
    {
        string message = identifier == null
            ? "Subject not found"
            : $"Subject with identifier '{identifier}' is not found";

        return new Error("Subject.NotFound", message, ErrorType.NotFound);
    }
    
    public static Error MultipleSubjectsMissing(IReadOnlyList<int> identifiers)
    {
        string message = $"Subjects with identifiers '{string.Join(", ", identifiers)}' are not found";
        return new Error("Subject.MultipleSubjectsMissing", message, ErrorType.NotFound);
    }
    
    public static Error AlreadyExists(string identifier = "")
    {
        string message = string.IsNullOrWhiteSpace(identifier)
            ? "Subject already exists"
            : $"Subject with identifier '{identifier}' already exists";

        return new Error("Subject.AlreadyExists", message, ErrorType.Conflict);
    }

    public static Error AlreadyArchived(int? identifier = null)
    {
        string message = identifier == null
            ? "Subject is already archived"
            : $"Subject with identifier '{identifier}' is already archived";

        return new Error("Subject.AlreadyArchived", message, ErrorType.Conflict);
    }

    public static Error NotArchived(int? identifier = null)
    {
        string message = identifier == null
            ? "Subject is not archived"
            : $"Subject with identifier '{identifier}' is not archived";

        return new Error("Subject.NotArchived", message, ErrorType.Conflict);
    }
    
    public static Error HasAssociatedClasses(int? identifier = null)
    {
        string message = identifier == null
            ? "Subject has classes associated with it"
            : $"Subject with identifier '{identifier}' has classes associated with it";

        return new Error("Subject.HasClasses", message, ErrorType.Conflict);
    }
}