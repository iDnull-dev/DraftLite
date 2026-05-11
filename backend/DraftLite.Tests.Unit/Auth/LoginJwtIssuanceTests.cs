using FluentAssertions;
using DraftLite.Service.Services;
using DraftLite.Tests.Unit.Helpers;

namespace DraftLite.Tests.Unit.Auth;

/// <summary>
/// "Login" côté API DraftLite = obtenir un JWT porteur pour appeler les routes <c>[Authorize]</c>.
/// Il n'y a pas d'endpoint HTTP <c>/auth/login</c> dans le contrat actuel : ces tests couvrent
/// l'émission du token (<see cref="JwtService"/>) alignée avec la validation symétrique utilisée en <c>IntegrationTest</c>.
/// </summary>
public sealed class LoginJwtIssuanceTests
{
    private readonly JwtService _jwt = JwtTestHelper.CreateService();

    [Fact]
    public void Login_IssuedJwt_IsValid()
    {
        var token = _jwt.GenerateToken(subject: "google-login-1", email: "u@example.com", role: "User", pseudo: "u1");
        JwtTestHelper.IsValid(token).Should().BeTrue();
    }

    [Fact]
    public void Login_IssuedJwt_ContainsEmailPseudoRoleAndSubClaims()
    {
        var token = _jwt.GenerateToken(
            subject: "google-456",
            email: "dev@draftlite.io",
            role: "User",
            pseudo: "devlucas");

        JwtTestHelper.GetClaim(token, "email").Should().Be("dev@draftlite.io");
        JwtTestHelper.GetClaim(token, "pseudo").Should().Be("devlucas");
        JwtTestHelper.GetClaim(token, "role").Should().Be("User");
        JwtTestHelper.GetClaim(token, "sub").Should().Be("google-456");
    }

    [Fact]
    public void Login_IssuedJwt_HasIssuerAndAudienceFromSettings()
    {
        var token = _jwt.GenerateToken("sub-x", "x@x.com", "User");
        var principal = JwtTestHelper.ReadToken(token);
        principal.FindFirst("iss")?.Value.Should().Be(JwtTestHelper.TestIssuer);
        principal.FindFirst("aud")?.Value.Should().Be(JwtTestHelper.TestAudience);
    }

    [Fact]
    public void Login_IssuedJwt_ExpiresAfterConfiguredLifetime()
    {
        var before = DateTime.UtcNow;
        var token = _jwt.GenerateToken("sub-y", "y@y.com", "User");
        var after = DateTime.UtcNow;

        var expClaim = JwtTestHelper.GetClaim(token, "exp");
        var expiry = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim!)).UtcDateTime;
        expiry.Should().BeCloseTo(before.Add(JwtTestHelper.TokenLifetime), precision: TimeSpan.FromSeconds(5));
        expiry.Should().BeBefore(after.Add(JwtTestHelper.TokenLifetime).AddSeconds(2));
    }

    [Fact]
    public async Task Login_ConcurrentIssuance_BothTokensValid()
    {
        var tasks = Enumerable.Range(0, 2)
            .Select(_ => Task.Run(() => _jwt.GenerateToken("google-concurrent", "c@x.com", "User")));
        var tokens = await Task.WhenAll(tasks);
        tokens.Should().AllSatisfy(t => JwtTestHelper.IsValid(t).Should().BeTrue());
    }
}
