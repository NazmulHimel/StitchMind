using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Auth.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; } = false;
    public string? DeviceInfo { get; private set; }
    public string? IpAddress { get; private set; }
    public User User { get; private set; } = null!;

    private RefreshToken() { }

    public static RefreshToken Create(Guid userId, string token, int expiryDays,
        string? deviceInfo = null, string? ipAddress = null)
        => new()
        {
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
            DeviceInfo = deviceInfo,
            IpAddress = ipAddress
        };

    public bool IsExpired() => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive()  => !IsRevoked && !IsExpired();
    public void Revoke()    { IsRevoked = true; UpdatedAt = DateTime.UtcNow; }
}
