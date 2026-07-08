using Microsoft.Extensions.Caching.Memory;
using VacaYAY.Business.Interfaces.Auth;

namespace VacaYAY.Business.Services.Auth;

/// <summary>
/// In-memory <see cref="ITokenDenylist"/> backed by <see cref="IMemoryCache"/>. Revoked
/// token ids are kept only until the token would have expired on its own — after that,
/// normal lifetime validation rejects it anyway.
/// <para>
/// Note: this is per-process. If the API is scaled to multiple instances, replace this
/// with a shared store (database or Redis) so a logout is seen by every instance.
/// </para>
/// </summary>
public class TokenDenylist : ITokenDenylist
{
    private const string KeyPrefix = "revoked-jti:";

    private readonly IMemoryCache _cache;

    public TokenDenylist(IMemoryCache cache)
    {
        _cache = cache;
    }

    public void Revoke(string tokenId, DateTime expiresAtUtc)
    {
        var ttl = expiresAtUtc - DateTime.UtcNow;
        if (ttl <= TimeSpan.Zero)
        {
            return; // already expired — nothing to revoke
        }

        // Absolute expiry ties the cache entry's lifetime to the token's own expiry.
        _cache.Set(KeyPrefix + tokenId, true, new DateTimeOffset(expiresAtUtc, TimeSpan.Zero));
    }

    public bool IsRevoked(string tokenId)
    {
        return _cache.TryGetValue(KeyPrefix + tokenId, out _);
    }
}
