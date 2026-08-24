using CodeWithMixx.API.Common.Result;
using Microsoft.AspNetCore.Identity;

namespace CodeWithMixx.API.Common.Results;

public class Result
{
    public bool IsSuccess { get; }
    public IReadOnlyList<Error> Errors { get; }

    protected Result(bool isSuccess, IReadOnlyList<Error> errors)
    {
        IsSuccess = isSuccess;
        Errors = errors;
    }

    public static Result Success() => new(true, []);

    public static Result Failure(params Error[] errors)
    {
        if (errors.Length == 0)
            throw new ArgumentException("At least one error must be provided within a failure");

        return new Result(false, errors.AsReadOnly());

    }

    public static Result Failure(params IdentityError[] identityErrors)
    {
        if (identityErrors.Length == 0)
            throw new ArgumentException("At least one error must be provided within a failure");

        var errors = identityErrors.Select(e => new Error(e.Code, e.Description)).ToList();

        return new Result(false, errors.AsReadOnly());
    }

}

public class Result<T> : Result
{
    public T? Payload { get; }

    private Result(bool isSuccess, IReadOnlyList<Error> errors) : base(isSuccess, errors)
    {
        Payload = default;
    }
    
    private Result(bool isSuccess, IReadOnlyList<Error> errors, T? payload) : base(isSuccess, errors)
    {
        Payload = payload;
    }

    public new static Result<T> Success() => new(true, []);

    public static Result<T> Success(T payload)
    {
        if (payload is null)
            throw new ArgumentNullException(nameof(payload), "Payload cannot be null");

        return new Result<T>(true, [], payload);
    }

    public new static Result<T> Failure(params Error[] errors)
    {
        if(errors.Length == 0)
            throw new ArgumentException("At least one error must be provided within a failure");
        
        return new Result<T>(false, errors.AsReadOnly());
    }

    public new static Result<T> Failure(params IdentityError[] identityErrors)
    {
        if (identityErrors.Length == 0)
            throw new ArgumentException("At least one error must be provided within a failure");

        var errors = identityErrors.Select(e => new Error(e.Code, e.Description)).ToList();

        return new Result<T>(false, errors.AsReadOnly());
    }



}