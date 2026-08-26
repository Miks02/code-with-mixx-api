using System.Net;
using AwesomeAssertions;
using CodeWithMixx.API.Common.Result;
using CodeWithMixx.API.Common.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Xunit;

namespace CodeWithMixx.UnitTests.Common.Results;

public class ResultExtensionsTests
{
    [Theory]
    [InlineData(HttpStatusCode.Created, typeof(Created))]
    [InlineData(HttpStatusCode.OK, typeof(Ok))]
    [InlineData(HttpStatusCode.NoContent, typeof(NoContent))]
    public void ToTypedResult_WhenResultIsSuccess_ShouldReturnCorrectResultType(HttpStatusCode status, Type expectedType)
    {
        var result = Result.Success();
        
        var typedResult = result.ToTypedResult(status);
        
        typedResult.Should().BeOfType(expectedType);
    }
    
    [Theory]
    [InlineData(ErrorType.NotFound, StatusCodes.Status404NotFound)]
    [InlineData(ErrorType.Validation, StatusCodes.Status400BadRequest)]
    [InlineData(ErrorType.Unauthorized, StatusCodes.Status401Unauthorized)]
    [InlineData(ErrorType.Forbidden, StatusCodes.Status403Forbidden)]
    [InlineData(ErrorType.Conflict, StatusCodes.Status409Conflict)]
    [InlineData(ErrorType.TooManyRequests, StatusCodes.Status429TooManyRequests)]
    [InlineData(ErrorType.Failure, StatusCodes.Status500InternalServerError)]
    public void ToTypedResult_WhenResultIsFailure_ShouldReturnProblemDetailsWithCorrectStatusCode(ErrorType errorType, int expectedStatusCode)
    {
        var error = new Error("TestCode","TestDescription", errorType);
        var result = Result.Failure(error);
        
        var typedResult = result.ToTypedResult();
        
        typedResult.Should().BeOfType<ProblemHttpResult>();
        var problemResult = typedResult as ProblemHttpResult;
        problemResult!.StatusCode.Should().Be(expectedStatusCode);
    }
    
    [Fact]
    public void HandleResult_WhenResultIsSuccess_ShouldReturnOkResult()
    {
        var result = Result.Success();
        
        var typedResult = result.ToTypedResult();
        
        typedResult.Should().BeOfType<Ok>();
    }
    
    [Fact]
    public void HandleResult_WhenResultIsFailure_ShouldReturnProblemDetails()
    {
        var error = new Error("TestCode","TestDescription", ErrorType.NotFound);
        var result = Result.Failure(error);
        
        var typedResult = result.ToTypedResult();
        
        typedResult.Should().BeOfType<ProblemHttpResult>();
        var problemResult = typedResult as ProblemHttpResult;
        problemResult!.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }
    
    [Fact]
    public void HandleIdentityResult_WhenResultIsSuccess_ShouldReturnOkResult()
    {
        var result = Result.Success();
        
        var typedResult = result.ToTypedResult();
        
        typedResult.Should().BeOfType<Ok>();
    }
    
    [Fact]
    public void HandleIdentityResult_WhenResultIsFailure_ShouldReturnProblemDetails()
    {
        var identityResult = IdentityResult.Failed(new IdentityError { Code = "TestCode", Description = "TestDescription" });
        
        var result = identityResult.HandleIdentityResult();
        
        var typedResult = result.ToTypedResult();
        
        typedResult.Should().BeOfType<ProblemHttpResult>();
        var problemResult = typedResult as ProblemHttpResult;
        problemResult!.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }
}