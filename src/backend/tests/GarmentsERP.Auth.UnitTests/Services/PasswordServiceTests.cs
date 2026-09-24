using GarmentsERP.Auth.Infrastructure.Services;
using Xunit;

namespace GarmentsERP.Auth.UnitTests.Services;

public class PasswordServiceTests
{
    private readonly PasswordService _service = new();

    [Theory]
    [InlineData("Secret@123", true)]
    [InlineData("Pass#2026", true)]
    [InlineData("weak", false)]               // too short
    [InlineData("nocapital123!", false)]      // no uppercase
    [InlineData("NOLOWER123!", false)]        // no lowercase
    [InlineData("NoDigitsAtAll!", false)]     // no digit
    [InlineData("NoSpecial123", false)]       // no special
    public void MeetsComplexity_ShouldValidateCorrectly(string password, bool expected)
    {
        var result = _service.MeetsComplexity(password);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Hash_And_Verify_ShouldWorkProperly()
    {
        var password = "SecureEnterprisePassword#2026";
        var hash = _service.Hash(password);

        Assert.NotNull(hash);
        Assert.NotEqual(password, hash);

        Assert.True(_service.Verify(password, hash));
        Assert.False(_service.Verify("WrongPassword", hash));
    }
}
