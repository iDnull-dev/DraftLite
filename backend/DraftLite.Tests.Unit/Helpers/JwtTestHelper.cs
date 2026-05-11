using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using DraftLite.DTO.AppSettings;
using DraftLite.Service.Services;

namespace DraftLite.Tests.Unit.Helpers;

/// <summary>
/// Builds a JwtService wired to a predictable test configuration.
/// Also provides helpers to decode and validate returned JWT strings.
/// </summary>
public static class JwtTestHelper
{
    // Must be ≥ 32 chars (256-bit minimum for HMAC-SHA256)
    public const string TestSecret = "super-secret-test-key-32chars-ok!";
    public const string TestIssuer = "DraftLite-api-test";
    public const string TestAudience = "DraftLite-app-test";
    public static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(1);

    public static JwtSettings TestSettings => new()
    {
        Secret = TestSecret,
        Issuer = TestIssuer,
        Audience = TestAudience,
        ExpiryHours = 1
    };

    /// <summary>Creates a real JwtService using the test configuration.</summary>
    public static JwtService CreateService()
        => new(Options.Create(TestSettings));

    // ── Assertion helpers ────────────────────────────────────────────────────

    /// <summary>
    /// Parses a JWT string without signature validation — safe for asserting on claims.
    /// </summary>
    public static ClaimsPrincipal ReadToken(string jwt)
    {
        var handler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestSecret));

        return handler.ValidateToken(jwt, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = true,
            ValidIssuer = TestIssuer,
            ValidateAudience = true,
            ValidAudience = TestAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        }, out _);
    }

    /// <summary>
    /// Returns true when the JWT can be validated with the test key and has not expired.
    /// </summary>
    public static bool IsValid(string jwt)
    {
        try { ReadToken(jwt); return true; }
        catch { return false; }
    }

    /// <summary>Returns the value of a specific claim from the JWT.</summary>
    public static string? GetClaim(string jwt, string claimType)
    {
        var principal = ReadToken(jwt);

        static string? Find(ClaimsPrincipal p, params string[] types)
            => types.Select(t => p.FindFirst(t)?.Value).FirstOrDefault(v => v is not null);

        return claimType switch
        {
            "sub" => Find(principal, JwtRegisteredClaimNames.Sub, ClaimTypes.NameIdentifier),
            "email" => Find(principal, JwtRegisteredClaimNames.Email, ClaimTypes.Email),
            "role" => Find(principal, "role", ClaimTypes.Role),
            "exp" => Find(principal, JwtRegisteredClaimNames.Exp),
            "iss" => Find(principal, JwtRegisteredClaimNames.Iss),
            "aud" => Find(principal, JwtRegisteredClaimNames.Aud),
            _ => Find(principal, claimType)
        };
    }
}
