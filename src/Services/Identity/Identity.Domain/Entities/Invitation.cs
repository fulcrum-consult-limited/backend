namespace Identity.Domain.Entities;

public sealed class Invitation : BaseEntity
{
    private static readonly TimeSpan DefaultExpiry = TimeSpan.FromHours(48);

    public Guid UserId { get; private set; }
    public Email Email { get; private set; } = null!;
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public bool IsUsed { get; private set; }
    public DateTime? UsedAt { get; private set; }

    private Invitation() { }
    public static Invitation Create(Guid userId, Email email, TimeSpan? expiry = null)
    {
        return new Invitation
        {
            UserId = userId,
            Email = email,
            Token = GenerateToken(),
            ExpiresAt = DateTime.UtcNow.Add(expiry ?? DefaultExpiry),
            IsUsed = false
        };
    }

    public void Use()
    {
        if (IsUsed)
            throw new InvitationAlreadyUsedException(Id);

        if (DateTime.UtcNow >= ExpiresAt)
            throw new InvitationExpiredException(Id);

        IsUsed = true;
        UsedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    public bool IsExpired() => DateTime.UtcNow >= ExpiresAt;

    public void Invalidate()
    {
        ExpiresAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    private static string GenerateToken() =>
        Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64))
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
}