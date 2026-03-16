namespace Identity.Infrastructure.Redis;

public sealed class RedisTokenRevocationStore(IConnectionMultiplexer redis) : ITokenRevocationStore
{
    private readonly IDatabase _db = redis.GetDatabase();
    private const string KeyPrefix = "revoked_jwt:";

    public async Task RevokeAsync(string jti, TimeSpan ttl, CancellationToken ct = default) =>
        await _db.StringSetAsync(
            $"{KeyPrefix}{jti}",
            "1",
            ttl);

    public async Task<bool> IsRevokedAsync(string jti, CancellationToken ct = default) =>
        await _db.KeyExistsAsync($"{KeyPrefix}{jti}");
}