using GarmentsERP.Auth.Domain.Entities;

namespace GarmentsERP.Auth.Application.Interfaces;

public record JwtTokenResult(string AccessToken, DateTime Expiry, string RefreshToken);

public interface IJwtTokenService
{
    JwtTokenResult GenerateTokens(User user, IReadOnlyList<string> permissions);
    Guid? ValidateToken(string token);
}
