namespace AgoraHub360.ERP.Tests.Application;

using AgoraHub360.ERP.Application.Common;

public class ResultTests
{
    [Fact]
    public void Result_Success_ReturnsValueAndIsSuccessTrue()
    {
        var result = Result<string>.Success("test");

        Assert.True(result.IsSuccess);
        Assert.Equal("test", result.Value);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Result_Failure_ReturnsErrorAndIsSuccessFalse()
    {
        var result = Result<string>.Failure("something went wrong");

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal("something went wrong", result.Error);
    }
}
