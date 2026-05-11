using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using DraftLite.DTO;

namespace DraftLite.Tests.Integration;

/// <summary>
/// Scénarios HTTP register + « login » (JWT) + « logout » (absence / invalidité du token).
/// </summary>
public sealed class UsersAuthIntegrationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public async Task Register_ValidRequest_Returns200AndUserDto()
    {
        await using var factory = new AuthWebApplicationFactory();
        factory.SeedUserRole();
        var client = factory.CreateClient();

        var email = $"user_{Guid.NewGuid():N}@test.local";
        var payload = new { email, pseudo = "integration", googleId = "google-int-1" };
        var response = await client.PostAsJsonAsync("/users/register", payload, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<UserDto>(JsonOptions);
        dto.Should().NotBeNull();
        dto!.Email.Should().Be(email);
        dto.Pseudo.Should().Be("integration");
        dto.RoleName.Should().Be("User");
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns200AndUpdatesPseudo()
    {
        await using var factory = new AuthWebApplicationFactory();
        factory.SeedUserRole();
        var client = factory.CreateClient();
        var email = $"dup_{Guid.NewGuid():N}@test.local";

        var first = await client.PostAsJsonAsync("/users/register",
            new { email, pseudo = "v1", googleId = (string?)null }, JsonOptions);
        first.StatusCode.Should().Be(HttpStatusCode.OK);

        var second = await client.PostAsJsonAsync("/users/register",
            new { email, pseudo = "v2", googleId = "gid-updated" }, JsonOptions);
        second.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await second.Content.ReadFromJsonAsync<UserDto>(JsonOptions);
        dto!.Pseudo.Should().Be("v2");
        dto.Email.Should().Be(email);
    }

    [Fact]
    public async Task Register_InvalidEmail_Returns500()
    {
        await using var factory = new AuthWebApplicationFactory();
        factory.SeedUserRole();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/users/register",
            new { email = "   ", pseudo = "x" }, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Me_ValidJwt_Returns200()
    {
        await using var factory = new AuthWebApplicationFactory();
        factory.SeedUserRole();
        var client = factory.CreateClient();

        var email = $"me_{Guid.NewGuid():N}@test.local";
        var googleId = $"gid-{Guid.NewGuid():N}";
        var reg = await client.PostAsJsonAsync("/users/register",
            new { email, pseudo = "me-user", googleId }, JsonOptions);
        reg.EnsureSuccessStatusCode();

        var token = IssueTestJwt(googleId, email, role: "User", pseudo: "me-user");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var me = await client.GetAsync("/users");
        me.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await me.Content.ReadFromJsonAsync<UserDto>(JsonOptions);
        body!.Email.Should().Be(email);
        body.Pseudo.Should().Be("me-user");
    }

    [Fact]
    public async Task Me_NoToken_Returns401_LogoutStateless()
    {
        await using var factory = new AuthWebApplicationFactory();
        factory.SeedUserRole();
        var client = factory.CreateClient();

        var me = await client.GetAsync("/users");
        me.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Me_ExpiredToken_Returns401()
    {
        await using var factory = new AuthWebApplicationFactory();
        factory.SeedUserRole();
        var client = factory.CreateClient();

        var token = IssueTestJwt("sub", "e@e.com", role: "User", lifetime: TimeSpan.FromSeconds(-2));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var me = await client.GetAsync("/users");
        me.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Me_WrongSignatureToken_Returns401()
    {
        await using var factory = new AuthWebApplicationFactory();
        factory.SeedUserRole();
        var client = factory.CreateClient();

        var badToken = IssueTestJwt("sub", "e@e.com", role: "User", overrideSecret: "wrong-secret-key-32chars-minimum!!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", badToken);

        var me = await client.GetAsync("/users");
        me.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Me_ValidThenNoHeader_SecondRequest401()
    {
        await using var factory = new AuthWebApplicationFactory();
        factory.SeedUserRole();
        var client = factory.CreateClient();

        var email = $"seq_{Guid.NewGuid():N}@test.local";
        var googleId = $"gid-seq-{Guid.NewGuid():N}";
        await client.PostAsJsonAsync("/users/register",
            new { email, pseudo = "seq", googleId }, JsonOptions);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", IssueTestJwt(googleId, email, "User", "seq"));
        (await client.GetAsync("/users")).StatusCode.Should().Be(HttpStatusCode.OK);

        client.DefaultRequestHeaders.Authorization = null;
        (await client.GetAsync("/users")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private static string IssueTestJwt(
        string subject,
        string email,
        string role,
        string? pseudo = null,
        TimeSpan? lifetime = null,
        string? overrideSecret = null)
    {
        var secret = overrideSecret ?? AuthWebApplicationFactory.TestJwtSecret;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, subject),
            new(JwtRegisteredClaimNames.Email, email),
            new("role", role)
        };
        if (!string.IsNullOrWhiteSpace(pseudo))
            claims.Add(new Claim("pseudo", pseudo));

        var token = new JwtSecurityToken(
            issuer: AuthWebApplicationFactory.TestJwtIssuer,
            audience: AuthWebApplicationFactory.TestJwtAudience,
            claims: claims,
            notBefore: DateTime.UtcNow.AddMinutes(-1),
            expires: DateTime.UtcNow.Add(lifetime ?? TimeSpan.FromHours(1)),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
