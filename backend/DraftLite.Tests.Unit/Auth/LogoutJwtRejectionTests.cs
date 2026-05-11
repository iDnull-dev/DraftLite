using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using DraftLite.Tests.Unit.Helpers;

namespace DraftLite.Tests.Unit.Auth;

/// <summary>
/// Tests for the logout contract.
///
/// Logout in DraftLite is client-side: the Angular app drops the JWT from memory.
/// The server's responsibility is to reject any request that arrives without a valid JWT.
///
/// These tests verify the JWT validation layer — the code that runs before every
/// protected controller action via JWT middleware / [Authorize].
/// </summary>
public class LogoutJwtRejectionTests
{
    // ── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Runs the same TokenValidationParameters your middleware uses.
    /// Returns the ClaimsPrincipal on success, throws SecurityTokenException variants on failure.
    /// </summary>
    private static ClaimsPrincipal Validate(string jwt, bool validateLifetime = true)
    {
        var handler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtTestHelper.TestSecret));

        return handler.ValidateToken(jwt, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = true,
            ValidIssuer = JwtTestHelper.TestIssuer,
            ValidateAudience = true,
            ValidAudience = JwtTestHelper.TestAudience,
            ValidateLifetime = validateLifetime,
            ClockSkew = TimeSpan.Zero     // zero tolerance — mirrors production config
        }, out _);
    }

    private static string IssueToken(string email = "lucas@example.com",
                                     TimeSpan? lifetime = null,
                                     string? overrideSecret = null)
    {
        var secret = overrideSecret ?? JwtTestHelper.TestSecret;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: JwtTestHelper.TestIssuer,
            audience: JwtTestHelper.TestAudience,
            claims: new[] { new Claim("email", email) },
            notBefore: DateTime.UtcNow.AddMinutes(-2),
            expires: DateTime.UtcNow.Add(lifetime ?? TimeSpan.FromHours(1)),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // ── Happy path: a valid JWT must pass ────────────────────────────────────

    [Fact]
    public void ValidJwt_PassesValidation()
    {
        var jwt = IssueToken();
        var act = () => Validate(jwt);
        act.Should().NotThrow();
    }

    // ── Post-logout: no token sent ────────────────────────────────────────────

    [Fact]
    public void NullToken_ThrowsArgumentNullException()
    {
        // Simulates: the client dropped the JWT (logged out) and sends no Authorization header
        Action act = () => Validate(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void EmptyToken_ThrowsSecurityTokenException()
    {
        Action act = () => Validate(string.Empty);
        act.Should().Throw<Exception>(); // handler throws on empty input
    }

    // ── Sad paths: invalid or expired tokens ─────────────────────────────────

    [Fact]
    public void ExpiredToken_ThrowsSecurityTokenExpiredException()
    {
        // Issue a token that expired 1 second ago
        var jwt = IssueToken(lifetime: TimeSpan.FromSeconds(-1));
        Action act = () => Validate(jwt);
        act.Should().Throw<SecurityTokenExpiredException>();
    }

    [Fact]
    public void TokenWithWrongSecret_ThrowsSignatureValidationFailed()
    {
        // Simulates an attacker crafting a JWT with a different secret
        var jwt = IssueToken(overrideSecret: "a-completely-different-secret-key!");
        Action act = () => Validate(jwt);
        act.Should().Throw<SecurityTokenSignatureKeyNotFoundException>();
    }

    // ── Edge / security cases ─────────────────────────────────────────────────

    // [Fact]
    // public void TamperedClaims_ThrowsSignatureValidationFailed()
    // {
    //     // Issue a valid token, then manually flip a character in the payload segment
    //     var jwt = IssueToken();
    //     var parts = jwt.Split('.');

    //     // Corrupt the payload (base64url middle part)
    //     var payloadBytes = Convert.FromBase64String(PadBase64(parts[1]));
    //     payloadBytes[5] ^= 0xFF; // flip bits — breaks signature
    //     parts[1] = Convert.ToBase64String(payloadBytes)
    //         .TrimEnd('=').Replace('+', '-').Replace('/', '_');
    //     var tampered = string.Join('.', parts);

    //     Action act = () => Validate(tampered);
    //     act.Should().Throw<Exception>();
    // }

    [Fact]
    public void TokenWithWrongAudience_ThrowsAudienceValidationFailed()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtTestHelper.TestSecret));
        var token = new JwtSecurityToken(
            issuer: JwtTestHelper.TestIssuer,
            audience: "some-other-app",          // wrong audience
            claims: new[] { new Claim("email", "x@x.com") },
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        Action act = () => Validate(jwt);
        act.Should().Throw<SecurityTokenInvalidAudienceException>();
    }

    [Fact]
    public void TokenWithWrongIssuer_ThrowsIssuerValidationFailed()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtTestHelper.TestSecret));
        var token = new JwtSecurityToken(
            issuer: "rogue-issuer",               // wrong issuer
            audience: JwtTestHelper.TestAudience,
            claims: new[] { new Claim("email", "x@x.com") },
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        Action act = () => Validate(jwt);
        act.Should().Throw<SecurityTokenInvalidIssuerException>();
    }

    [Fact]
    public void JunkString_IsNotAValidToken()
    {
        Action act = () => Validate("not.a.jwt");
        act.Should().Throw<Exception>();
    }

    // ── Private ───────────────────────────────────────────────────────────────

    private static string PadBase64(string base64)
    {
        base64 = base64.Replace('-', '+').Replace('_', '/');
        return (base64.Length % 4) switch
        {
            2 => base64 + "==",
            3 => base64 + "=",
            _ => base64
        };
    }
}
