using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using GarmentsERP.Auth.Application.Interfaces;
using GarmentsERP.Auth.Domain.Entities;
using GarmentsERP.Shared.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace GarmentsERP.Auth.Infrastructure.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _config;
    private readonly byte[] _keyBytes;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expiryMinutes;

    public JwtTokenService(IConfiguration config)
    {
        _config = config;
        var secret = _config["Jwt:Secret"] ?? "GarmentsERP_SuperSecureSecretKey_ForEnterpriseApplication_2026_Minimum32Bytes!";
        _keyBytes = Encoding.UTF8.GetBytes(secret);
        _issuer = _config["Jwt:Issuer"] ?? "GarmentsERP";
        _audience = _config["Jwt:Audience"] ?? "GarmentsERP.Client";
        _expiryMinutes = int.TryParse(_config["Jwt:ExpiryMinutes"], out var exp) ? exp : AppConstants.JwtAccessTokenExpiryMinutes;
    }

    public JwtTokenResult GenerateTokens(User user, IReadOnlyList<string> permissions)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var expires = DateTime.UtcNow.AddMinutes(_expiryMinutes);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Email, user.Email),
            new("FullName", user.FullName),
            new("CompanyId", user.CompanyId.ToString())
        };

        if (user.DepartmentId.HasValue)
        {
            claims.Add(new Claim("DepartmentId", user.DepartmentId.Value.ToString()));
        }

        foreach (var permission in permissions)
        {
            claims.Add(new Claim("permission", permission));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(_keyBytes), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var accessToken = tokenHandler.WriteToken(token);
        var refreshToken = GenerateRefreshToken();

        return new JwtTokenResult(accessToken, expires, refreshToken);
    }

    public Guid? ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(_keyBytes),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = false // Checked during refresh
            }, out _);

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
        catch
        {
            return null;
        }
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
