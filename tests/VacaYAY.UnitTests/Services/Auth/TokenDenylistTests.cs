using Microsoft.Extensions.Caching.Memory;
using VacaYAY.Business.Services.Auth;

namespace VacaYAY.UnitTests;

public class TokenDenylistTests : IDisposable
{
    private readonly MemoryCache _cache;
    private readonly TokenDenylist _denylist;

    public TokenDenylistTests()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
        _denylist = new TokenDenylist(_cache);
    }

    public void Dispose() => _cache.Dispose();

    [Fact]
    public void IsRevoked_ForUnknownToken_ReturnsFalse()
    {
        Assert.False(_denylist.IsRevoked("never-seen"));
    }

    [Fact]
    public void Revoke_ThenIsRevoked_ReturnsTrue()
    {
        // When
        _denylist.Revoke("jti-1", DateTime.UtcNow.AddHours(1));

        // Then
        Assert.True(_denylist.IsRevoked("jti-1"));
    }

    [Fact]
    public void Revoke_DoesNotAffectOtherTokens()
    {
        // When
        _denylist.Revoke("jti-1", DateTime.UtcNow.AddHours(1));

        // Then
        Assert.False(_denylist.IsRevoked("jti-2"));
    }

    [Fact]
    public void Revoke_WithAlreadyExpiredToken_IsNotStored()
    {
        // When — token whose lifetime is already over has nothing to revoke
        _denylist.Revoke("jti-old", DateTime.UtcNow.AddMinutes(-1));

        // Then
        Assert.False(_denylist.IsRevoked("jti-old"));
    }
}
