using FluentAssertions;
using Microsoft.Extensions.Options;
using DraftLite.DTO.AppSettings;
using DraftLite.Service.Services;
using DraftLite.Tests.Unit.Helpers;
using System.IdentityModel.Tokens.Jwt;

namespace DraftLite.Tests.Unit.Auth;

/// <summary>
/// Tests for JwtService in total isolation — no DB, no Google, no HTTP.
/// Verifies the contract: given a user payload, the service returns a
/// correctly shaped, signed, and scoped JWT.
/// </summary>
public class JwtServiceTests
{
    private static JwtService Create(string? secret = null, int expiryHours = 1)
        => new(Options.Create(new JwtSettings
        {
            Secret = secret ?? JwtTestHelper.TestSecret,
            Issuer = JwtTestHelper.TestIssuer,
            Audience = JwtTestHelper.TestAudience,
            ExpiryHours = expiryHours
        }));

    // ── Happy paths ──────────────────────────────────────────────────────────

    [Fact]
    public void GenerateToken_ReturnsNonEmptyString()
    {
        var service = Create();
        var token = service.GenerateToken(userId: 1, email: "lucas@example.com", role: "User");
        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GenerateToken_IsAValidJwt_ThatPassesValidation()
    {
        var service = Create();
        var token = service.GenerateToken(userId: 1, email: "lucas@example.com", role: "User");
        JwtTestHelper.IsValid(token).Should().BeTrue();
    }

    [Fact]
    public void GenerateToken_ContainsEmailClaim()
    {
        var service = Create();
        var token = service.GenerateToken(userId: 42, email: "test@draftlite.io", role: "User");
        JwtTestHelper.GetClaim(token, "email").Should().Be("test@draftlite.io");
    }

    // [Fact]
    // public void GenerateToken_ContainsRoleClaim()
    // {
    //     var service = Create();
    //     var token = service.GenerateToken(userId: 1, email: "admin@draftlite.io", role: "Admin");
    //     JwtTestHelper.GetClaim(token, "role").Should().Be("Admin");
    // }

    [Fact]
    public void GenerateToken_ContainsUserIdClaim()
    {
        var service = Create();
        var token = service.GenerateToken(userId: 99, email: "x@x.com", role: "User");
        JwtTestHelper.GetClaim(token, JwtRegisteredClaimNames.Sub).Should().Be("99");
    }

    [Fact]
    public void GenerateToken_ExpiryMatchesConfiguredLifetime()
    {
        var service = Create(expiryHours: 2);
        var before = DateTime.UtcNow;
        var token = service.GenerateToken(userId: 1, email: "x@x.com", role: "User");

        var expClaim = JwtTestHelper.GetClaim(token, "exp");
        expClaim.Should().NotBeNull();
        var expiry = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim!)).UtcDateTime;

        expiry.Should().BeCloseTo(before.AddHours(2), precision: TimeSpan.FromSeconds(5));
    }

    // [Fact]
    // public void GenerateToken_TwoCalls_ProduceDifferentTokens()
    // {
    //     // iat (issued-at) differs by at least a few milliseconds between calls
    //     var service = Create();
    //     var t1 = service.GenerateToken(userId: 1, email: "x@x.com", role: "User");
    //     var t2 = service.GenerateToken(userId: 1, email: "x@x.com", role: "User");
    //     t1.Should().NotBe(t2);
    // }

    // ── Sad / edge paths ─────────────────────────────────────────────────────

    [Fact]
    public void Constructor_SecretTooShort_ThrowsOnStartup()
    {
        // HMAC-SHA256 requires ≥ 256 bits (32 chars in ASCII)
        Action act = () => Create(secret: "too-short");
        act.Should().Throw<ArgumentException>()
            .WithMessage("*secret*");
    }

    [Fact]
    public void GenerateToken_NullEmail_ThrowsArgumentException()
    {
        var service = Create();
        Action act = () => service.GenerateToken(userId: 1, email: null!, role: "User");
        act.Should().Throw<ArgumentException>();
    }
}
