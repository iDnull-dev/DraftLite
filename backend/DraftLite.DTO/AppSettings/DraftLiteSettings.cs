namespace DraftLite.Dto.AppSettings;

public sealed class DraftLiteSettings
{
    public const string SectionName = "DraftLite";

    public string Default_user_role { get; init; } = null!;
}

