using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DraftLite.DTO.AppSettings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DraftLite.Service.Services;

/// <summary>
/// Issues signed JWT access tokens (HS256) for API authentication in test/integration scenarios.
/// Production may validate Google ID tokens instead; see <c>JwtRoutingSecurity</c>.
/// </summary>
public sealed class JwtService
{
    private readonly JwtSettings _settings;

    public JwtService(IOptions<JwtSettings> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _settings = options.Value;

        if (string.IsNullOrWhiteSpace(_settings.Secret) || _settings.Secret.Length < 32)
            throw new ArgumentException("JWT secret must be at least 32 characters.", nameof(options));
    }

    /// <summary>Numeric subject (legacy / tests).</summary>
    public string GenerateToken(int userId, string email, string role)
        => GenerateToken(
            userId.ToString(CultureInfo.InvariantCulture),
            email,
            role,
            pseudo: null);

    /// <summary>
    /// Subject becomes the <c>sub</c> claim (maps to <see cref="ClaimTypes.NameIdentifier"/> after validation).
    /// Use the user's Google ID when matching <see cref="DraftLite.Service.Services.UserService.GetMeAsync"/>.
    /// </summary>
    public string GenerateToken(string subject, string email, string role, string? pseudo = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(role);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, subject),
            new(JwtRegisteredClaimNames.Email, email),
            new("role", role),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };

        if (!string.IsNullOrWhiteSpace(pseudo))
            claims.Add(new Claim("pseudo", pseudo.Trim()));

        var hours = _settings.ExpiryHours <= 0 ? 1 : _settings.ExpiryHours;
        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddHours(hours),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
