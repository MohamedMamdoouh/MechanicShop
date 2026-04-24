using MechanicShop.Tests.Common.Auth;
using Xunit;
namespace MechanicShop.Domain.UnitTests.Auth;

public class RefreshTokenTests
{
    [Fact]
    public void Create_ShouldSucceed_WithValidData()
    {
        var result = RefreshTokenFactory.Create();

        Assert.True(result.IsSuccess);

        var token = result.Value;

        Assert.NotNull(token);
        Assert.False(string.IsNullOrEmpty(token.Token));
        Assert.True(token.ExpiresOnUtc > DateTime.UtcNow);
    }

    [Fact]
    public void Create_ShouldFail_WhenIdEmpty()
    {
        var result = RefreshTokenFactory.Create(userId: Guid.Empty.ToString());

        Assert.False(result.IsSuccess);

        Assert.Equal("RefreshToken.UserId.Required", result.TopError!.Value.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_ShouldFail_WhenIdInvalid(string? userId)
    {
        var result = RefreshTokenFactory.Create(userId: userId);

        Assert.False(result.IsSuccess);

        Assert.Equal("RefreshToken.UserId.Required", result.TopError!.Value.Code);
    }

    [Fact]
    public void Create_ShouldFail_WhenExpiresOnUtcInPast()
    {
        var result = RefreshTokenFactory.Create(expiresOnUtc: DateTime.UtcNow.AddMinutes(-1));

        Assert.False(result.IsSuccess);

        Assert.Equal("RefreshToken.Expiry.Invalid", result.TopError!.Value.Code);
    }
}