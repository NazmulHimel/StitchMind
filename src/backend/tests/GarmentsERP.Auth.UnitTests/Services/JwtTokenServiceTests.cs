using GarmentsERP.Auth.Domain.Entities;
using GarmentsERP.Auth.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace GarmentsERP.Auth.UnitTests.Services;

public class JwtTokenServiceTests
{
    private readonly JwtTokenService _service;

    public JwtTokenServiceTests()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"Jwt:Secret", "A_Very_Long_Super_Secure_Enterprise_Key_Minimum_32_Bytes_Long_2026!"},
            {"Jwt:Issuer", "GarmentsERP.Tests"},
            {"Jwt:Audience", "GarmentsERP.Client"},
            {"Jwt:ExpiryMinutes", "15"}
        };

        IConfiguration config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _service = new JwtTokenService(config);
    }

    [Fact]
    public void GenerateTokens_ShouldReturnValidTokens()
    {
        var user = User.Create("Admin User", "admin", "admin@test.com", "hash", Guid.NewGuid());
        var permissions = new[] { "Procurement.Read", "Inventory.Create" };

        var result = _service.GenerateTokens(user, permissions);

        Assert.NotNull(result.AccessToken);
        Assert.NotNull(result.RefreshToken);
        Assert.True(result.Expiry > DateTime.UtcNow);

        var validatedUserId = _service.ValidateToken(result.AccessToken);
        Assert.Equal(user.Id, validatedUserId);
    }
}
