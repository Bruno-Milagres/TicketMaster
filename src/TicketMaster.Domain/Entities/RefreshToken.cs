namespace TicketMaster.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; private set; }
    public string UserId { get; private set; }
    public string Token { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => RevokedAt == null && !IsExpired;

    private RefreshToken()
    {
        UserId = string.Empty;
        Token = string.Empty;
    }

    public RefreshToken(string userId, string token, DateTime expiresAt)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Token = token;
        ExpiresAt = expiresAt;
    }

    public void Revoke() => RevokedAt = DateTime.UtcNow;
}
