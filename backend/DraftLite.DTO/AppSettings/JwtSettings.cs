namespace DraftLite.DTO.AppSettings;

public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Secret { get; init; } = null!;
    public string Issuer { get; init; } = null!;
    public string Audience { get; init; } = null!;
    /// <summary>Access token lifetime in hours (app-issued JWT).</summary>
    public int ExpiryHours { get; init; } = 1;
}

