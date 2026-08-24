using System.Net;
using CodeWithMixx.API.Common.Result;
using CodeWithMixx.API.Domain.Entities.Users;
using CodeWithMixx.API.Domain.ErrorCatalog;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Common.Results;

public static class ResultExtensions
{
    public static IResult ToTypedResult(this Result result, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        if (!result.IsSuccess)
            return ToProblemResult(result.Errors[0]);

        IResult successResult = statusCode switch
        {
            HttpStatusCode.Created => TypedResults.Created(),
            HttpStatusCode.OK => TypedResults.Ok(),
            HttpStatusCode.NoContent => TypedResults.NoContent(),
            _ => TypedResults.Ok()
        };

        return successResult;
    }
    
    public static IResult ToTypedResult<T>(this Result<T> result, HttpStatusCode statusCode = HttpStatusCode.OK, string? location = "" )
    {
        if (!result.IsSuccess)
            return ToProblemResult(result.Errors[0]);

        IResult successResult = statusCode switch
        {
            HttpStatusCode.Created => TypedResults.Created(location, result.Payload),
            HttpStatusCode.OK => TypedResults.Ok(result.Payload),
            HttpStatusCode.NoContent => TypedResults.NoContent(),
            _ => TypedResults.Ok(new {value = result.Payload}),
        };

        return successResult;
    }

    private static IResult ToProblemResult(Error error)
    {
        var status = error switch
        {
            { Type: ErrorType.NotFound } => StatusCodes.Status404NotFound,
            { Type: ErrorType.Validation } => StatusCodes.Status400BadRequest,
            { Type: ErrorType.Unauthorized } => StatusCodes.Status401Unauthorized,
            { Type: ErrorType.Forbidden } => StatusCodes.Status403Forbidden,
            { Type: ErrorType.Conflict } => StatusCodes.Status409Conflict,
            { Type: ErrorType.TooManyRequests} => StatusCodes.Status429TooManyRequests,
            _ => StatusCodes.Status500InternalServerError
        };
        
        return TypedResults.Problem(new ProblemDetails
        {
            Title = $"urn:mixxfit-api:error:{error.Code}",
            Detail = "An error occurred while processing your request.",
            Status = status,
            Instance = $"/errors/{error.Code}",
            Extensions = { ["errorCode"] = error.Code }
        });
    }
    
    public static Result HandleResult(this Result result)
    {
        if (result.IsSuccess)
            return Result.Success();

        return Result.Failure(result.Errors.ToArray());
    }

    public static Result<T> HandleResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return result.Payload is null ? Result<T>.Success() : Result<T>.Success(result.Payload);

        return Result<T>.Failure(result.Errors.ToArray());
    }

    public static Result HandleIdentityResult(this IdentityResult result)
    {
        if (result.Succeeded)
            return Result.Success();

        var errors = ConvertToErrorList(result.Errors);

        return Result.Failure(errors.ToArray());
    }

    public static Result<T> HandleIdentityResult<T>(this IdentityResult result, T data)
    {
        if (result.Succeeded)
            return Result<T>.Success(data);

        var errors = ConvertToErrorList(result.Errors);

        return Result<T>.Failure(errors.ToArray());
    }

    private static Error MapIdentityError(IdentityError error)
    {
        return error.Code switch
        {
            "DuplicateUserName" => UserError.UsernameAlreadyExists(),
            "DuplicateEmail" => UserError.EmailAlreadyExists(),
            "PasswordMismatch" => AuthError.InvalidCurrentPassword(),
            "PasswordTooShort" => AuthError.PasswordTooShort(),
            "PasswordRequiresDigit" => AuthError.PasswordRequiresDigit(),
            "PasswordRequiresUpper" => AuthError.PasswordRequiresUpper(),
            "PasswordRequiresNonAlphanumeric" => AuthError.PasswordRequiresNonAlphanumeric(),
            _ => new Error(error.Code, error.Description)
        };
    }

    private static IReadOnlyList<Error> ConvertToErrorList(IEnumerable<IdentityError> errors)
    {
        return errors.Select(MapIdentityError).ToList();
    }
}