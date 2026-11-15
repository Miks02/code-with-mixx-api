using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Domain.Entities.Users;

namespace CodeWithMixx.API.Domain.Entities.Students;

public class Student : ISoftDeletable
{
    public User User { get; private set; } = null!;
    public string UserId { get; private set; } = null!;

    public string? University { get; private set; }

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    private readonly List<Reservation> _reservations = [];
    public IReadOnlyCollection<Reservation> Reservations => _reservations.AsReadOnly();

    private Student() {}

    public static Result<Student> Create(string userId, string? university)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Result<Student>.Failure(StudentError.UserIdRequired());

        var universityResult = NormalizeUniversity(university);
        if (!universityResult.IsSuccess)
            return Result<Student>.Failure(universityResult.Errors[0]);

        return Result<Student>.Success(new Student
        {
            UserId = userId,
            University = universityResult.Payload
        });
    }

    public Result ChangeUniversity(string? newUniversity)
    {
        if (IsDeleted)
            return Result.Failure(StudentError.CannotModifyDeleted(UserId));

        var universityResult = NormalizeUniversity(newUniversity);
        if (!universityResult.IsSuccess)
            return Result.Failure(universityResult.Errors[0]);

        University = universityResult.Payload;
        return Result.Success();
    }

    public Result Delete()
    {
        if (IsDeleted)
            return Result.Failure(StudentError.AlreadyDeleted(UserId));

        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        University = null;
        return Result.Success();
    }

    // Blank becomes null (no university); otherwise trimmed and length-checked.
    private static Result<string?> NormalizeUniversity(string? university)
    {
        if (string.IsNullOrWhiteSpace(university))
            return Result<string?>.Success();

        var trimmed = university.Trim();
        if (trimmed.Length > StudentConstants.MaxUniversityLength)
            return Result<string?>.Failure(StudentError.UniversityTooLong());

        return Result<string?>.Success(trimmed);
    }
}
