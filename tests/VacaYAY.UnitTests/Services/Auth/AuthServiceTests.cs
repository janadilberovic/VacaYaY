using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VacaYAY.Business.DTOs.Auth;
using VacaYAY.Business.Interfaces.Auth;
using VacaYAY.Business.Services.Auth;
using VacaYAY.Data;
using VacaYAY.Domain.Entities;

namespace VacaYAY.UnitTests;

public class AuthServiceTests : IDisposable
{
    private static readonly PasswordHasher<User> Hasher = new();

    private readonly VacaYAYDbContext _db;
    private readonly AuthService _service;
    private readonly RecordingDenylist _denylist;

    // AuthService maps the user to AuthUserDto via Mapster's global config; register it once, as the app does.
    static AuthServiceTests()
    {
        TypeAdapterConfig.GlobalSettings.Scan(typeof(AuthService).Assembly);
    }

    public AuthServiceTests()
    {
        _db = NewDb();
        _denylist = new RecordingDenylist();
        _service = new AuthService(_db, Hasher, new StubTokenService(), _denylist);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task LoginAsync_UnknownEmail_ReturnsNull()
    {
        var result = await _service.LoginAsync(new LoginRequest { Email = "nobody@vacayay.test", Password = "x" });

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_InactiveUser_ReturnsNull()
    {
        // Given
        _db.Users.Add(HashedUser("ana@vacayay.test", "secret", isActive: false));
        await _db.SaveChangesAsync();

        // When
        var result = await _service.LoginAsync(new LoginRequest { Email = "ana@vacayay.test", Password = "secret" });

        // Then
        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_SoftDeletedUser_ReturnsNull()
    {
        // Given — the global query filter hides soft-deleted users from login
        var user = HashedUser("ana@vacayay.test", "secret");
        user.IsDeleted = true;
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // When
        var result = await _service.LoginAsync(new LoginRequest { Email = "ana@vacayay.test", Password = "secret" });

        // Then
        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_CorrectPassword_ReturnsToken()
    {
        // Given
        _db.Users.Add(HashedUser("ana@vacayay.test", "secret"));
        await _db.SaveChangesAsync();

        // When
        var result = await _service.LoginAsync(new LoginRequest { Email = "ana@vacayay.test", Password = "secret" });

        // Then
        Assert.NotNull(result);
        Assert.Equal(StubTokenService.Token, result!.AccessToken);
        Assert.Equal(StubTokenService.Expiry, result.ExpiresAtUtc);
        Assert.False(result.MustChangePassword);
        Assert.Equal("ana@vacayay.test", result.User!.Email);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ReturnsNull()
    {
        // Given
        _db.Users.Add(HashedUser("ana@vacayay.test", "secret"));
        await _db.SaveChangesAsync();

        // When
        var result = await _service.LoginAsync(new LoginRequest { Email = "ana@vacayay.test", Password = "wrong" });

        // Then
        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_FirstLogin_CorrectTempPassword_RequiresPasswordChangeWithoutToken()
    {
        // Given — no hash yet, HR-issued temp password
        _db.Users.Add(TempPasswordUser("ana@vacayay.test", "temp-123"));
        await _db.SaveChangesAsync();

        // When
        var result = await _service.LoginAsync(new LoginRequest { Email = "ana@vacayay.test", Password = "temp-123" });

        // Then — access must be gated behind a real password
        Assert.NotNull(result);
        Assert.True(result!.MustChangePassword);
        Assert.Null(result.AccessToken);
        Assert.Equal("ana@vacayay.test", result.User!.Email);
    }

    [Fact]
    public async Task LoginAsync_FirstLogin_WrongTempPassword_ReturnsNull()
    {
        // Given
        _db.Users.Add(TempPasswordUser("ana@vacayay.test", "temp-123"));
        await _db.SaveChangesAsync();

        // When
        var result = await _service.LoginAsync(new LoginRequest { Email = "ana@vacayay.test", Password = "nope" });

        // Then
        Assert.Null(result);
    }

    [Fact]
    public async Task ChangePasswordAsync_UnknownEmail_ReturnsNull()
    {
        var result = await _service.ChangePasswordAsync(new ChangePasswordRequest
        {
            Email = "nobody@vacayay.test",
            CurrentPassword = "x",
            NewPassword = "new-secret",
        });

        Assert.Null(result);
    }

    [Fact]
    public async Task ChangePasswordAsync_FirstLogin_CorrectTempPassword_SetsHashClearsTempAndReturnsToken()
    {
        // Given
        _db.Users.Add(TempPasswordUser("ana@vacayay.test", "temp-123"));
        await _db.SaveChangesAsync();

        // When
        var result = await _service.ChangePasswordAsync(new ChangePasswordRequest
        {
            Email = "ana@vacayay.test",
            CurrentPassword = "temp-123",
            NewPassword = "new-secret",
        });

        // Then
        Assert.NotNull(result);
        Assert.Equal(StubTokenService.Token, result!.AccessToken);

        var stored = await _db.Users.SingleAsync(u => u.Email == "ana@vacayay.test");
        Assert.Null(stored.TempPassword);
        Assert.NotNull(stored.PasswordHash);
        Assert.Equal(
            PasswordVerificationResult.Success,
            Hasher.VerifyHashedPassword(stored, stored.PasswordHash!, "new-secret"));
    }

    [Fact]
    public async Task ChangePasswordAsync_FirstLogin_WrongTempPassword_ReturnsNull()
    {
        // Given
        _db.Users.Add(TempPasswordUser("ana@vacayay.test", "temp-123"));
        await _db.SaveChangesAsync();

        // When
        var result = await _service.ChangePasswordAsync(new ChangePasswordRequest
        {
            Email = "ana@vacayay.test",
            CurrentPassword = "wrong",
            NewPassword = "new-secret",
        });

        // Then
        Assert.Null(result);
        var stored = await _db.Users.SingleAsync(u => u.Email == "ana@vacayay.test");
        Assert.Equal("temp-123", stored.TempPassword);
        Assert.Null(stored.PasswordHash);
    }

    [Fact]
    public async Task ChangePasswordAsync_NormalUser_CorrectCurrentPassword_UpdatesHashAndReturnsToken()
    {
        // Given
        _db.Users.Add(HashedUser("ana@vacayay.test", "old-secret"));
        await _db.SaveChangesAsync();

        // When
        var result = await _service.ChangePasswordAsync(new ChangePasswordRequest
        {
            Email = "ana@vacayay.test",
            CurrentPassword = "old-secret",
            NewPassword = "new-secret",
        });

        // Then
        Assert.NotNull(result);
        var stored = await _db.Users.SingleAsync(u => u.Email == "ana@vacayay.test");
        Assert.Equal(
            PasswordVerificationResult.Success,
            Hasher.VerifyHashedPassword(stored, stored.PasswordHash!, "new-secret"));
        Assert.Equal(
            PasswordVerificationResult.Failed,
            Hasher.VerifyHashedPassword(stored, stored.PasswordHash!, "old-secret"));
    }

    [Fact]
    public async Task ChangePasswordAsync_NormalUser_WrongCurrentPassword_ReturnsNull()
    {
        // Given
        _db.Users.Add(HashedUser("ana@vacayay.test", "old-secret"));
        await _db.SaveChangesAsync();

        // When
        var result = await _service.ChangePasswordAsync(new ChangePasswordRequest
        {
            Email = "ana@vacayay.test",
            CurrentPassword = "wrong",
            NewPassword = "new-secret",
        });

        // Then
        Assert.Null(result);
    }

    [Fact]
    public async Task LogoutAsync_RevokesTokenOnDenylist()
    {
        // Given
        var expiry = DateTime.UtcNow.AddHours(1);

        // When
        await _service.LogoutAsync("jti-1", expiry);

        // Then
        var revocation = Assert.Single(_denylist.Revocations);
        Assert.Equal("jti-1", revocation.TokenId);
        Assert.Equal(expiry, revocation.ExpiresAtUtc);
    }

    private static User HashedUser(string email, string password, bool isActive = true)
    {
        var user = new User { FirstName = "Ana", LastName = "Zoric", Email = email, IsActive = isActive };
        user.PasswordHash = Hasher.HashPassword(user, password);
        return user;
    }

    private static User TempPasswordUser(string email, string tempPassword) => new()
    {
        FirstName = "Ana",
        LastName = "Zoric",
        Email = email,
        TempPassword = tempPassword,
        IsActive = true,
    };

    private static VacaYAYDbContext NewDb()
    {
        var options = new DbContextOptionsBuilder<VacaYAYDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new VacaYAYDbContext(options);
    }

    private sealed class StubTokenService : ITokenService
    {
        public const string Token = "test-token";
        public static readonly DateTime Expiry = new(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public (string Token, DateTime ExpiresAtUtc) CreateAccessToken(User user) => (Token, Expiry);
    }

    private sealed class RecordingDenylist : ITokenDenylist
    {
        public List<(string TokenId, DateTime ExpiresAtUtc)> Revocations { get; } = new();

        public void Revoke(string tokenId, DateTime expiresAtUtc) => Revocations.Add((tokenId, expiresAtUtc));

        public bool IsRevoked(string tokenId) => Revocations.Exists(r => r.TokenId == tokenId);
    }
}
