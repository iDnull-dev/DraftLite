namespace DraftLite.Dto.AppSettings;

public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Secret { get; init; } = null!;
    public string Issuer { get; init; } = null!;
    public string Audience { get; init; } = null!;
}

