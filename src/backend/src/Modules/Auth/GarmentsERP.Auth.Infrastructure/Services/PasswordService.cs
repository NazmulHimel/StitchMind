using System.Text.RegularExpressions;
using GarmentsERP.Auth.Application.Interfaces;

namespace GarmentsERP.Auth.Infrastructure.Services;

public class PasswordService : IPasswordService
{
    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 11);
    }

    public bool Verify(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
            return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }

    public bool MeetsComplexity(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            return false;

        // At least 1 uppercase, 1 lowercase, 1 digit, 1 special character
        var hasUpper = password.Any(char.IsUpper);
        var hasLower = password.Any(char.IsLower);
        var hasDigit = password.Any(char.IsDigit);
        var hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));

        return hasUpper && hasLower && hasDigit && hasSpecial;
    }
}
