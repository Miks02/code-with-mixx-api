using AwesomeAssertions;
using CodeWithMixx.API.Common.Result;
using Xunit;

namespace CodeWithMixx.UnitTests.Common.Results;

public class ErrorTests
{
    [Fact]
    public void Error_ShouldInitializePropertiesCorrectly()
    {
        var code = "TestCode";
        var description = "TestDescription";
        var errorType = ErrorType.Validation;

        var error = new Error(code, description, errorType);

        error.Code.Should().Be(code);
        error.Description.Should().Be(description);
        error.Type.Should().Be(errorType);
    }
    
    [Theory]
    [InlineData("", "description")]
    [InlineData("   ", "description")]
    [InlineData(null, "description")]
    public void Error_WithoutCode_ShouldThrowException(string code, string description)
    {
        Func<Error> act = () => new Error(code, description);
        
        act.Should().Throw<ArgumentException>()
            .WithParameterName(nameof(code))
            .WithMessage("*Error code cannot be empty*");
    }
    
    [Theory]
    [InlineData("code", "")]
    [InlineData("code", "   ")]
    [InlineData("code", null)]
    public void Error_WithoutDescription_ShouldThrowException(string code, string description)
    {
        Func<Error> act = () => new Error(code, description);
        
        act.Should().Throw<ArgumentException>()
            .WithParameterName(nameof(description))
            .WithMessage("*Error description cannot be empty*");
    } 
   
}