using GarmentsERP.Auth.Domain.Entities;
using Xunit;

namespace GarmentsERP.Auth.UnitTests.Domain;

public class RefreshTokenEntityTests
{
    [Fact]
    public void Create_ShouldBeActive()
    {
        var userId = Guid.NewGuid();
        var token = RefreshToken.Create(userId, "test-token-123", 7, "Mozilla/5.0", "127.0.0.1");

        Assert.Equal(userId, token.UserId);
        Assert.Equal("test-token-123", token.Token);
        Assert.True(token.IsActive());
        Assert.False(token.IsRevoked);
        Assert.False(token.IsExpired());
    }

    [Fact]
    public void Revoke_ShouldMakeTokenInactive()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), "test-token", 7);
        Assert.True(token.IsActive());

        token.Revoke();

        Assert.True(token.IsRevoked);
        Assert.False(token.IsActive());
    }
}
