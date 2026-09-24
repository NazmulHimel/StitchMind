using GarmentsERP.Auth.Domain.Entities;
using Xunit;

namespace GarmentsERP.Auth.UnitTests.Domain;

public class UserEntityTests
{
    [Fact]
    public void Create_ShouldNormalizeUserNameAndEmail()
    {
        var companyId = Guid.NewGuid();
        var user = User.Create(" John Doe ", " JohnD ", " JOHN@EXAMPLE.COM ", "hashed_pw", companyId, "+123456789");

        Assert.Equal("John Doe", user.FullName);
        Assert.Equal("johnd", user.UserName);
        Assert.Equal("john@example.com", user.Email);
        Assert.Equal(companyId, user.CompanyId);
        Assert.True(user.IsActive);
        Assert.False(user.IsLockedOut());
        Assert.Equal(0, user.FailedLoginAttempts);
    }

    [Fact]
    public void RecordFailedLogin_ShouldLockout_WhenMaxAttemptsReached()
    {
        var user = User.Create("Admin", "admin", "admin@example.com", "hash", Guid.NewGuid());

        user.RecordFailedLogin(maxAttempts: 3, lockoutMinutes: 15);
        Assert.False(user.IsLockedOut());
        Assert.Equal(1, user.FailedLoginAttempts);

        user.RecordFailedLogin(maxAttempts: 3, lockoutMinutes: 15);
        Assert.False(user.IsLockedOut());
        Assert.Equal(2, user.FailedLoginAttempts);

        user.RecordFailedLogin(maxAttempts: 3, lockoutMinutes: 15);
        Assert.True(user.IsLockedOut());
        Assert.Equal(3, user.FailedLoginAttempts);
    }

    [Fact]
    public void RecordSuccessfulLogin_ShouldResetFailedAttemptsAndLockout()
    {
        var user = User.Create("Admin", "admin", "admin@example.com", "hash", Guid.NewGuid());
        user.RecordFailedLogin(maxAttempts: 2, lockoutMinutes: 30);
        user.RecordFailedLogin(maxAttempts: 2, lockoutMinutes: 30);
        Assert.True(user.IsLockedOut());

        user.RecordSuccessfulLogin();

        Assert.False(user.IsLockedOut());
        Assert.Equal(0, user.FailedLoginAttempts);
        Assert.NotNull(user.LastLoginAt);
    }

    [Fact]
    public void ChangePassword_ShouldUpdateHashAndResetLockout()
    {
        var user = User.Create("Admin", "admin", "admin@example.com", "hash1", Guid.NewGuid());
        user.RecordFailedLogin(maxAttempts: 1, lockoutMinutes: 30);
        Assert.True(user.IsLockedOut());

        user.ChangePassword("hash2");

        Assert.Equal("hash2", user.PasswordHash);
        Assert.False(user.IsLockedOut());
        Assert.Equal(0, user.FailedLoginAttempts);
        Assert.NotNull(user.PasswordChangedAt);
    }
}
