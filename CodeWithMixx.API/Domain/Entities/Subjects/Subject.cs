using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Classes;
using CodeWithMixx.API.Domain.Entities.Projects;

namespace CodeWithMixx.API.Domain.Entities.Subjects;

public class Subject : IAuditable, ISoftDeletable
{
    public int Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    private readonly List<Class> _classes = [];
    private readonly List<Project> _projects = [];

    public IReadOnlyCollection<Class> Classes => _classes.AsReadOnly();
    public IReadOnlyCollection<Project> Projects => _projects.AsReadOnly();
    
    private Subject() {}
    
    public static Result<Subject> Create(string name, string description)
    {
        var validationResult = Validate(name, description);
        if (!validationResult.IsSuccess)
            return Result<Subject>.Failure(validationResult.Errors[0]);

        return Result<Subject>.Success(new Subject
        {
            Name = name.Trim(),
            Description = description.Trim()
        });
    }

    public Result Update(string name, string description)
    {
        var validationResult = Validate(name, description);
        if (!validationResult.IsSuccess)
            return validationResult;

        Name = name.Trim();
        Description = description.Trim();
        return Result.Success();
    }

    public Result Delete()
    {
        return Archive();
    }

    public Result Archive()
    {
        if (IsDeleted)
            return Result.Failure(SubjectError.AlreadyArchived(Id));

        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Restore()
    {
        if (!IsDeleted)
            return Result.Failure(SubjectError.NotArchived(Id));

        IsDeleted = false;
        DeletedAt = null;
        return Result.Success();
    }

    private static Result Validate(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(SubjectError.NameRequired());

        if (name.Trim().Length > SubjectConstants.MaxNameLength)
            return Result.Failure(SubjectError.NameTooLong());

        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure(SubjectError.DescriptionRequired());

        if (description.Trim().Length > SubjectConstants.MaxDescriptionLength)
            return Result.Failure(SubjectError.DescriptionTooLong());

        return Result.Success();
    }
}
