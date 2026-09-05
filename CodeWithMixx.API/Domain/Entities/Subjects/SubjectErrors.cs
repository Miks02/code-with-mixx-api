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
}