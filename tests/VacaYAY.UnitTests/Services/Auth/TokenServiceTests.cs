using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using VacaYAY.Business.DTOs.Auth;
using VacaYAY.Business.Services.Auth;
using VacaYAY.Domain.Entities;

namespace VacaYAY.UnitTests;

public class TokenServiceTests
{
    private const string SigningKey = "0123456789ABCDEF0123456789ABCDEF";

    private readonly TokenService _service;

    public TokenServiceTests()
    {
        var options = Options.Create(new JwtOptions
        {
            Issuer = "vacayay-issuer",
            Audience = "vacayay-audience",
            SigningKey = SigningKey,
            AccessTokenHours = 4,
        });
        _service = new TokenService(options);
    }

    [Fact]
    public void CreateAccessToken_EmbedsUserIdentityClaims()
    {
        // Given
        var user = new User { Id = 42, Email = "hr@vacayay.test", Role = UserRole.HR };

        // When
        var (token, _) = _service.CreateAccessToken(user);

        // Then
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Equal("42", jwt.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal("hr@vacayay.test", jwt.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.Equal("HR", jwt.Claims.Single(c => c.Type == ClaimTypes.Role).Value);
    }

    [Fact]
    public void CreateAccessToken_SetsIssuerAndAudienceFromOptions()
    {
        // When
        var (token, _) = _service.CreateAccessToken(new User { Id = 1, Email = "a@b.test", Role = UserRole.Employee });

        // Then
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Equal("vacayay-issuer", jwt.Issuer);
        Assert.Contains("vacayay-audience", jwt.Audiences);
    }

    [Fact]
    public void CreateAccessToken_ExpiresAfterConfiguredHours()
    {
        // Given
        var before = DateTime.UtcNow;

        // When
        var (_, expiresAtUtc) = _service.CreateAccessToken(new User { Id = 1, Email = "a@b.test", Role = UserRole.Employee });

        // Then — AccessTokenHours is 4; allow slack for the call itself
        Assert.InRange(expiresAtUtc, before.AddHours(4), DateTime.UtcNow.AddHours(4).AddSeconds(5));
    }

    [Fact]
    public void CreateAccessToken_GeneratesUniqueJtiPerCall()
    {
        // Given
        var user = new User { Id = 1, Email = "a@b.test", Role = UserRole.Employee };

        // When
        var jtiA = JtiOf(_service.CreateAccessToken(user).Token);
        var jtiB = JtiOf(_service.CreateAccessToken(user).Token);

        // Then
        Assert.NotEqual(jtiA, jtiB);
    }

    private static string JtiOf(string token) =>
        new JwtSecurityTokenHandler().ReadJwtToken(token)
            .Claims.Single(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
}
