namespace Identity.Domain.Interfaces;

public interface ITokenRevocationStore
{
    Task RevokeAsync(string jti, TimeSpan ttl, CancellationToken ct = default);

    Task<bool> IsRevokedAsync(string jti, CancellationToken ct = default);
}