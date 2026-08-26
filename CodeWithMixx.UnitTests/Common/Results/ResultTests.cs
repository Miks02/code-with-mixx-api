using AwesomeAssertions;
using CodeWithMixx.API.Common.Result;
using CodeWithMixx.API.Common.Results;
using Xunit;

namespace CodeWithMixx.UnitTests.Common.Results;

public class ResultTests
{
    [Fact]
    public void Success_ShouldReturnSuccessResult()
    {
        var result = Result.Success();
        
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
    
    [Fact]
    public void Failure_ShouldReturnFailureResult()
    {
        var error = new Error("TestCode","TestDescription");
        var result = Result.Failure(error);
        
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(error);
    }
    
    [Fact]
    public void GenericSuccess_ShouldReturnSuccessResultWithPayload()
    {
        var someDto = new { Name = "Test" };
        var result = Result<object>.Success(someDto);
        
        result.IsSuccess.Should().BeTrue();
        result.Payload.Should().BeEquivalentTo(someDto);
        result.Errors.Should().BeEmpty();
    }
    
    [Fact]
    public void GenericFailure_ShouldReturnFailureResult()
    {
        var error = new Error("TestCode","TestDescription");
        var result = Result<object>.Failure(error);
        
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(error);
    }
}